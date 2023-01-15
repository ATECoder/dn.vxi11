namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="CreateLinkResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11Message.CreateLinkProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Create_LinkResp</c>. <para>
///  
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

    /// <summary>   Default constructor. </summary>
    public CreateLinkResp()
    {
        this._errorCode = new();
        this._link = new();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  The XDR decoding stream. </param>
    public CreateLinkResp( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    private DeviceErrorCode _errorCode;
    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error. </value>
    public DeviceErrorCode ErrorCode { get => this._errorCode; set => this._errorCode = value ?? new(); }

    private DeviceLink _link;
    /// <summary>
    /// Gets or sets the <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    /// call. This data structure is sent back to the client to identify the device for all
    /// subsequent Core calls.
    /// </summary>
    /// <value> The link to the device. </value>
    public DeviceLink DeviceLink { get => this._link; set => this._link = value ?? new(); }

    /// <summary>   Gets or sets the abort port for the <see cref="DeviceAsyncClient.DeviceAbort(DeviceLink)"/> Device Abort RPC. </summary>
    /// <value> The abort port. </value>
    public short AbortPort { get; set; }

    /// <summary>   Gets or sets the max data size in bytes device will accept on a write. </summary>
    /// <value> The maximum <see cref="Vxi11Message.DeviceWriteProcedure"/> data size. </value>
    public int MaxReceiveSize { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.ErrorCode.Encode( encoder );
        this.DeviceLink.Encode( encoder );
        encoder.EncodeShort( this.AbortPort );
        encoder.EncodeInt( this.MaxReceiveSize );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCode = new DeviceErrorCode( decoder );
        this.DeviceLink = new DeviceLink( decoder );
        this.AbortPort = decoder.DecodeShort();
        this.MaxReceiveSize = decoder.DecodeInt();
    }
}
