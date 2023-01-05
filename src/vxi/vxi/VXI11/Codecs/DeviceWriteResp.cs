namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceWriteResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceWriteProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_WriteResp</c>. <para>
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

    /// <summary>   Gets or sets the error. </summary>
    /// <value> The error. </value>
    public DeviceErrorCode Error { get; set; }

    /// <summary>   Gets or sets the size; the number of bytes written. </summary>
    /// <value> The number of bytes written. </value>
    public int Size { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceWriteResp()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceWriteResp( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.Error.Encode( encoder );
        encoder.EncodeInt( this.Size );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Error = new DeviceErrorCode( decoder );
        this.Size = decoder.DecodeInt();
    }

}
