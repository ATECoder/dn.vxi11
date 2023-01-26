using System.Net;
using System.Reflection;

using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Visa;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.IEEE488.Mock;

/// <summary>   An IEEE488 Mock server. </summary>
/// <remarks>   
/// Closing a client connected to the Mock local server no longer throws an exception when destroying the link.
/// </remarks>
public partial class Ieee488MockServer : DeviceCoreServerStubBase
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public Ieee488MockServer() : this( null, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port number where the server will wait for incoming calls. </param>
    public Ieee488MockServer( int port ) : this( null, port )
    {
    }

    public Ieee488MockServer( IPAddress? bindAddr, int port ) : this( new Ieee488Device(), bindAddr, port )
    {
    }

    public Ieee488MockServer( Ieee488Device device ) : this( device, null, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Ieee488MockServer( Ieee488Device device, IPAddress? bindAddr, int port ) : base( bindAddr ?? IPAddress.Any, port )
    {
        this._device = device;
        this._interfaceDeviceString = string.Empty;
        this._readMessage = string.Empty;
        this._writeMessage = string.Empty;
        this.AbortPortNumber = AbortChannelServer.AbortPortDefault;
        this.MaxReceiveLength = Ieee488Client.MaxReceiveLengthDefault;
    }

    /// <summary>   Close all transports listed in a set of server transports. </summary>
    /// <remarks>
    /// Only by calling this method processing of remote procedure calls by individual transports can
    /// be stopped. This is because every server transport is handled by its own thread.
    /// </remarks>
    /// <exception cref="AggregateException">   Thrown when an Aggregate error condition occurs. </exception>
    public override void Close()
    {

        List<Exception> exceptions = new();

        try
        {
            this.AbortServer?.StopRpcProcessing();
            // todo: wait for the server to stop running and then close. 
            this.AbortServer?.Close();
        }
        catch ( Exception ex )
        {
            exceptions.Add( ex );
        }
        finally
        {
            this.AbortServer = null;
        }

        try
        {
            base.Close();
        }
        catch ( Exception ex )
        {
            exceptions.Add( ex );
        }

        this._device = null;

        if ( exceptions.Any() )
        {
            AggregateException aggregateException = new( exceptions );
            throw aggregateException;
        }
    }

    #endregion

    #region " abort server "

    private int _abortPortNumber;
    /// <summary>   Gets or sets the abort port number. </summary>
    /// <value> The abortPort number. </value>
    public int AbortPortNumber
    {
        get => this._abortPortNumber;
        set => _ = this.SetProperty( ref this._abortPortNumber, value );
    }

    /// <summary>   Handler abort request. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Device error event information. </param>
    private void HandlerAbortRequest( object sender, DeviceErrorEventArgs e )
    {
        if ( this._device is null ) return;
        e.ErrorCodeValue = this._device.Abort().ErrorCode.ErrorCodeValue;
    }

    protected AbortChannelServer? AbortServer { get; set; }

    /// <summary>   Device abort. </summary>
    /// <remarks>
    /// To successfully complete a device_abort RPC, a network instrument server SHALL: <para>
    /// 
    /// 1. Initiate termination of any core channel, in-progress RPC associated with the link except
    /// destroy_link, device_enable_srq, and device_unlock. </para><para>
    /// 
    /// 2. Return with error set to 0, no error, to indicate successful completion </para><para>
    /// 
    /// The intent of this rule is to handle the device_abort RPC ahead of the other operations, but
    /// due to operating system specific implementation details the timeliness cannot be guaranteed. </para>
    /// <para>
    /// 
    /// The device_abort RPC only aborts an in-progress RPC, not a queued RPC. </para><para>
    /// 
    /// After replying to the device_abort call, the network instrument server SHALL reply to the
    /// original in-progress call which was aborted with error set to 23, aborted.  </para><para>
    /// 
    /// Receiving 0 on the abort call at the network instrument client only means that the abort was
    /// successfully delivered to the network instrument server. </para><para>
    /// 
    /// The lid parameter is compared against the active link identifiers . If none match,
    /// device_abort SHALL terminate with error set to 4 invalid link identifier.  </para><para>
    /// 
    /// The operation of device_abort SHALL NOT be affected by locking  </para>
    /// </remarks>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <returns>   A DeviceErrorCode. </returns>
    public virtual void EnableAbortServer()
    {
        if ( this.AbortServer is null )
        {
            this.AbortServer = new AbortChannelServer( this.IPv4Address, this.AbortPortNumber );
            this.AbortServer.AbortRequested += this.HandlerAbortRequest;

            this.AbortServer.Run();
        }
    }

    #endregion

    #region " I/O messages "

    private string _writeMessage;
    /// <summary>   Gets or sets a message that was sent to the device. </summary>
    /// <value> The message that was sent to the device. </value>
    public string WriteMessage
    {
        get => this._writeMessage;
        set => _ = this.SetProperty( ref this._writeMessage, value );
    }

    private string _readMessage;
    /// <summary>   Gets or sets a message that was received from the device. </summary>
    /// <value> A message that was received from the device. </value>
    public string ReadMessage
    {
        get => this._readMessage;
        set => _ = this.SetProperty( ref this._readMessage, value );
    }

    #endregion

    #region " IEEE488 properties "

    /// <summary>
    /// Gets or sets the encoding to use when serializing strings. If <see langcref="null" />, the
    /// system's default encoding is to be used.
    /// </summary>
    /// <value> The character encoding. </value>
    public override Encoding CharacterEncoding
    {
        get => base.CharacterEncoding;
        set => _ = this.SetProperty( base.CharacterEncoding!, value, () => base.CharacterEncoding = value );
    }

    private int _waitOnOutTime = 1000;
    /// <summary>   Timeout wait time ms. </summary>
    /// <value> The wait on out time. </value>
    public int WaitOnOutTime
    {
        get => this._waitOnOutTime;
        set => _ = this.SetProperty( ref this._waitOnOutTime, value );
    }

    /// <summary>   The current operation instruction type. </summary>
    /// <value> The type of the current operation. </value>
    public Ieee488OperationType CurrentOperationType { get; private set; } = Ieee488OperationType.None;

    private string _interfaceDeviceString;
    /// <summary>   Gets or sets the interface device string. </summary>
    /// <value> The interface device string. </value>
    public string InterfaceDeviceString
    {
        get => this._interfaceDeviceString;
        private set => _ = this.SetProperty( ref this._interfaceDeviceString, value );
    }

    private DeviceAddress _interfaceDevice;
    /// <summary>   Gets or sets the interface device. </summary>
    /// <value> The interface device. </value>
    public DeviceAddress InterfaceDevice
    {
        get => this._interfaceDevice;
        set {
            _ = this.SetProperty( ref this._interfaceDevice, value );
            this.InterfaceDeviceString = this._interfaceDevice.InterfaceDeviceAddress;
        }
    }

    private int _maxReceiveLength;
    public int MaxReceiveLength
    {
        get => this._maxReceiveLength;
        set => _ = this.SetProperty( ref this._maxReceiveLength, value );
    }

    #endregion

    #region " mock device "

    /// <summary>
    /// current device
    /// </summary>
    private IIeee488Device? _device = null;

    /// <summary>
    /// Thread synchronization locks
    /// </summary>
    private readonly ManualResetEvent _asyncLocker = new( false );

    /// <summary>
    /// Read cache buffer
    /// </summary>
    private byte[] _readBuffer = Array.Empty<byte>();

    #endregion

    #region " LXI-11 ONC/RPC Calls "

    private int _linkId = 0;

    /// <summary>   Create a device connection; Opens a link to a device. </summary>
    /// <remarks>
    /// To successfully complete a create_link RPC, a network instrument server SHALL: <para>
    /// 1. If lockDevice is set to true, acquire the lock for the device. </para><para>
    /// 2. Return in lid a link identifier to be used with future calls. The value of lid SHALL be
    /// unique for
    /// all currently active links within a network instrument server.  </para><para>
    /// 3. Return in maxRecvSize the size of the largest data parameter the network instrument server
    /// can
    /// accept in a device_write RPC.This value SHALL be at least 1024.  </para><para>
    /// 4. Return in asyncPort the port number for asynchronous RPCs. See device_abort.  </para><para>
    /// 5. Return with error set to 0, no error, to indicate successful completion.  </para><para>
    /// 
    /// The device parameter is a string which identifies the device for communications.See the
    /// document(s) referred to in section A.6, Related Documents, for definitions of this string.  </para><para>
    /// 
    /// A network instrument server should be able to maintain at least two separate links
    /// simultaneously over a single network instrument connection. </para><para>
    /// 
    /// The network instrument client sends an identifying number in the clientId parameter.While
    /// this protocol requires no special behavior based on the value of clientId, the device may
    /// provide a local means to examine its value to help a user identify communication problems. </para> <para>
    /// 
    /// The network instrument server SHALL NOT alter its function based on the clientId. </para><para>
    /// 
    /// If create_link is called when another link is not available, create_link SHALL terminate and
    /// set error to 9. </para><para>
    /// 
    /// The operation of create_link SHALL ignore locks if lockDevice is false. </para><para>
    /// If lockDevice is true and the lock is not freed after at least lock_timeout milliseconds,
    /// create_link SHALL terminate without creating a link and return with error set to 11, device
    /// locked by another link.Page 26 Section B: Network Instrument Protocol October 4, 2000
    /// Printing VXIbus Specification: VXI-11 Revision 1.0 </para><para>
    /// 
    /// The execution of create_link SHALL have no effect on the state of any device associated with
    /// the network instrument server. </para><para>
    /// 
    /// A create_link RPC cannot be aborted since a valid link identifier is not yet available.A
    /// network instrument client should set lock_timeout to a reasonable value to avoid locking up
    /// the server. </para>
    /// </remarks>
    /// <param name="linkInfo"> Information describing the link. </param>
    /// <returns>   The new link to a device. </returns>
    public override CreateLinkResp CreateLink( CreateLinkParms linkInfo )
    {
        CreateLinkResp reply = new() {
            DeviceLink = new DeviceLink() { LinkId = this._linkId++ },
            MaxReceiveSize = this.MaxReceiveLength,
            AbortPort = ( short ) this.AbortPortNumber
        };

        Logger.Writer.LogVerbose( $"creating link to {linkInfo.Device}" );

        this.InterfaceDevice = new DeviceAddress( linkInfo.Device );
        reply.ErrorCode = this.InterfaceDevice.IsValid()
            ? new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.NoError }
            : new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.InvalidLinkIdentifier };
        return reply;
    }

    /// <summary>   Destroy a connection. </summary>
    /// <remarks>
    /// To successfully complete a destroy_link RPC, a network instrument server SHALL: <para>
    /// 1. Deactivate the link identifier and recover any resources associated with the link. </para><para>
    /// 2. If this link has the lock, free the lock (see device_lock and create_link). </para><para>
    /// 3. Disable this link from using the interrupt mechanism( see device_enable_srq ). </para><para>
    /// 4. Return with error set to 0, no error, to indicate successful completion. </para><para>
    /// The Device_Link( link identifier ) parameter is compared against the active link
    /// identifiers.If none match, destroy_link SHALL terminate and set error to 4. </para><para>
    /// 
    /// After a destroy_link, the network instrument server typically becomes ready to execute a new
    /// create_link, assuming the resources have not already been utilized. </para><para>
    /// 
    /// The execution of destroy_link SHALL have no effect on the state of any device associated with
    /// the network instrument server. </para><para>
    /// 
    /// The operation of destroy_link SHALL NOT be affected by device_abort. </para>
    /// </remarks>
    /// <param name="deviceLink">   The device link. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DestroyLink( DeviceLink deviceLink )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Create an interrupt channel. </summary>
    /// <param name="deviceRemoteFunction"> The device remote function. </param>
    /// <returns>   The new interrupt channel 1. </returns>
    public override DeviceError CreateIntrChan( DeviceRemoteFunc deviceRemoteFunction )
    {
        DeviceError result = new() { ErrorCode = new DeviceErrorCode( ( int ) OncRpcExceptionReason.OncRpcSuccess ) };
        return result;
    }

    /// <summary>   Destroy an interrupt channel. </summary>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DestroyIntrChan()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Device clear. </summary>
    /// <remarks>
    /// Since not all devices directly support a clear operation, how this operation is executed depends upon the
    /// interface between the network instrument server and the device. <para>
    /// If the device does not support a clear operation and the network instrument server is able to detect this,
    /// device_clear SHALL terminate and set error to 8, operation not supported. </para><para>
    /// The lid parameter is compared against the active link identifiers. If none match, device_clear SHALL
    /// terminate with error set to 4, invalid link identifier. </para><para>
    /// If some other link has the lock, device_clear SHALL examine the waitlock flag in flags.If the flag is set,
    /// device_clear SHALL block until the lock is free.If the flag is not set, device_clear SHALL terminate
    /// with error set to 11, device locked by another link. </para><para>
    /// If after at least lock_timeout milliseconds the lock is not freed, device_clear SHALL terminate with error
    /// set to 11, device locked by another device. </para><para>
    /// If after at least io_timeout milliseconds the operation is not complete, device_clear SHALL terminate
    /// with error set to 15, I/O timeout. </para><para>
    /// If the network instrument server encounters a device specific I/O error while attempting to clear the
    /// device, device_clear SHALL terminate with error set to 17, I/O error. </para><para>
    /// If the asynchronous device_abort RPC is called during execution, device_clear SHALL terminate with
    /// error set to 23, abort. </para>
    /// </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceClear( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device executes a command. </summary>
    /// <param name="deviceCommandParameters">  device command parameters. </param>
    /// <returns>   A Device_DocmdResp. </returns>
    public override DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms deviceCommandParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device enables or does not enable the Send Request service. </summary>
    /// <param name="deviceEnableSrqParameters">    Device enable SRQ parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceEnableSrq( DeviceEnableSrqParms deviceEnableSrqParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device local control. </summary>
    /// <remarks>
    /// To successfully complete a device_local RPC, a network instrument server SHALL: <para>
    /// 1. Place the associated device in a local state. </para><para>
    /// 2. Return with error set to zero, no error, to indicate successful completion. </para><para>
    /// Since not all devices directly support a local state, how this operation is executed depends upon the
    /// interface between the network instrument server and the device. </para><para>
    /// If the device does not support a local state and the network instrument server is able to detect this,
    /// device_local SHALL terminate and set error to 8, operation not supported. </para><para>
    /// The lid parameter is compared against the active link identifiers . If none match, device_local SHALL
    /// terminate with error set to 4, invalid link identifier. </para><para>
    /// If some other link has the lock, device_local SHALL examine the waitlock flag in flags.If the flag is set,
    /// device_local SHALL block until the lock is free.If the flag is not set, device_local SHALL terminate
    /// with error set to 11, device locked by another link. </para><para>
    /// If after at least lock_timeout milliseconds the lock is not freed, device_local SHALL terminate with error
    /// set to 11, device locked by another link. </para><para>
    /// If after at least io_timeout milliseconds the operation is not complete, device_local SHALL terminate
    /// with error set to 15, I/O timeout. </para><para>
    /// If the network instrument server encounters a device specific I/O error while attempting to place the
    /// device in the local state, device_local SHALL terminate with error set to 17, I/O error. </para><para>
    /// If the asynchronous device_abort RPC is called during execution, device_local SHALL terminate with
    /// error set to 23, abort. </para> 
    /// </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceLocal( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device remote control. </summary>
    /// <remarks>
    /// Since not all devices directly support a remote state, how this operation is executed depends upon the
    /// interface between the network instrument server and the device. <para>
    /// If the device does not support a remote state and the network instrument server is able to detect this,
    /// device_remote SHALL terminate and set error to 8, operation not supported. </para><para>
    /// The lid parameter is compared against the active link identifiers. If none match, device_remote SHALL
    /// terminate with error set to 4, invalid link identifier. </para><para>
    /// If some other link has the lock, device_remote SHALL examine the waitlock flag in flags.If the flag is
    /// set, device_remote SHALL block until the lock is free.If the flag is not set, device_remote SHALL
    /// terminate with error set to 11, device locked by another link.  </para><para>
    /// If after at least lock_timeout milliseconds the lock is not freed, device_remote SHALL terminate with
    /// error set to 11, device locked by another link.  </para><para>
    /// If after at least io_timeout milliseconds the operation is not complete, device_remote SHALL terminate
    /// with error set to 15, I/O timeout.  </para><para>
    /// If the network instrument server encounters a device specific I/O error while attempting to place the
    /// device in the remote state, device_remote SHALL terminate with error set to 17, I/O error. </para><para>
    /// If the asynchronous device_abort RPC is called during execution, device_remote SHALL terminate with
    /// error set to 23, abort. </para>
    /// </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceRemote( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Returns the device status byte. </summary>
    /// <remarks>
    /// To successfully complete a device_readstb RPC, the network instrument server SHALL: <para>
    /// 1. Return in the stb parameter the device's status byte. </para><para>
    /// 2. Return with error set to 0, no error, to indicate successful completion. </para><para>
    /// 
    /// Since not all devices directly support a status byte, how this operation is executed and the
    /// semantics of the stb parameter depend upon the interface between the network instrument
    /// server and the device.  </para><para>
    /// 
    /// If a status byte cannot be returned, device_readstb SHALL terminate and set error to 8,
    /// operation not supported.  </para><para>
    /// 
    /// The lid parameter is compared against the active link identifiers.If none match,
    /// device_readstb SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// 
    /// If some other link has the lock, the procedure examines the waitlock flag in flags.If the
    /// flag is set, device_readstb blocks until the lock is free before retrieving the status byte.
    /// If the flag is not set, device_readstb SHALL terminate and set error to 11, device locked by
    /// another link.</para><para>
    /// 
    /// If after at least lock_timeout milliseconds the lock is not freed, device_readstb SHALL
    /// terminate with error set to 11, device locked by another link.</para><para>
    /// 
    /// If after at least io_timeout milliseconds the operation is not complete, device_readstb SHALL
    /// terminate with error set to 15, I/O timeout.</para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// read the data, device_readstb SHALL terminate with error set to 17.</para><para>
    /// 
    /// If the asynchronous device_abort RPC is called during execution, device_readstb SHALL
    /// terminate with error set to 23.</para>
    /// </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_ReadStbResp. </returns>
    public override DeviceReadStbResp DeviceReadStb( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Performs a trigger. </summary>
    /// <remarks>
    /// If the device does not support a trigger and the network instrument server is able to detect
    /// this, device_trigger SHALL terminate and set error to 8, operation not supported. <para>
    /// 
    /// IEEE 488.1 and similar interfaces may not be able to detect that the device does not support
    /// a trigger. </para><para>
    /// The lid parameter is compared against the link identifiers.If none match, device_trigger
    /// SHALL terminate and set error to 4, invalid link identifier. </para><para>
    /// 
    /// If some other link has the lock, device_trigger SHALL examine the waitlock flag in flags.If
    /// the flag is set, device_trigger SHALL block until the lock is free before sending the
    /// trigger.If the flag is not set, device_trigger SHALL terminate and set error to 11, device
    /// locked by another link. </para><para>
    /// 
    /// If after at least lock_timeout milliseconds the lock is not freed, device_trigger SHALL
    /// terminate with error set to 11, device locked by another link. </para><para>
    /// 
    /// If after at least io_timeout milliseconds the operation is not complete, device_trigger SHALL
    /// terminate with error set to 15, I/O timeout. </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while sending to
    /// trigger , device_trigger SHALL terminate with error set to 17, I/O error. </para><para>
    /// 
    /// If the asynchronous device_abort RPC is called during execution, device_trigger SHALL
    /// terminate with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceTrigger( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Lock the device. </summary>
    /// <remarks>
    /// To successfully complete a device_lock RPC, a network instrument server SHALL: <para>
    /// 1. Acquire the device's lock. </para><para>
    /// 2. Return with error set to zero, no error, to indicate successful completion. </para><para>
    /// If this link already has the lock, the network instrument server SHALL terminate with error set to 11,
    /// device locked by another link. </para><para>
    /// Multiple network instrument servers on the same host need to communicate with one another to
    /// implement locking since locks are global to all network instrument servers in a given host. </para><para>
    /// The lid parameter is compared against the active link identifiers . If none match, device_lock SHALL
    /// terminate, before trying to acquire the device's lock, with error set to 4, invalid link identifier. </para><para>
    /// If some other link has the lock, device_lock SHALL examine the waitlock flag in flags.If the flag is set,
    /// device_lock SHALL block until the lock is free.If the flag is not set, device_lock SHALL terminate with
    /// error set to 11, device locked by another link. </para><para>
    /// The network instrument server blocks if another link has the lock, but does not block if another link is
    /// performing an I/O operation so long as the lock is available. </para><para>
    /// If after at least lock_timeout milliseconds the lock is not freed, device_lock SHALL terminate with error
    /// set to 11, device locked by another link. </para><para>
    /// If the asynchronous device_abort RPC is called during execution, device_lock SHALL terminate with
    /// error set to 23, abort. </para><para>
    /// The locks SHALL be tied to the core connection between the network instrument client and the network
    /// instrument server.This means that if the network instrument server detects a broken connection, it
    /// SHALL release all of the connection's locks. </para>
    /// </remarks>
    /// <param name="deviceLockParameters"> Device lock parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceLock( DeviceLockParms deviceLockParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Unlock the device. </summary>
    /// <remarks>
    /// To successfully complete a device_unlock, a network instrument server SHALL: <para>
    /// 1. Release the lock. </para><para>
    /// 2. Return with error set to zero, no error, to indicate successful completion. </para><para>
    /// The Device_Link (link identifier) parameter is compared against the active link identifiers . If none
    /// match, device_unlock SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// If this link does not have the lock, device_unlock SHALL terminate with error set to 12, no lock held by
    /// this link. </para><para>
    /// The operation of device_unlock SHALL NOT be affected by device_abort. </para>
    /// </remarks>
    /// <param name="deviceLink">   The device link parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceUnlock( DeviceLink deviceLink )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Read a message. </summary>
    /// <remarks>
    /// To successfully complete a device_read RPC, a network instrument server SHALL: <para>
    /// 1. Transfer bytes into the data parameter until one of the following termination conditions
    /// are met:
    /// a.An END indicator is read.The END bit in reason SHALL be set. </para><para>
    /// b.requestSize bytes are transferred.The REQCNT bit in reason SHALL be set. This termination
    /// condition SHALL be used if requestSize is zero.  </para><para>
    /// c.termchrset is set in flags and a character which matches termChar is transferred.The CHR
    /// bit in reason SHALL be set. </para><para>
    /// d.The buffer used to return the response is full.No bits in reason SHALL BE set.
    /// 2. Return with error set to 0, no error, to indicate successful completion.  </para><para>
    /// If more than one termination condition is valid, reason contains the bitwise inclusive OR of
    /// all the reasons.  </para><para>
    /// 
    /// If reason is not set (value of 0) and error is zero, then the network instrument client could
    /// issue device_read calls until one of the other three termination conditions is encountered. </para>
    /// 
    /// <list type="bullet">Abort shall cause the following errors: <item>
    /// The lid parameter is compared against the active link identifiers. If none match, device_read
    /// SHALL
    ///    terminate with error set to 4, invalid link identifier. </item><item>
    /// 
    /// If some other link has the lock, device_read SHALL examine the wait lock flag in flags. If
    /// the flag is set, device_read SHALL block until the lock is free before transferring data.If
    /// the flag is not set, device_read SHALL terminate with error set to 11, device locked by
    /// another link. </item><item>
    /// 
    /// If after at least lock_timeout milliseconds the lock is not freed, device_read SHALL
    /// terminate with error set to 11, device locked by another device and data.data_len set to
    /// zero. </item><item>
    /// 
    /// If the transfer takes longer than io_timeout milliseconds, device_read SHALL terminate with
    /// error set to
    /// 15, I/O timeout, data.data_len set to however many bytes were transferred, and reason set to
    /// zero. </item><item>
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// read the data, device_read SHALL terminate with error set to 17, I/O error. </item><item>
    /// 
    /// If the asynchronous device_abort RPC is called during execution, device_read SHALL terminate
    /// with error set to 23, abort. </item><item>
    /// 
    /// The number of bytes transferred from the device into data SHALL be returned in data.data_len
    /// even when device_read terminates due to a timeout or <c>device_abort</c>. </item></list>
    /// </remarks>
    /// <param name="deviceReadParameters"> Device read parameters. </param>
    /// <returns>   A Device_ReadResp. </returns>
    public override DeviceReadResp DeviceRead( DeviceReadParms deviceReadParameters )
    {
        DeviceReadResp readRes = new();
        if ( this.CurrentOperationType == Ieee488OperationType.None || this.CurrentOperationType == Ieee488OperationType.Write )
        {
            this._readBuffer = Array.Empty<byte>();
            _ = this._asyncLocker.Reset();
        }
        if ( !this._asyncLocker.WaitOne( this.WaitOnOutTime ) )
        {
            readRes.SetData( this._readBuffer );
            readRes.ErrorCode = new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.IOError }; // timeout
            readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
            return readRes;
        }

        if ( this.CurrentOperationType == Ieee488OperationType.Read )
        {
            readRes.SetData( this._readBuffer );
            readRes.ErrorCode = new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.NoError };
            readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
        }
        this.CurrentOperationType = Ieee488OperationType.None; //Reset the action type
        return readRes;
    }

    /// <summary>   Process the device write procedure. </summary>
    /// <remarks>
    /// To a successfully complete a device_write RPC, the network instrument server SHALL: <para>
    /// 1. Transfer the contents of data to the device. </para><para>
    /// 2. Return in size parameter the number of bytes accepted by the device. </para><para>
    /// 3. Return with error set to 0, no error. </para><para>
    /// 
    /// If the end flag in flags is set, then an END indicator SHALL be associated with the last byte
    /// in data. </para><para>
    /// 
    /// If a controller needs to send greater than maxRecvSize bytes to the device at one time, then
    /// the network instrument client makes multiple calls to device_write to accomplish the complete
    /// transaction.A network instrument server accepts at least 1,024 bytes in a single device_write
    /// call due to RULE B.6.3.  </para><para>
    /// The value of data.data_len may be zero, in which case no device actions are performed.  </para>
    /// <para>
    /// 
    /// The lid parameter is compared to the active link identifiers. If none match, device_write
    /// SHALL terminate and set error to 4, invalid link identifier. </para><para>
    /// 
    /// If data.data_len is greater than the value of maxRecvSize returned in create_link,
    /// device_write SHALL terminate without transferring any bytes to the device and SHALL set error
    /// to 5.Section B: Network Instrument Protocol Page 29 October 4, 2000 Printing VXIbus
    /// Specification: VXI-11 Revision 1.0 </para><para>
    /// 
    /// If some other link has the lock, device_write SHALL examine the waitlock flag in flags.If the
    /// flag is set, device_write SHALL block until the lock is free.If the flag is not set,
    /// device_write SHALL terminate and set error to 11, device already locked by another link. </para>
    /// <para>
    /// 
    /// If after at least lock_timeout milliseconds the lock is not freed, device_write SHALL
    /// terminate with error set to 11, device already locked by another link. </para><para>
    /// 
    /// If after at least io_timeout milliseconds not all of data has been transferred to the device,
    /// device_write SHALL terminate with error set to 15, I/O timeout. This timeout is based on the
    /// entire transaction and not the time required to transfer single bytes. </para><para>
    /// 
    /// The io_timeout value set by the application may need to change based on the size of data. </para>
    /// <para>
    /// 
    /// If the asynchronous device_abort RPC is called during execution, device_write SHALL terminate
    /// with error set to 23, abort. </para><para>
    /// 
    /// The number of bytes transferred to the device SHALL be returned in size, even when the call
    /// terminates due to a timeout or device_abort. </para><para>
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// write the data, device_write SHALL terminate with error set to 17, I/O error. </para>
    ///  <list type="bullet">Abort shall cause the following errors: <item>
    /// If the asynchronous device_abort RPC is called during execution, <c>device_write</c>
    /// terminate with error set to 23, abort. </item><item>
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// write the data, device_write SHALL terminate with error set to 17, I/O error. </item><item>
    /// 
    /// </item></list>
    /// </remarks>
    /// <param name="deviceWriteParameters">    Device write parameters. </param>
    /// <returns>   A Device_WriteResp. </returns>
    public override DeviceWriteResp DeviceWrite( DeviceWriteParms deviceWriteParameters )
    {
        // get the write command.
        string cmd = this.CharacterEncoding.GetString( deviceWriteParameters.GetData() );
        Logger.Writer.LogVerbose( $"link ID: {deviceWriteParameters.Link.LinkId} -> Received：{cmd}" );
        DeviceWriteResp result = new() { ErrorCode = new DeviceErrorCode( ( int ) OncRpcExceptionReason.OncRpcSuccess ) };

        // holds one or more SCPI commands each with its arguments
        string[] scpiCommands = cmd.Split( new char[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries );

        if ( scpiCommands.Length == 0 )
        {
            // The instruction is incorrect or undefined
            result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.SyntaxError );
            return result;
        }

        // process all the SCPI commands
        for ( int n = 0; n < scpiCommands.Length; n++ )
        {
            string spciCommand = scpiCommands[n]; // select a complete SCPI command with optional arguments
            Logger.Writer.LogVerbose( $"Process the instruction： {spciCommand}" );
            string[] scpiArgs = Array.Empty<string>(); // Holds the SCPI command arguments

            // split the command to the core command and its arguments:
            string[] scpiCmdElements = scpiCommands[n].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            spciCommand = scpiCmdElements[0].Trim();

            _ = this._asyncLocker.Reset(); // Block threads
            this._readBuffer = Array.Empty<byte>();

            // check if we have a query message (read) or a write message:
            this.CurrentOperationType = spciCommand[^1] == '?' ? Ieee488OperationType.Read : Ieee488OperationType.Write;

            // get the command arguments
            if ( scpiCmdElements.Length >= 2 )
                scpiArgs = scpiCmdElements[1].Split( new char[] { '，' }, StringSplitOptions.RemoveEmptyEntries );

            // find the mock server method that corresponds to the SCPI command.
            MethodInfo? method = this._device!.GetType().GetMethods().ToList().Find( p => {
                var att = p.GetCustomAttribute( typeof( Ieee488Attribute ) );
                if ( att == null || att is not Ieee488Attribute ) return false;
                Ieee488Attribute scpiAtt = ( Ieee488Attribute ) att;

                // return success if the command matches the method attribute
                return String.Equals( scpiAtt.Content, spciCommand, StringComparison.OrdinalIgnoreCase );
            } );

            if ( method is not null )
            {
                Ieee488Attribute scpiAtt = ( Ieee488Attribute ) method.GetCustomAttribute( typeof( Ieee488Attribute ) )!;
                try
                {
                    object? res = null;
                    switch ( scpiAtt.OperationType )
                    {
                        case Ieee488OperationType.None:
                            Logger.Writer.LogMemberWarning( $"The attribute of method {method} is marked incorrectly as {scpiAtt.OperationType}。" );
                            break;
                        case Ieee488OperationType.Write:
                            this.WriteMessage = scpiCommands[n];
                            // invoke the corresponding method
                            res = method.Invoke( this._device, scpiArgs );
                            result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.NoError );
                            break;
                        case Ieee488OperationType.Read://Query instructions
                            this.WriteMessage = scpiCommands[n];
                            res = method.Invoke( this._device, scpiArgs );
                            if ( res is not null )
                            {
                                this.ReadMessage = res.ToString();
                                this._readBuffer = this.CharacterEncoding.GetBytes( res.ToString()! );
                                Logger.Writer.LogVerbose( $"Query results： {res}。" );
                            }
                            else
                            {
                                this.ReadMessage = "null";
                                Logger.Writer.LogVerbose( "Query results：NULL。" );
                                result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.NoError );
                            }
                            break;
                    }
                }
                catch ( Exception ex )
                {
                    Logger.Writer.LogMemberError( $"An error occurred when the method was called：{method}", ex );
                    // Parameter error
                    result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.ParameterError );
                }
            }
            else
            {
                Logger.Writer.LogMemberWarning( $"No method found： {spciCommand}" );
                result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.SyntaxError ); // The instruction is incorrect or undefined
                this.CurrentOperationType = Ieee488OperationType.None;
            }
            _ = this._asyncLocker.Set();//Reset block
        }


        return result;
    }

    #endregion

}
