namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceLink"/> class defines the request XDR
/// codec used with all device core XDR codecs such as <see cref="CreateLinkResp"/>.
/// </summary>
/// <remarks>   Renamed from <c>Device_Link</c>. <para>
///  
/// VXI-11 Specifications: </para>
/// <code>
/// typedef long Device_Link;
/// </code>
/// The network instrument server returns an identifier of type <see cref="DeviceLink"/> as a
/// result of the <see cref="Vxi11Message.CreateLinkProcedure"/> call. This identifier
/// is handed back to the network instrument server by the network instrument client on each
/// subsequent call. The network instrument server verifies the validity of the identifier on
/// each call. The <see cref="DeviceLink"/> data is not modified by the controller.
/// </remarks>
public class DeviceLink : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceLink()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="value">    The device link id. </param>
    public DeviceLink( int value )
    {
        this.Value = value;
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

    /// <summary>   The identifies of the Core device link between the client and the server. </summary>
    /// <value> The value. </value>
    public int Value { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.Value.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Value = decoder.DecodeInt();
    }
}
