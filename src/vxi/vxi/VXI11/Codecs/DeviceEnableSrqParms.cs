namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceEnableSrqParms "/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.DeviceEnableSrqProcedure"/> RPC message.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_EnableSrqParms </c>. </para><para>
/// 
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

    /// <summary>   Default constructor. </summary>
    public DeviceEnableSrqParms()
    {
        this._link = new();
        this._handle = Array.Empty<byte>();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceEnableSrqParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceEnableSrqParms"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceEnableSrqParms"/>. </returns>
    public static DeviceEnableSrqParms DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceEnableSrqParms( decoder );
    }

    private DeviceLink _link;
    /// <summary>   Gets or sets the <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/> call. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink Link { get => this._link; set => this._link = value ?? new(); }

    /// <summary>   Gets or sets a value indicating whether to enable or disable interrupts. </summary>
    /// <value> True if enable, false if not. </value>
    public bool Enable { get; set; }

    /// <summary>   Gets or sets the handle. Host specific data for handling the service request. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="DeviceSrqParms.GetHandle()"/> 
    /// when a service request occurs. <para>
    /// The network instrument client should send in the handle parameter a unique link identifier. This will
    /// allow the network instrument client to identify the link associated with subsequent 
    /// <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/> RPCs. </para>
    /// </remarks>
    /// <value> The handle. </value>
    private byte[] _handle;

    /// <summary>   Gets the handle. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="DeviceSrqParms.GetHandle()"/> 
    /// when a service request occurs. </remarks>
    /// <returns>   An array of byte. </returns>
    public byte[] GetHandle()
    {
        return this._handle;
    }

    /// <summary>   Sets a handle. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="DeviceSrqParms.GetHandle()"/> 
    /// when a service request occurs. </remarks>
    /// <param name="handle">   The handle. </param>
    public void SetHandle( byte[] handle )
    {
        this._handle = handle ?? Array.Empty<byte>();
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.Link.Encode( encoder );
        this.Enable.Encode( encoder );
        this._handle.EncodeDynamicOpaque( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Link = new DeviceLink( decoder );
        this.Enable = decoder.DecodeBoolean();
        this._handle = decoder.DecodeDynamicOpaque();
    }

}
