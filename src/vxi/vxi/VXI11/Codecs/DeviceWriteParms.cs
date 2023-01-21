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
///    Device_Link lid;             /* link id from create_link */
///    unsigned long io_timeout;    /* time to wait for I/O */
///    unsigned long lock_timeout;  /* time to wait for lock */
///    Device_Flags flags;
///    opaque data;                 /* the data length and the data itself */
/// };
/// </code>
/// The network instrument server has indirect control over the maximum size of data through the
/// value of <see cref="CreateLinkResp.MaxReceiveSize"/> returned in <see cref="Vxi11Message.CreateLinkProcedure"/>.
/// If a controller needs to send greater than <see cref="CreateLinkResp.MaxReceiveSize"/> bytes
/// to the device at one time, then the network instrument client makes multiple calls to <see cref="Vxi11Message.DeviceWriteProcedure"/>
/// to accomplish the complete transaction. A network instrument server accepts at least 1024
/// bytes in a single <see cref="Vxi11Message.DeviceWriteProcedure"/> call. <para>
/// 
/// The variable length data, which is sent to the server is encoded as 
/// <see cref="XdrEncodingStreamBase.EncodeDynamicOpaque(byte[])"/> </para>
/// </remarks>
public class DeviceWriteParms : IXdrCodec
{
    /// <summary>   Default constructor. </summary>
    public DeviceWriteParms()
    {
        this._link = new DeviceLink();
        this._flags = new DeviceFlags();
        this._data = Array.Empty<byte>();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceWriteParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceWriteParms"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceWriteParms"/>. </returns>
    public static DeviceWriteParms DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceWriteParms( decoder );
    }

    private DeviceLink _link;
    /// <summary>
    /// Gets or sets the <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    /// call.
    /// </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink Link { get => this._link; set => this._link = value ?? new(); }

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

    private DeviceFlags _flags;
    /// <summary>   Gets or sets the <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/> options. </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get => this._flags; set => this._flags = value ?? new(); }

    private byte[] _data;

    /// <summary>   Gets the data to send. </summary>
    /// <returns>   An array of byte. </returns>
    public byte[] GetData() { return this._data; }

    /// <summary>   Sets a data to send. </summary>
    /// <remarks>
    /// Associate an END message (?EOI) with the last byte in data when the end flag in <see cref="Flags"/>
    /// is set. 
    /// </remarks>
    /// <param name="data"> Gets or sets the data. </param>
    public void SetData( byte[] data ) { this._data = data ?? Array.Empty<byte>(); }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.Link.Encode( encoder );
        this.IOTimeout.Encode( encoder );
        this.LockTimeout.Encode( encoder );
        this.Flags.Encode( encoder );
        this._data.EncodeDynamicOpaque( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Link = new DeviceLink( decoder );
        this.IOTimeout = decoder.DecodeInt();
        this.LockTimeout = decoder.DecodeInt();
        this.Flags = new DeviceFlags( decoder );
        this._data = decoder.DecodeDynamicOpaque();
    }

}
