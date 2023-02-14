using System.Net;

using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Server;

/// <summary>   A VXI-11 server capable of serving multiple clients. </summary>
/// <remarks> Implements the minimum requirements for a VXI-11 including an <see cref="AbortChannelServer"/>
/// and <see cref="InterruptChannelClient"/></remarks>
public partial class Vxi11MultiClientServer : Vxi11Server
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public Vxi11MultiClientServer() : this( new Vxi11Device( new Vxi11Instrument(), new Vxi11Interface() ),
                                            IPAddress.Any, 0 )
    {
        throw new NotImplementedException();
    }


    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Vxi11MultiClientServer( IVxi11Device device, IPAddress bindAddr, int port = 0 ) : base( device, bindAddr, port )
    {
        throw new NotImplementedException();
    }

    #endregion

    #region " RPC call management for multiple clients "

    private readonly object _lock = new();

    /// <summary>   Create a device connection; Opens a link to a device. </summary>
    /// <remarks>
    /// To successfully complete a create_link RPC, a network instrument server SHALL: <para>
    /// 1. If lockDevice is set to true, acquire the lock for the device. </para><para>
    /// 2. Return in <c>link id</c> a link identifier to be used with future calls. The value of <c>
    /// link id</c> SHALL be unique for all currently active links within a network instrument
    /// server.  </para><para>
    /// 3. Return in maxRecvSize the size of the largest data parameter the network instrument server
    /// can accept in a <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  RPC.This value SHALL be at least 1024.  </para><para>
    /// 4. Return in asyncPort the port number for asynchronous RPCs. See device_abort.  </para><para>
    /// 5. Return with error set to 0, no error, to indicate successful completion.  </para><para>
    /// 
    /// The device parameter is a string which identifies the device for communications.See the
    /// document(s) referred to in section A.6, Related Documents, for definitions of this string.  </para>
    /// <para>
    /// 
    /// A network instrument server should be able to maintain at least two separate links
    /// simultaneously over a single network instrument connection. </para><para>
    /// 
    /// The network instrument client sends an identifying number in the clientId parameter. While
    /// this protocol requires no special behavior based on the value of clientId, the device may
    /// provide a local means to examine its value to help a user identify communication problems. </para>
    /// <para>
    /// 
    /// The network instrument server SHALL NOT alter its function based on the clientId. </para><para>
    /// 
    /// If create_link is called when another link is not available, create_link SHALL terminate and
    /// set error to 9. </para><para>
    /// 
    /// The operation of create_link SHALL ignore locks if lockDevice is false. </para><para>
    /// If lockDevice is true and the lock is not freed after at least <c>lock_timeout</c>
    /// milliseconds, create_link SHALL terminate without creating a link and return with error set
    /// to 11, device locked by another link. </para><para>
    /// 
    /// The execution of create_link SHALL have no effect on the state of any device associated with
    /// the network instrument server. </para><para>
    /// 
    /// A create_link RPC cannot be aborted since a valid link identifier is not yet available.A
    /// network instrument client should set <c>lock_timeout</c> to a reasonable value to avoid
    /// locking up the server. </para>
    /// </remarks>
    /// <param name="request">  The request of type <see cref="CreateLinkParms"/> to use with the
    ///                         remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="CreateLinkResp"/> to send to the remote procedure call.
    /// </returns>
    public override CreateLinkResp CreateLink( CreateLinkParms request )
    {
        lock( this._lock )
        {
            return this.Device?.AwaitLockReleaseAsync() ?? true
                ? base.CreateLink( request )
                : new CreateLinkResp() { ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink };
        }
    }

    /// <summary>   Destroy a connection. </summary>
    /// <remarks>
    /// To successfully complete a destroy_link RPC, a network instrument server SHALL: <para>
    /// 1. Deactivate the link identifier and recover any resources associated with the link. </para><para>
    /// 2. If this link has the lock, free the lock (see <c>device_lock</c> and create_link). </para><para>
    /// 3. Disable this link from using the interrupt mechanism (see <c>device_enable_srq</c>). </para>
    /// <para>
    /// 4. Return with error set to 0, no error, to indicate successful completion. </para><para>
    /// The Device_Link( link identifier ) parameter is compared against the active link identifiers.
    /// If none match, destroy_link SHALL terminate and set error to 4. </para><para>
    /// 
    /// After a destroy_link, the network instrument server typically becomes ready to execute a new
    /// create_link, assuming the resources have not already been utilized. </para><para>
    /// 
    /// The execution of destroy_link SHALL have no effect on the state of any device associated with
    /// the network instrument server. </para><para>
    /// 
    /// The operation of destroy_link SHALL NOT be affected by device_abort. </para>
    /// </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceLink"/> to use with the
    ///                         remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DestroyLink( DeviceLink request )
    {
        lock ( this._lock )
        {
            return base.DestroyLink( request );
        }
    }

    /// <summary>   Device clear. </summary>
    /// <remarks>
    /// Since not all devices directly support a clear operation, how this operation is executed
    /// depends upon the interface between the network instrument server and the device. <para>
    /// If the device does not support a clear operation and the network instrument server is able to
    /// detect this, device_clear SHALL terminate and set error to 8, operation not supported. </para>
    /// <para>
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match,
    /// device_clear SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// If some other link has the lock, device_clear SHALL examine the <see cref="DeviceOperationFlags.Waitlock"/> flag in <c>
    /// flags</c> . If the flag is set, device_clear SHALL block until the lock is free. If the flag
    /// is not set, device_clear SHALL terminate with error set to 11, device locked by another link.
    /// </para><para>
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, device_clear SHALL
    /// terminate with error set to 11, device locked by another device. </para><para>
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, device_clear
    /// SHALL terminate with error set to 15, I/O timeout. </para><para>
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// clear the device, device_clear SHALL terminate with error set to 17, I/O error. </para><para>
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, device_clear SHALL
    /// terminate with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DeviceClear( DeviceGenericParms request )
    {
        lock ( this._lock )
        {
            return base.DeviceClear( request );
        }
    }

    #endregion

}
