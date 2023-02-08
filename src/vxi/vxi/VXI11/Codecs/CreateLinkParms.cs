namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="CreateLinkParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.CreateLinkProcedure"/> RPC message.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Create_LinkParms</c>. </para><para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// struct Create_LinkParms {
///   long clientId;              /* implementation specific value */
///   bool lockDevice;            /* attempt to lock the device */
///   unsigned long lock_timeout; /* time to wait on a lock */
///   string device;              /* name of device */
/// };
/// </code>
/// </remarks>
public class CreateLinkParms : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public CreateLinkParms()
    {
        this.DeviceName = string.Empty;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  The XDR Decoding stream </param>
    public CreateLinkParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="CreateLinkParms"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="CreateLinkParms"/>. </returns>
    public static CreateLinkParms DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new CreateLinkParms( decoder );
    }

    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    public int ClientId { get; set; }

    /// <summary>   Gets or sets a value indicating whether the device is locked. </summary>
    /// <remarks>
    /// <see cref="bool"/> types are encoded as <see cref="int"/> with 1 is <see langword="true"/>.
    /// </remarks>
    /// <value> True if lock device, false if not. </value>
    public bool LockDevice { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <remarks>
    /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a
    /// lock to be released. If the device is locked by another link and the <see cref="LockTimeout"/>
    /// is non-zero, the network instrument server allows at least <see cref="LockTimeout"/>
    /// milliseconds for a lock to be released. <para>
    /// 
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned integer because the timeout value is unlikely to exceed the maximum integer
    /// value.
    /// </para>
    /// </remarks>
    /// <value> The time to wait on a lock. </value>
    public int LockTimeout { get; set; }

    /// <summary>
    /// Gets or sets the device name also called device name, e.g., inst0, gpib,5 or
    /// usb0[...].
    /// </summary>
    /// <value> The device name. </value>
    public string DeviceName { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( this.ClientId );
        encoder.EncodeBoolean( this.LockDevice );
        encoder.EncodeInt( this.LockTimeout );
        encoder.EncodeString( this.DeviceName );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ClientId = decoder.DecodeInt();
        this.LockDevice = decoder.DecodeBoolean();
        this.LockTimeout = decoder.DecodeInt();
        this.DeviceName = decoder.DecodeString();
    }

}
