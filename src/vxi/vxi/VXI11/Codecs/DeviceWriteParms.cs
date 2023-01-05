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
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public DeviceWriteParms( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase xdr )
    {
        this.DeviceLinkId.Encode( xdr );
        xdr.EncodeInt( this.IOTimeout );
        xdr.EncodeInt( this.LockTimeout );
        this.Flags.Encode( xdr );
        xdr.EncodeDynamicOpaque( this.Data );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase xdr )
    {
        this.DeviceLinkId = new DeviceLink( xdr );
        this.IOTimeout = xdr.DecodeInt();
        this.LockTimeout = xdr.DecodeInt();
        this.Flags = new DeviceFlags( xdr );
        this.Data = xdr.DecodeDynamicOpaque();
    }

}
