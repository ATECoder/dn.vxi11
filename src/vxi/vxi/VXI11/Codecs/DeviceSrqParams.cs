namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceSrqParams"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/> RPC message.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_SrqParams</c>. </para><para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_SrqParams {
///    opaque handle{};
/// };
/// </code>
/// </remarks>
public class DeviceSrqParams : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceSrqParams() : this( new byte[40] )
    {
        this._handle = Array.Empty<byte>();
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-06-02. </remarks>
    /// <param name="handle">   The handle. </param>
    public DeviceSrqParams( byte[] handle )
    {
        this._handle = handle ?? new byte[40];
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceSrqParams( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceSrqParams"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceSrqParams"/>. </returns>
    public static DeviceSrqParams DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceSrqParams( decoder );
    }

    private byte[] _handle;

    /// <summary>   Gets the handle. </summary>
    /// <returns>   An array of byte. </returns>
    public byte[] GetHandle()
    {
        return this._handle;
    }

    /// <summary>   Sets a handle. </summary>
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
        this._handle.EncodeDynamicOpaque( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this._handle = decoder.DecodeDynamicOpaque();
    }

}
