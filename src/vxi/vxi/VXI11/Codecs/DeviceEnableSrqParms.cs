namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceEnableSrqParms "/> class defines the request XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceEnableSrqProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_EnableSrqParms </c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_EnableSrqParms {
///    Device_Link lid;
///    bool enable;        /* Enable or disable interrupts */
///    opaque handle{40};  /* Host specific data */
/// };
/// </code>
/// </remarks>
public class DeviceEnableSrqParms : IXdrCodec
{
    /// <summary>   Gets or sets the identifier of the device link from the connect call. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink DeviceLinkId {  get; set; }

    /// <summary>   Gets or sets a value indicating whether to enable or disable interrupts. </summary>
    /// <value> True if enable, false if not. </value>
    public bool Enable { get; set; }

    /// <summary>   Gets or sets the handle. Host specific data. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="DeviceSrqParms.Handle"/> 
    /// when a service request occurs. </remarks>
    /// <value> The handle. </value>
    public byte[] Handle { get; set; }

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2023-01-04. </remarks>
    public DeviceEnableSrqParms()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceEnableSrqParms( XdrDecodingStreamBase decoder )
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
        encoder.EcodeBoolean( this.Enable );
        encoder.EncodeDynamicOpaque( this.Handle );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.DeviceLinkId = new DeviceLink( decoder );
        this.Enable = decoder.DecodeBoolean();
        this.Handle = decoder.DecodeDynamicOpaque();
    }

}
