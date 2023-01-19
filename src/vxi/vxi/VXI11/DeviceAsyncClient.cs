using System.Net;

using cc.isr.ONC.RPC.Client;
using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="DeviceAsyncClient"/> implements the client stub proxy for the
/// <see cref="Vxi11ProgramConstants.DeviceAsyncProgram"/> remote program. It provides method
/// stubs which, when called, in turn call the appropriate remote method (procedure).
/// </summary>
/// <remarks>
/// <para>
/// 
/// Renamed from <c>vxi11_DEVICE_ASYNC_Client</c> </para>
/// </remarks>
public class DeviceAsyncClient : OncRpcClientStubBase
{

    /// <summary>
    /// Constructs a <see cref="DeviceAsyncClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceAsyncProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <param name="client">   The ONC/RPC client connection object implementing the particular
    ///                         protocol and program. </param>
    public DeviceAsyncClient( OncRpcClientBase client ) : base( client )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceAsyncClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceAsyncProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program.. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocols.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocols.OncRpcTcp"/>. </param>
    public DeviceAsyncClient( IPAddress host, OncRpcProtocols protocol, int timeout ) : this( host, Vxi11ProgramConstants.DeviceAsyncProgram,
                                                                                                   Vxi11ProgramConstants.DeviceAsyncVersion,
                                                                                                   0, protocol, timeout )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceAsyncClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceAsyncProgram"/> 
    /// remote program can be accessed. 
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program. </param>
    /// <param name="port">     The Port number at host where the remote program can be reached. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC
    ///                         calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocols.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocols.OncRpcTcp"/>. </param>
    public DeviceAsyncClient( IPAddress host, int port, OncRpcProtocols protocol, int timeout ) : this( host, Vxi11ProgramConstants.DeviceAsyncProgram,
                                                                                                       Vxi11ProgramConstants.DeviceAsyncVersion, port,
                                                                                                       protocol, timeout )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceAsyncClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceAsyncProgram"/>
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
    public DeviceAsyncClient( IPAddress host, int program, int version, int port, OncRpcProtocols protocol,
                                                                        int timeout ) : base( host, program, version, port, protocol, timeout )
    {
    }

    /// <summary>   Calls remote procedure <see cref="Vxi11Message.DeviceAbortProcedure"/>. </summary>
    /// <remarks>
    /// <para>
    /// 
    /// Renamed from <c>device_abort_1</c> </para>.
    /// </remarks>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceLink"/> to send with the
    ///                         remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    ///
    public DeviceError DeviceAbort( DeviceLink request )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceAbortProcedure, Vxi11ProgramConstants.DeviceAsyncVersion, request, result );
        return result;
    }

}
