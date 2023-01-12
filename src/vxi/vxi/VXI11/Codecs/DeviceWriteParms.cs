namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceWriteParms"/> class defines the request XDR codec for the <see cref="Vxi11Message.DeviceWriteProcedure"/>
/// RPC message.
/// </summary>
/// <remarks>
/// Renamed from <c>Device_WriteParms</c>. <para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_WriteParms {
///    Device_Link lid; /* link id from create_link */
///    unsigned long io_timeout; /* time to wait for I/O */
///    unsigned long lock_timeout; /* time to wait for lock */
///    Device_Flags flags;
///    opaque data; /* the data length and the data itself */
/// };
/// </code>
/// The network instrument server has indirect control over the maximum size of data through the
/// value of
/// <see cref="CreateLinkResp.MaxReceiveSize"/> returned in <see cref="Vxi11Message.CreateLinkProcedure"/>
/// .
/// If a controller needs to send greater than <see cref="CreateLinkResp.MaxReceiveSize"/> bytes
/// to the device at one time, then the network instrument client makes multiple calls to <see cref="Vxi11Message.DeviceWriteProcedure"/>
/// to accomplish the complete transaction. A network instrument server accepts at least 1024
/// bytes in a single <see cref="Vxi11Message.DeviceWriteProcedure"/> call. 
/// </remarks>
public class DeviceWriteParms : IXdrCodec
{
    /// <summary>   Default constructor. </summary>
    public DeviceWriteParms()
    {
        this._deviceLinkId = new DeviceLink();
        this._flags = new DeviceFlags();
        this._data = Array.Empty<byte>();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceWriteParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    private DeviceLink _deviceLinkId;
    /// <summary>   Gets or sets the identifier of the device link from the <see cref="Vxi11Message.CreateLinkProcedure"/> call. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink DeviceLinkId { get => this._deviceLinkId; set => this._deviceLinkId = value ?? new(); }

    /// <summary>   Gets or sets the i/o timeout. </summary>
    /// <value> The i/o timeout. </value>
    public int IOTimeout { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <value> The lock timeout. </value>
    public int LockTimeout { get; set; }

    private DeviceFlags _flags;
    /// <summary>   Gets or sets the <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/> options. </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get => this._flags; set => this._flags = value ?? new(); }

    private byte[] _data;

    /// <summary>   Gets the data. </summary>
    /// <returns>   An array of byte. </returns>
    public byte[] GetData() { return this._data; }

    /// <summary>   Sets a data. </summary>
    /// <remarks> 
    /// Associate an END message (?EOI) with the last byte in data when the end flag in <see cref="Flags"/> is set.
    /// </remarks>
    /// <param name="data"> Gets or sets the data. </param>
    public void SetData( byte[] data ) { this._data = data ?? Array.Empty<byte>(); }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.DeviceLinkId.Encode( encoder );
        encoder.EncodeInt( this.IOTimeout );
        encoder.EncodeInt( this.LockTimeout );
        this.Flags.Encode( encoder );
        encoder.EncodeDynamicOpaque( this._data );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.DeviceLinkId = new DeviceLink( decoder );
        this.IOTimeout = decoder.DecodeInt();
        this.LockTimeout = decoder.DecodeInt();
        this.Flags = new DeviceFlags( decoder );
        this._data = decoder.DecodeDynamicOpaque();
    }

}
