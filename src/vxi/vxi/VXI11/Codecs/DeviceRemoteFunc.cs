namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceRemoteFunc"/> class defines the request XDR
/// codec for the <see cref="Vxi11MessageConstants.CreateInterruptChannelProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_RemoteFunc</c>. <para>
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
    /// <summary>   Gets or sets the host address. </summary>
    /// <value> The host address. </value>
    public int HostAddr {  get; set; }

    /// <summary>   Gets or sets the host port. </summary>
    /// <value> The host port. </value>
    public int HostPort { get; set; }

    /// <summary>   Gets or sets the program number, should be <see cref="Vxi11ProgramConstants.DeviceInterruptProgram"/>. </summary>
    /// <value> The program number. </value>
    public int ProgNum { get; set; }

    /// <summary>   Gets or sets the program version; should be <see cref="Vxi11ProgramConstants.DeviceInterruptVersion"/>. </summary>
    /// <value> The program version. </value>
    public int ProgVers { get; set; }

    /// <summary>   Gets or sets the <see cref="DeviceAddrFamily"/> program family . </summary>
    /// <value> The <see cref="DeviceAddrFamily"/> program family. </value>
    public int ProgFamily { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceRemoteFunc()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public DeviceRemoteFunc( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase xdr )
    {
        xdr.EncodeInt( this.HostAddr );
        xdr.EncodeInt( this.HostPort );
        xdr.EncodeInt( this.ProgNum );
        xdr.EncodeInt( this.ProgVers );
        xdr.EncodeInt( this.ProgFamily );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase xdr )
    {
        this.HostAddr = xdr.DecodeInt();
        this.HostPort = xdr.DecodeInt();
        this.ProgNum = xdr.DecodeInt();
        this.ProgVers = xdr.DecodeInt();
        this.ProgFamily = xdr.DecodeInt();
    }

}
