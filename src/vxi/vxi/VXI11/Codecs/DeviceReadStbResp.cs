namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceReadStbResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceReadStbProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_ReadStbResp</c>. <para>
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
    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> error. </summary>
    /// <value> The error. </value>
    public DeviceErrorCode Error {  get; set; }

    /// <summary>   Gets or sets the status byte. </summary>
    /// <value> The status byte. </value>
    public byte Stb { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceReadStbResp()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public DeviceReadStbResp( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase xdr )
    {
        this.Error.Encode( xdr );
        xdr.EncodeByte( this.Stb );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase xdr )
    {
        this.Error = new DeviceErrorCode( xdr );
        this.Stb = xdr.DecodeByte();
    }

}
