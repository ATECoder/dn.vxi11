using System.Net;

using cc.isr.ONC.RPC.Client;
using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="AbortChannelClient"/> implements the client stub proxy for the
/// <see cref="Vxi11ProgramConstants.AsyncProgram"/> remote program. It provides method
/// stubs which, when called, in turn call the appropriate remote method (procedure).
/// </summary>
/// <remarks>
/// After the first create_link, the network instrument client may create an RPC client for the abort channel,
/// but no additional client creations are necessary after subsequent <c>create_link</c>s. These connections may be
/// torn down by the network instrument client once all links have been closed with <c>destroy_link</c>. The whole
/// sequence could then start over. <para>
/// 
/// Used to transfer the <c>device_abort</c> RPC (optional for client).</para><para>
/// 
/// Renamed from <c>vxi11_DEVICE_ASYNC_Client</c> </para>
/// </remarks>
public class AbortChannelClient : OncRpcClientStubBase
{

    /// <summary>
    /// Constructs a <see cref="AbortChannelClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.AsyncProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <param name="client">   The ONC/RPC client connection object implementing the particular
    ///                         protocol and program. </param>
    public AbortChannelClient( OncRpcClientBase client ) : base( client )
    {
    }

    /// <summary>
    /// Constructs a <see cref="AbortChannelClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.AsyncProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program.. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocol"/> protocol to be used for ONC/RPC calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocol.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocol.OncRpcTcp"/>. </param>
    public AbortChannelClient( IPAddress host, OncRpcProtocol protocol, int timeout ) : this( host, Vxi11ProgramConstants.AsyncProgram,
                                                                                                   Vxi11ProgramConstants.AsyncVersion,
                                                                                                   0, protocol, timeout )
    {
    }

    /// <summary>
    /// Constructs a <see cref="AbortChannelClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.AsyncProgram"/> 
    /// remote program can be accessed. 
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program. </param>
    /// <param name="port">     The Port number at host where the remote program can be reached. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocol"/> protocol to be used for ONC/RPC
    ///                         calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocol.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocol.OncRpcTcp"/>. </param>
    public AbortChannelClient( IPAddress host, int port, OncRpcProtocol protocol, int timeout ) : this( host, Vxi11ProgramConstants.AsyncProgram,
                                                                                                       Vxi11ProgramConstants.AsyncVersion, port,
                                                                                                       protocol, timeout )
    {
    }

    /// <summary>
    /// Constructs a <see cref="AbortChannelClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.AsyncProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program. </param>
    /// <param name="program">  The Remote program number. </param>
    /// <param name="version">  The Remote program version number. </param>
    /// <param name="port">     The Port number at host where the remote program can be reached. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocol"/> protocol to be used for ONC/RPC
    ///                         calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocol.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocol.OncRpcTcp"/>. </param>
    public AbortChannelClient( IPAddress host, int program, int version, int port, OncRpcProtocol protocol,
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
        this.Client?.Call( ( int ) Vxi11Message.DeviceAbortProcedure, Vxi11ProgramConstants.AsyncVersion, request, result );
        return result;
    }

}
