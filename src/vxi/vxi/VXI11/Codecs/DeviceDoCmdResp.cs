namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceDoCmdResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceDoCommandProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_DocmdResp</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_DocmdResp {
/// Device_ErrorCode error; /* returned status */
/// opaque data_out{}; /* returned data parameter */
/// };
/// </code>
/// </remarks>
public class DeviceDoCmdResp : IXdrCodec
{
    /// <summary>   Gets or sets the error (return status). </summary>
    /// <value> The error. </value>
    public DeviceErrorCode Error { get; set; }

    /// <summary>   Gets or sets the data out. Returned data parameters. </summary>
    /// <value> The data out. </value>
    public byte[] DataOut { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceDoCmdResp()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceDoCmdResp( XdrDecodingStreamBase decoder )
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
        encoder.EncodeDynamicOpaque( this.DataOut );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Error = new DeviceErrorCode( decoder );
        this.DataOut = decoder.DecodeDynamicOpaque();
    }

}
