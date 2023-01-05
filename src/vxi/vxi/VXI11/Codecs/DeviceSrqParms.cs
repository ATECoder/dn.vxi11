namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceSrqParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceInterruptSrqProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_SrqParms</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_SrqParms {
///    opaque handle{};
/// };
/// </code>
/// </remarks>
public class DeviceSrqParms : IXdrCodec
{

    /// <summary>   Gets or sets the handle. </summary>
    /// <value> The handle. </value>
    public byte[] Handle { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceSrqParms()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceSrqParms( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeDynamicOpaque( this.Handle );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Handle = decoder.DecodeDynamicOpaque();
    }

}
