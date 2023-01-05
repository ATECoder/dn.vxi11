namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="CreateLinkParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11MessageConstants.CreateLinkProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Create_LinkParms</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Create_LinkParms {
///   long clientId;   /* implementation specific value */
///   bool lockDevice; /* attempt to lock the device */
///   unsigned long lock_timeout; /* time to wait on a lock */
///   string device; /* name of device */
/// };
/// </code>
/// </remarks>
public class CreateLinkParms : IXdrCodec
{

    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    public int ClientId { get; set; }

    /// <summary>   Gets or sets a value indicating whether the device is locked. </summary>
    /// <value> True if lock device, false if not. </value>
    public bool LockDevice { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <value> The time to wait on a lock. </value>
    public int LockTimeout { get; set; }

    /// <summary>   Gets or sets the device name. </summary>
    /// <value> The device name. </value>
    public string Device { get; set; }

    public CreateLinkParms()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="xdr">  The XDR Decoding stream </param>
    public CreateLinkParms( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase xdr )
    {
        xdr.EncodeInt( this.ClientId );
        xdr.EcodeBoolean( this.LockDevice );
        xdr.EncodeInt( this.LockTimeout );
        xdr.EncodeString( this.Device );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <remarks>   2023-01-04. </remarks>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase xdr )
    {
        this.ClientId = xdr.DecodeInt();
        this.LockDevice = xdr.DecodeBoolean();
        this.LockTimeout = xdr.DecodeInt();
        this.Device = xdr.DecodeString();
    }

}
