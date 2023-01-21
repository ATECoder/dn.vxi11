using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Codecs;

using System.Net;

namespace cc.isr.VXI11;

/// <summary>
/// The abstract VXI-11 <see cref="Vxi11ProgramConstants.DeviceAsyncProgram"/> <see cref="DeviceAsyncServerStubBase"/> class is the base class upon which
/// to build VXI-11 <see cref="Vxi11ProgramConstants.DeviceAsyncProgram"/> TCP and UDP servers.
/// </summary>
public abstract class DeviceAsyncServerStubBase : OncRpcServerStubBase, IOncRpcDispatchable
{

    /// <summary>   Default constructor. </summary>
    public DeviceAsyncServerStubBase() : this( 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port. </param>
    public DeviceAsyncServerStubBase( int port ) : this( IPAddress.Any, port )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="bindAddr"> The bind address. </param>
    /// <param name="port">     The port. </param>
    public DeviceAsyncServerStubBase( IPAddress bindAddr, int port )
    {
        OncRpcProgramInfo[] registeredPrograms = new OncRpcProgramInfo[] {
            new OncRpcProgramInfo(Vxi11ProgramConstants.DeviceAsyncProgram, Vxi11ProgramConstants.DeviceAsyncVersion),
        };
        this.SetRegisteredPrograms( registeredPrograms );

        OncRpcTransportBase[] transports = new OncRpcTransportBase[] {
        new OncRpcUdpTransport(this, bindAddr, port, registeredPrograms, OncRpcTransportBase.BufferSizeDefault),
        new OncRpcTcpTransport(this, bindAddr, port, registeredPrograms, OncRpcTransportBase.BufferSizeDefault)
        };
        this.SetTransports( transports );
    }

    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <remarks>
    /// This interface has some fairly deep semantics, so please read the description above for how
    /// to use it properly. For background information about fairly deep semantics, please also refer
    /// to <i>Gigzales</i>, <i>J</i>.: Semantics considered harmful. Addison-Reilly, 1992, ISBN 0-542-
    /// 10815-X. <para>
    ///  
    /// See the introduction to this class for examples of how to use this interface properly.</para>
    /// </remarks>
    /// <param name="call">         <see cref="T:cc.isr.ONC.RPC.Server.OncRpcCallInformation" />
    ///                             about the call to handle, like the caller's Internet address, the ONC/RPC
    ///                             call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    <see cref="Vxi11Message">procedure</see>/> number requested. </param>
    public void DispatchOncRpcCall( OncRpcCallHandler call, int program, int version, int procedure )
    {
        if ( version == Vxi11ProgramConstants.DeviceInterruptVersion )
            switch ( procedure )
            {
                case ( int ) Vxi11Message.DeviceAbortProcedure:
                    {
                        DeviceLink link = new();
                        call.RetrieveCall( link );
                        DeviceError result = this.DeviceAbort( link );
                        call.Reply( result );
                        break;
                    }
                default:
                    call.ReplyProcedureNotAvailable();
                    break;
            }
        else
            call.ReplyProgramNotAvailable();
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceAbortProcedure"/>.
    /// </summary>
    /// <remarks>   <para>
    /// 
    /// Renamed from <c>device_abort_1</c> </para>. </remarks>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="link"> The request of type <see cref="Codecs.DeviceLink"/> to send to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DeviceAbort( DeviceLink link );

}
