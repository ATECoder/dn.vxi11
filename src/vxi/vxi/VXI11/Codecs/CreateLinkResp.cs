namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="CreateLinkResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11MessageConstants.CreateLinkProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Create_LinkResp</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Create_LinkResp {
///    Device_ErrorCode error;
///    Device_Link lid;
///    unsigned short abortPort;  /* for the abort RPC */
///    unsigned long maxRecvSize; /* specifies max data size in bytes device will accept on a write */
/// };
/// </code>
/// </remarks>
public class CreateLinkResp : IXdrCodec
{
    /// <summary>   Gets or sets the error code. </summary>
    /// <value> The error code. </value>
    public DeviceErrorCode ErrorCode { get; set; }

    /// <summary>   Gets or sets the identifier of the device link. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink DeviceLinkId { get; set; }

    /// <summary>   Gets or sets the abort port for the <see cref="DeviceAsyncClient.DeviceAbort(DeviceLink)"/> Device Abort RPC. </summary>
    /// <value> The abort port. </value>
    public short AbortPort { get; set; }

    /// <summary>   Gets or sets the max data size in bytes device will accept on a write. </summary>
    /// <value> The maximum <see cref="Vxi11MessageConstants.DeviceWriteProcedure"/> data size. </value>
    public int MaxReceiveSize { get; set; }

    /// <summary>   Default constructor. </summary>
    public CreateLinkResp()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="xdr">  The XDR decoding stream. </param>
    public CreateLinkResp( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase xdr )
    {
        this.ErrorCode.Encode( xdr );
        this.DeviceLinkId.Encode( xdr );
        xdr.EncodeShort( this.AbortPort );
        xdr.EncodeInt( this.MaxReceiveSize );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase xdr )
    {
        this.ErrorCode = new DeviceErrorCode( xdr );
        this.DeviceLinkId = new DeviceLink( xdr );
        this.AbortPort = xdr.DecodeShort();
        this.MaxReceiveSize = xdr.DecodeInt();
    }
}
