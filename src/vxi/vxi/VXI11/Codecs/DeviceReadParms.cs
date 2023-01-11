namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceReadParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.DeviceReadProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_ReadParms</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_ReadParms {
///    Device_Link lid;            /* link id from create_link */
///    unsigned long requestSize;  /* Bytes requested */
///    unsigned long io_timeout;   /* time to wait for I/O */
///    unsigned long lock_timeout; /* time to wait for lock */
///    Device_Flags flags;
///    char termChar;              /* valid if flags and 'Term Char Set' */
/// };
/// </code>
/// </remarks>
public class DeviceReadParms : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceReadParms()
    {
        this._deviceLinkId = new();
        this._flags = new();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceReadParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    private DeviceLink _deviceLinkId;
    /// <summary>   Gets or sets the identifier of the device link from the <see cref="Vxi11Message.CreateLinkProcedure"/> call. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink DeviceLinkId { get => this._deviceLinkId; set => this._deviceLinkId = value ?? new(); }

    /// <summary>   Gets or sets the request size (number of bytes). </summary>
    /// <value> The size of the request. </value>
    public int RequestSize { get; set; }

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

    private DeviceFlags _flags;
    /// <summary>   Gets or sets the <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/> options. </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get => this._flags; set => this._flags = value ?? new(); }

    /// <summary>
    /// Gets or sets the termination character; valid if flags <see cref="DeviceOperationFlags.TerminationCharacterSet"/>
    /// is set.
    /// </summary>
    /// <value> The term character. </value>
    public byte TermChar { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.DeviceLinkId.Encode( encoder );
        encoder.EncodeInt( this.RequestSize );
        encoder.EncodeInt( this.IOTimeout );
        encoder.EncodeInt( this.LockTimeout );
        this.Flags.Encode( encoder );
        encoder.EncodeByte( this.TermChar );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.DeviceLinkId = new DeviceLink( decoder );
        this.RequestSize = decoder.DecodeInt();
        this.IOTimeout = decoder.DecodeInt();
        this.LockTimeout = decoder.DecodeInt();
        this.Flags = new DeviceFlags( decoder );
        this.TermChar = decoder.DecodeByte();
    }

}
