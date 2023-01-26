namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceWriteResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11Message.DeviceWriteProcedure"/> RPC message.
/// </summary>
/// <remarks> <para>
///
/// Renamed from <c>Device_WriteResp</c>. </para> <para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_WriteResp {
///    Device_ErrorCode error;
///    unsigned long size; /* Number of bytes written */
/// };
/// </code>
/// </remarks>
public class DeviceWriteResp : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceWriteResp()
    {
        this._errorCode = new DeviceErrorCode();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceWriteResp( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceWriteResp"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceWriteResp"/>. </returns>
    public static DeviceWriteResp DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceWriteResp( decoder );
    }

    private DeviceErrorCode _errorCode;
    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error. </value>
    public DeviceErrorCode ErrorCode { get => this._errorCode; set => this._errorCode = value ?? new(); }

    /// <summary>   Gets or sets the size; the number of bytes written. </summary>
    /// <value> The number of bytes written. </value>
    public int Size { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.ErrorCode.Encode( encoder );
        this.Size.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCode = new DeviceErrorCode( decoder );
        this.Size = decoder.DecodeInt();
    }

}
