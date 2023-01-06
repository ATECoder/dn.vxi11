namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceDoCmdParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceDoCommandProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_DocmdParms</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_DocmdParms {
///    Device_Link lid;             /* link id from create_link */
///    Device_Flags flags;          /* flags specifying various options */
///    unsigned long io_timeout;    /* time to wait for I/O to complete */
///    unsigned long lock_timeout;  /* time to wait on a lock */
///    long cmd;                    /* which command to execute */
///    bool network_order;          /* client's byte order */
///    long datasize;               /* size of individual data elements */
///    opaque data_in{};            /* do cmd data parameters */
/// };
/// </code>
/// </remarks>
public class DeviceDoCmdParms : IXdrCodec
{
    /// <summary>   Gets or sets the identifier of the device link from the connect call. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink DeviceLinkId {  get; set; }

    /// <summary>   Gets or sets the flags specifying various options. </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get; set; }

    /// <summary>   Gets or sets the i/o timeout. </summary>
    /// <remarks>
    /// The i/o timeout value determines how long a network instrument server allows an I/O operation to take.
    /// </remarks>
    /// <value> The i/o timeout. </value>
    public int IOTimeout { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <remarks>
    /// The lock timeout determines how long a network instrument server will wait for a lock to be released.
    /// Units for both are in milliseconds.
    /// </remarks>
    /// <value> The lock timeout. </value>
    public int LockTimeout { get; set; }

    /// <summary>   Gets or sets the command; which command to execute. </summary>
    /// <remarks>
    /// The value of <see cref="Cmd"/> is compared against the values supported by the network
    /// instrument server. If the particular value is not supported, the server returns error code
    /// <see cref="DeviceErrorCodeValue.OperationNotSupported"/> (8).
    /// </remarks>
    /// <value> The command. </value>
    public int Cmd { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the network order; client's byte order. Network order
    /// is defined by the Internet Protocol Suite.
    /// </summary>
    /// <value>
    /// True (set) if the architecture of the network instrument client specifies byte-ordering in
    /// network order( big-endian), false if not.
    /// </value>
    public bool NetworkOrder { get; set; }

    /// <summary>   Gets or sets the data size. </summary>
    /// <value> The size of the data. </value>
    public int DataSize { get; set; }

    /// <summary>   Gets or sets the data in; do cmd data parameters. </summary>
    /// <value> The data in. </value>
    public byte[] DataIn { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceDoCmdParms()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceDoCmdParms( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.DeviceLinkId.Encode( encoder );
        this.Flags.Encode( encoder );
        encoder.EncodeInt( this.IOTimeout );
        encoder.EncodeInt( this.LockTimeout );
        encoder.EncodeInt( this.Cmd );
        encoder.EcodeBoolean( this.NetworkOrder );
        encoder.EncodeInt( this.DataSize );
        encoder.EncodeDynamicOpaque( this.DataIn );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>   2023-01-04. </remarks>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.DeviceLinkId = new DeviceLink( decoder );
        this.Flags = new DeviceFlags( decoder );
        this.IOTimeout = decoder.DecodeInt();
        this.LockTimeout = decoder.DecodeInt();
        this.Cmd = decoder.DecodeInt();
        this.NetworkOrder = decoder.DecodeBoolean();
        this.DataSize = decoder.DecodeInt();
        this.DataIn = decoder.DecodeDynamicOpaque();
    }

}
