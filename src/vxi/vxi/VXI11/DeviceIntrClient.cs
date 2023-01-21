using System.Net;

using cc.isr.ONC.RPC.Client;
using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="DeviceIntrClient"/> implements the client stub proxy for the
/// <see cref="Vxi11ProgramConstants.DeviceInterruptProgram"/> remote program. It provides method
/// stubs which, when called, in turn call the appropriate remote method (procedure).
/// </summary>
/// <remarks> <para>
/// 
/// Renamed from <c>vxi11_DEVICE_INTR_Client</c> </para>.
/// </remarks>
public class DeviceIntrClient : OncRpcClientStubBase
{

    /// <summary>
    /// Constructs a <see cref="DeviceIntrClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceInterruptVersion"/>
    /// remote program can be accessed.
    /// </summary>
    /// <param name="client">   The ONC/RPC client connection object implementing the particular
    ///                         protocol and program. </param>
    public DeviceIntrClient( OncRpcClientBase client ) : base( client )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceIntrClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceInterruptVersion"/>
    /// remote program can be accessed.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program.. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocols.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocols.OncRpcTcp"/>. </param>
    public DeviceIntrClient( IPAddress host, OncRpcProtocols protocol, int timeout ) : this( host, Vxi11ProgramConstants.DeviceInterruptProgram,
                                                                                                   Vxi11ProgramConstants.DeviceInterruptVersion,
                                                                                                   0, protocol, timeout )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceIntrClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceInterruptProgram"/> 
    /// remote program can be accessed. 
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program. </param>
    /// <param name="port">     The Port number at host where the remote program can be reached. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC
    ///                         calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocols.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocols.OncRpcTcp"/>. </param>
    public DeviceIntrClient( IPAddress host, int port, OncRpcProtocols protocol, int timeout ) : this( host, Vxi11ProgramConstants.DeviceInterruptProgram,
                                                                                                       Vxi11ProgramConstants.DeviceInterruptVersion, port,
                                                                                                       protocol, timeout )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceIntrClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceInterruptProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program. </param>
    /// <param name="program">  The Remote program number. </param>
    /// <param name="version">  The Remote program version number. </param>
    /// <param name="port">     The Port number at host where the remote program can be reached. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC
    ///                         calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocols.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocols.OncRpcTcp"/>. </param>
    public DeviceIntrClient( IPAddress host, int program, int version, int port, OncRpcProtocols protocol,
                                                                       int timeout ) : base( host, program, version, port, protocol, timeout )
    {
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 
    /// Renamed from <c>device_intr_srq_1</c> </para>.
    /// </remarks>
    /// <param name="handle">   The handle as it was received from the Core device. </param>
    public void DeviceIntrSrq( byte[] handle )
    {
        DeviceSrqParms request = new() {
        };
        request.SetHandle( handle );
        this.DeviceIntrSrq( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 
    /// Renamed from <c>device_intr_srq_1</c> </para>.
    /// </remarks>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceSrqParms"/> to send to
    ///                         the remote procedure call. </param>
    public void DeviceIntrSrq( DeviceSrqParms request )
    {
        VoidXdrCodec result = VoidXdrCodec.VoidXdrCodecInstance;
        this.Client?.Call( ( int ) Vxi11Message.DeviceInterruptSrqProcedure, Vxi11ProgramConstants.DeviceInterruptVersion, request, result );
    }

}
