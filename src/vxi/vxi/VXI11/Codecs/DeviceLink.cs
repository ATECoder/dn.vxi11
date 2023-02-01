namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceLink"/> class defines the request XDR
/// codec used with all device core XDR codecs such as <see cref="CreateLinkResp"/>.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_Link</c>. </para><para>
///  
/// VXI-11 Specifications: </para>
/// <code>
/// typedef long Device_Link;
/// </code>
/// 
/// The network instrument server returns an identifier of type <see cref="DeviceLink"/> as a
/// result of the <see cref="Vxi11Message.CreateLinkProcedure"/> call. This identifier
/// is handed back to the network instrument server by the network instrument client on each
/// subsequent call. The network instrument server verifies the validity of the identifier on
/// each call. The <see cref="DeviceLink"/> data is not modified by the controller. <para>
/// 
/// DeviceFlagsCodec and DeviceErrorCodec are represented as integers, which simplifies the code
/// quite a bit and matches the VXI-11 specifications. <see cref="DeviceLink"/> codec is kept
/// even though it also is defined as a <c>typedef long</c> because Device Link is an argument in
/// some of the RPC calls whereas <see cref="DeviceOperationFlags"/> and <see cref="DeviceErrorCodeValue"/>
/// are only included as members of codec classes.
/// </para>
/// </remarks>
public class DeviceLink : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceLink()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="linkId">    The device link id. </param>
    public DeviceLink( int linkId )
    {
        this.LinkId = linkId;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceLink( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceLink"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceLink"/>. </returns>
    public static DeviceLink DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceLink( decoder );
    }

    /// <summary>   The identifier of the Core device link between the client and the server. </summary>
    /// <value> The value. </value>
    public int LinkId { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.LinkId.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.LinkId = decoder.DecodeInt();
    }
}
