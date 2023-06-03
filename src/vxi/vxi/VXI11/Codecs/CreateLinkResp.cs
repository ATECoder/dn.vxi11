namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="CreateLinkResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11Message.CreateLinkProcedure"/> RPC message.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Create_LinkResp</c>. </para><para>
///  
/// VXI-11 Specifications: </para>
/// <code>
/// typedef long Device_Link;
/// struct Create_LinkResp {
///    Device_ErrorCode error;
///    Device_Link lid;
///    unsigned short abortPort;  /* for the abort RPC */
///    unsigned long maxRecvSize; /* specifies max data size in bytes device will accept on a write */
/// };
/// </code>
/// 
/// DeviceFlagsCodec and DeviceErrorCodeCodec are represented as integers, which simplifies the code
/// quite a bit and matches the VXI-11 specifications. <see cref="DeviceLink"/> codec is kept
/// even though it also is defined as a <c>typedef long</c> because Device Link is an argument in
/// some of the RPC calls whereas <see cref="DeviceOperationFlags"/> and <see cref="DeviceErrorCode"/>
/// are only included as members of codec classes. 
/// </remarks>
public class CreateLinkResp : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public CreateLinkResp()
    {
        this.ErrorCode = DeviceErrorCode.NoError;
        this._link = new();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  The XDR decoding stream. </param>
    public CreateLinkResp( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="CreateLinkResp"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="CreateLinkResp"/>. </returns>
    public static CreateLinkResp DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new CreateLinkResp( decoder );
    }

    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error as <see cref="DeviceErrorCode"/>. </value>
    public DeviceErrorCode ErrorCode { get; set; }

    private DeviceLink _link;
    /// <summary>
    /// Gets or sets the <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    /// call. This data structure is sent back to the client to identify the device for all
    /// subsequent Core calls.
    /// </summary>
    /// <remarks>
    /// The value of <see cref="DeviceLink"/> link is be unique for all currently active links within
    /// a network instrument server.
    /// </remarks>
    /// <value> The link to the device. </value>
    public DeviceLink DeviceLink { get => this._link; set => this._link = value ?? new(); }

    /// <summary>
    /// Gets or sets the abort port for the <see cref="AbortChannelClient.DeviceAbort(DeviceLink)"/>
    /// </summary>
    /// <remarks>
    /// The <see cref="AbortPort"/> is returned from the network instrument is used by the
    /// <see cref="AbortChannelClient"/> for implementing the <see cref="Vxi11Message.DeviceAbortProcedure">
    /// Device Abort</see>/&gt; RPC.
    /// <para>
    /// 
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned short because XDR encodes <see cref="short"/>s as <see cref="int"/>s. </para>
    /// </remarks>
    /// <value> The abort port. </value>
    public int AbortPort { get; set; }

    /// <summary>   Gets or sets the max data size in bytes device will accept on a write. </summary>
    /// <remarks>
    /// This is the size of the largest data set the network instrument server can accept in a <see cref="Vxi11Message.DeviceWriteProcedure"/>
    /// RPC. This value is at least 1024. <para>
    /// 
    /// The value is returned from the network instrument is used by the <see cref="CoreChannelClient"/>
    /// for implementing the <see cref="Vxi11Message.DeviceWriteProcedure">Device Write</see>/&gt;
    /// RPC. </para><para>
    /// 
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned short because the value is not expected to exceed that maximum <see cref="int"/>
    /// value. </para>
    /// </remarks>
    /// <value> The maximum <see cref="Vxi11Message.DeviceWriteProcedure"/> data size. </value>
    public int MaxReceiveSize { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- this <see cref="CreateLinkResp"/> into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.ErrorCode.Encode( encoder );
        this.DeviceLink.Encode( encoder );
        this.AbortPort.Encode( encoder );
        this.MaxReceiveSize.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- this <see cref="CreateLinkResp"/> from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCode = decoder.DecodeInt().ToDeviceErrorCode();
        this.DeviceLink = DeviceLink.DecodeInstance( decoder );
        this.AbortPort = decoder.DecodeShort();
        this.MaxReceiveSize = decoder.DecodeInt();
    }
}
