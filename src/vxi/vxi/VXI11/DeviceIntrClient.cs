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
/// The <c>create_intr_chan</c> RPC is used to identify the host or port that can service the interrupt. The
/// <c>device_enable_srq</c> RPC is used to enable or disable an interrupt. The <c>destroy_intr_chan</c> RPC is used to
/// close the interrupt channel. </para><para>
/// 
/// The <c>device_enable_srq</c> RPC contains a handle parameter. The same data contained in handle is passed
/// back in the handle parameter of the <c>device_intr_srq</c> RPC. Since the same data is passed back, the
/// network instrument client can identify the link associated with the <c>device_intr_srq</c>. </para><para>
/// 
/// The network instrument protocol recognizes one type of interrupt, service request. Note that the return
/// type to the interrupt RPC is void, denoting a one-way RPC. </para><para>
/// 
/// A network instrument host uses the following RPCL definition for interrupt messages.
/// <code>
/// struct Device_SrqParms
/// {
///    opaque handle;
/// };
/// program DEVICE_INTR
/// {
///    version DEVICE_INTR_VERSION {
///        void device_intr_srq( Device_SrqParms) = 30;
///    }=1;
/// } = 0x0607B1;
/// </code>
/// The program number <c>0x0607B1</c> is the registered program number for the network instrument protocol's
/// interrupt channel. </para> <para>
/// 
/// Renamed from <c>vxi11_DEVICE_INTR_Client</c> </para>.
/// </remarks>
public class DeviceIntrClient : OncRpcClientStubBase
{

    #region " construction and cleanup "

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

    #endregion

    #region " RPC actions "

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
        DeviceSrqParms request = new( handle ) {
        };
        this.DeviceIntrSrq( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/>.
    /// </summary>
    /// <remarks>
    /// The <c>device_intr_srq</c> RPC is implemented as a one-way RPC. This means that the network
    /// instrument server does not expect a response from the network instrument client. This is
    /// necessary to avoid deadlock situations in a single-threaded environment where if a response
    /// were expected to an interrupt both the network instrument client and network instrument
    /// server could be waiting for a response from the other, with neither proceeding. <para>
    /// 
    /// The network instrument server may issue interrupts in the middle of an active call. In
    /// general, this implementation gives more timely responses, and can be easier than delaying the
    /// interrupt until an in-progress action has finished. </para> <para>
    /// 
    /// Network instrument clients can implement interrupts by using either a separate interrupt process, threads,
    /// or by emulating threads using a signal handling routine that is invoked on incoming messages to the
    /// interrupt port.
    /// </para>
    /// <para>
    /// 
    /// Renamed from <c>device_intr_srq_1</c> </para>.
    /// </remarks>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceSrqParms"/> to send to
    ///                         the remote procedure call. </param>
    ///
    /// ### <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    public void DeviceIntrSrq( DeviceSrqParms request )
    {
        VoidXdrCodec result = VoidXdrCodec.VoidXdrCodecInstance;
        this.Client?.Call( ( int ) Vxi11Message.DeviceInterruptSrqProcedure, Vxi11ProgramConstants.DeviceInterruptVersion, request, result );
    }

    #endregion

}
