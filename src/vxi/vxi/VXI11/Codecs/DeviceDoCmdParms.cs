namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceDoCmdParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.DeviceDoCommandProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_DocmdParms</c>. <para>
/// 
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

    /// <summary>   Default constructor. </summary>
    public DeviceDoCmdParms()
    {
        this._link = new();
        this._flags = new();
        this._dataIn = Array.Empty<byte>();
        this.NetworkOrder = true;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceDoCmdParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceDoCmdParms"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceDoCmdParms"/>. </returns>
    public static DeviceDoCmdParms DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceDoCmdParms( decoder );
    }

    private DeviceLink _link;
    /// <summary>   Gets or sets the <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/> call. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink Link { get => this._link; set => this._link = value ?? new(); }

    private DeviceFlags _flags;
    /// <summary>   Gets or sets the <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/> options. </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get => this._flags; set => this._flags = value ?? new(); }

    /// <summary>   Gets or sets the i/o timeout. </summary>
    /// <remarks>
    /// The <see cref="IOTimeout"/> determines how long a network instrument server allows an I/O operation 
    /// to take. If the <see cref="IOTimeout"/> is non-zero, the network instrument server allows at least 
    /// <see cref="IOTimeout"/> milliseconds before returning control to the client with a timeout error.
    /// The time it takes for the I/O operation to complete does not include any time spent waiting for the lock.
    /// </remarks>
    /// <value> The i/o timeout. </value>
    public int IOTimeout { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <remarks>
    /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a lock
    /// to be released. If the device is locked by another link and the <see cref="LockTimeout"/> is non-zero,
    /// the network instrument server allows at least <see cref="LockTimeout"/> milliseconds for a lock to be 
    /// released.
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
    /// <see langword="true"/> (set) if the architecture of the network instrument client specifies byte-ordering in
    /// network order (big-endian), false if not.
    /// </value>
    public bool NetworkOrder { get; set; }

    /// <summary>   Gets or sets the data size. </summary>
    /// <value> The size of the data. </value>
    public int DataSize { get; set; }

    /// <summary>   Gets or sets the data in; do cmd data parameters. </summary>
    /// <value> The data in. </value>
    private byte[] _dataIn;

    /// <summary>   Gets data in. </summary>
    /// <returns>   An array of byte. </returns>
    public byte[] GetDataIn()
    {
        return this._dataIn ?? Array.Empty<byte>();
    }

    /// <summary>   Sets data in. </summary>
    /// <param name="dataIn">   Gets or sets the data in; do cmd data parameters. </param>
    public void SetDataIn( byte[] dataIn )
    {
        this._dataIn = dataIn ?? Array.Empty<byte>();
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.Link.Encode( encoder );
        this.Flags.Encode( encoder );
        this.IOTimeout.Encode( encoder );
        this.LockTimeout.Encode( encoder );
        this.Cmd.Encode( encoder );
        this.NetworkOrder.Encode( encoder );
        this.DataSize.Encode( encoder );
        this._dataIn.EncodeDynamicOpaque( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Link = new DeviceLink( decoder );
        this.Flags = new DeviceFlags( decoder );
        this.IOTimeout = decoder.DecodeInt();
        this.LockTimeout = decoder.DecodeInt();
        this.Cmd = decoder.DecodeInt();
        this.NetworkOrder = decoder.DecodeBoolean();
        this.DataSize = decoder.DecodeInt();
        this._dataIn = decoder.DecodeDynamicOpaque();
    }

}
