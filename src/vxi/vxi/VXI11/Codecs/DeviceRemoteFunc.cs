using cc.isr.VXI11.EnumExtensions;

namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceRemoteFunc"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.CreateInterruptChannelProcedure"/> RPC message.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_RemoteFunc</c>. </para><para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_RemoteFunc {
///    unsigned long hostAddr;       /* Host servicing Interrupt */
///    unsigned short hostPort;      /* valid port # on client */
///    unsigned long progNum;        /* DEVICE_INTR */
///    unsigned long progVers;       /* DEVICE_INTR_VERSION */
///    Device_AddrFamily progFamily; /* DEVICE_UDP | DEVICE_TCP */
/// };
/// </code>
/// </remarks>
public class DeviceRemoteFunc : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceRemoteFunc()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceRemoteFunc( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceRemoteFunc"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceRemoteFunc"/>. </returns>
    public static DeviceRemoteFunc DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceRemoteFunc( decoder );
    }

    /// <summary>   Gets or sets the host address. </summary>
    /// <value> The host address. </value>
    public uint HostAddr { get; set; }

    /// <summary>   Gets or sets the host port. </summary>
    /// <value> The host port. </value>
    public int HostPort { get; set; }

    /// <summary>   Gets or sets the program number, should be <see cref="Vxi11ProgramConstants.InterruptProgram"/>. </summary>
    /// <value> The program number. </value>
    public int ProgNum { get; set; }

    /// <summary>   Gets or sets the program version; should be <see cref="Vxi11ProgramConstants.InterruptVersion"/>. </summary>
    /// <value> The program version. </value>
    public int ProgVers { get; set; }

    /// <summary>   Gets or sets the <see cref="VXI11.TransportProtocol"/> . </summary>
    /// <remarks>
    /// Using UDP for the interrupt channel generally provides higher performance, but with the risks
    /// that some <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/> RPCs might not
    /// arrive at all or that they might arrive out of order.
    /// </remarks>
    /// <value> The <see cref="VXI11.TransportProtocol"/>. </value>
    public TransportProtocol TransportProtocol { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.HostAddr.Encode( encoder );
        this.HostPort.Encode( encoder );
        this.ProgNum.Encode( encoder );
        this.ProgVers.Encode( encoder );
        (( int ) this.TransportProtocol).Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.HostAddr = decoder.DecodeUInt();
        this.HostPort = decoder.DecodeInt();
        this.ProgNum = decoder.DecodeInt();
        this.ProgVers = decoder.DecodeInt();
        this.TransportProtocol = decoder.DecodeInt().ToTransportProtocol();
    }

}
