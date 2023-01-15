namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceSrqParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_SrqParms</c>. <para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_SrqParms {
///    opaque handle{};
/// };
/// </code>
/// </remarks>
public class DeviceSrqParms : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceSrqParms()
    {
        this._handle = Array.Empty<byte>();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceSrqParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceSrqParms"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceSrqParms"/>. </returns>
    public static DeviceSrqParms DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceSrqParms( decoder );
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
