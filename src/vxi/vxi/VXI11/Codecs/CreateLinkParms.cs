namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="CreateLinkParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.CreateLinkProcedure"/> RPC message.
/// </summary>
/// <remarks> <para>
/// </para>
///    Renamed from <c>Create_LinkParms</c>. <para>
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
        this.Device = string.Empty;
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
    /// <value> True if lock device, false if not. </value>
    public bool LockDevice { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <remarks>
    /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a lock
    /// to be released. If the device is locked by another link and the <see cref="LockTimeout"/> is non-zero,
    /// the network instrument server allows at least <see cref="LockTimeout"/> milliseconds for a lock to be 
    /// released.
    /// </remarks>
    /// <value> The time to wait on a lock. </value>
    public int LockTimeout { get; set; }

    /// <summary>   Gets or sets the interface device string, e.g., inst0, gpib,5 or usb0[...]. </summary>
    /// <value> The interface device string. </value>
    public string Device { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( this.ClientId );
        encoder.EncodeBoolean( this.LockDevice );
        encoder.EncodeInt( this.LockTimeout );
        encoder.EncodeString( this.Device );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>   2023-01-04. </remarks>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ClientId = decoder.DecodeInt();
        this.LockDevice = decoder.DecodeBoolean();
        this.LockTimeout = decoder.DecodeInt();
        this.Device = decoder.DecodeString();
    }

}
