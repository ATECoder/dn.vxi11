namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceWriteParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceWriteProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_WriteParms</c>. <para>
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
/// </remarks>
public class DeviceWriteParms : IXdrCodec
{
    /// <summary>   Gets or sets the identifier of the device link from <see cref="Vxi11MessageConstants.CreateLinkProcedure"/>. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink DeviceLinkId { get; set; }

    /// <summary>   Gets or sets the i/o timeout. </summary>
    /// <value> The i/o timeout. </value>
    public int IOTimeout { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <value> The lock timeout. </value>
    public int LockTimeout { get; set; }

    /// <summary>   Gets or sets the option <see cref="DeviceOperationFlag"/> flags. </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get; set; }

    /// <summary>   Gets or sets the data. </summary>
    /// <value> The data. </value>
    public byte[] Data { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceWriteParms()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceWriteParms( XdrDecodingStreamBase decoder )
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
        encoder.EncodeInt( this.IOTimeout );
        encoder.EncodeInt( this.LockTimeout );
        this.Flags.Encode( encoder );
        encoder.EncodeDynamicOpaque( this.Data );
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
        this.Data = decoder.DecodeDynamicOpaque();
    }

}
