namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceReadStbResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11Message.DeviceReadStbProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_ReadStbResp</c>. <para>
///  
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_ReadStbResp
/// {
///     Device_ErrorCode error; /* error code */
///     unsigned char stb; /* the returned status byte */
/// };
/// </code>
/// </remarks>
public class DeviceReadStbResp : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceReadStbResp()
    {
        this._errorCode = new DeviceErrorCode();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceReadStbResp( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceReadStbResp"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceReadStbResp"/>. </returns>
    public static DeviceReadStbResp DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceReadStbResp( decoder );
    }

    private DeviceErrorCode _errorCode;
    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error. </value>
    public DeviceErrorCode ErrorCode { get => this._errorCode; set => this._errorCode = value ?? new(); }

    /// <summary>   Gets or sets the status byte. </summary>
    /// <value> The status byte. </value>
    public byte Stb { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.ErrorCode.Encode( encoder );
        this.Stb.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCode = new DeviceErrorCode( decoder );
        this.Stb = decoder.DecodeByte();
    }

}
