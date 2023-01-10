using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Codecs;

using System.Net;

namespace cc.isr.VXI11;

/// <summary>
/// The abstract VXI-11 <see cref="Vxi11ProgramConstants.DeviceInterruptProgram"/> <see cref="DeviceIntrServerStubBase"/> class is the base class upon which
/// to build VXI-11 <see cref="Vxi11ProgramConstants.DeviceInterruptProgram"/> TCP and UDP servers.
/// </summary>
public abstract class DeviceIntrServerStubBase : OncRpcServerStubBase, IOncRpcDispatchable
{

    /// <summary>   Default constructor. </summary>
    public DeviceIntrServerStubBase() : this( 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port. </param>
    public DeviceIntrServerStubBase( int port ) : this( null, port )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="bindAddr"> The bind address. </param>
    /// <param name="port">     The port. </param>
    public DeviceIntrServerStubBase( IPAddress bindAddr, int port )
    {
        OncRpcServerTransportRegistrationInfo[] info = new OncRpcServerTransportRegistrationInfo[] {
            new OncRpcServerTransportRegistrationInfo(Vxi11ProgramConstants.DeviceInterruptProgram, Vxi11ProgramConstants.DeviceInterruptVersion),
        };
        this.SetTransportRegistrationInfo( info );

        OncRpcServerTransportBase[] transports = new OncRpcServerTransportBase[] {
            new OncRpcUdpServerTransport(this, bindAddr, port, info, OncRpcServerTransportBase.DefaultBufferSize),
            new OncRpcTcpServerTransport(this, bindAddr, port, info, OncRpcServerTransportBase.DefaultBufferSize)
        };
        this.SetTransports( transports );
    }

    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <remarks>
    /// This interface has some fairly deep semantics, so please read the description above for how
    /// to use it properly. For background information about fairly deep semantics, please also refer
    /// to <i>Gigzales</i>, <i>J</i>.: Semantics considered harmful. Addison-Reilly, 1992, ISBN 0-542-
    /// 10815-X. <para>
    /// See the introduction to this class for examples of how to use this interface properly.</para>
    /// </remarks>
    /// <param name="call">         <see cref="T:cc.isr.ONC.RPC.Server.OncRpcCallInformation" />
    ///                             about the call to handle, like the caller's Internet address, the ONC/RPC
    ///                             call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    public void DispatchOncRpcCall( OncRpcCallInformation call, int program, int version, int procedure )
    {
        if ( version == Vxi11ProgramConstants.DeviceInterruptVersion )
            switch ( (Vxi11MessageConstants ) procedure)
            {
                case Vxi11MessageConstants.DeviceInterruptSrqProcedure:
                    {
                        DeviceSrqParms args = new();
                        call.RetrieveCall( args );
                        this.DeviceIntrSrq( args );
                        call.Reply( VoidXdrCodec.VoidXdrCodecInstance );
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
    /// Calls remote procedure <see cref="Vxi11MessageConstants.DeviceInterruptSrqProcedure"/>.
    /// </summary>
    /// <remarks>   <para>
    /// Renamed from <c>device_intr_srq_1</c> </para>. </remarks>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceSrqParms"/>) to the remote procedure call.. </param>
    public abstract void DeviceIntrSrq( DeviceSrqParms arg1 );

}
