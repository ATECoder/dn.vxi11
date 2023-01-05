namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceLockParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceLockProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_LockParms</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_LockParms {
///    Device_Link lid;             /* link id from create_link */
///    Device_Flags flags;          /* Contains the wait lock flag */
///    unsigned long lock_timeout;  /* time to wait to acquire lock */
/// };
/// </code>
/// </remarks>
public class DeviceLockParms : IXdrCodec
{

    /// <summary>
    /// Gets or sets the identifier of the device link 
    /// (from the <see cref="Vxi11MessageConstants.CreateLinkProcedure"/>).
    /// </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink DeviceLinkId { get; set;  }

    /// <summary>   Gets or sets the flags; Contains the wait lock <see cref="DeviceFlags"/> flag </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get; set; }

    /// <summary>   Gets or sets the lock timeout; time to wait to acquire lock </summary>
    /// <value> The lock timeout. </value>
    public int LockTimeout { get; set; }


    /// <summary>   Default constructor. </summary>
    public DeviceLockParms()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceLockParms( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.DeviceLinkId.Encode( encoder );
        this.Flags.Encode( encoder );
        encoder.EncodeInt( this.LockTimeout );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.DeviceLinkId = new DeviceLink( decoder );
        this.Flags = new DeviceFlags( decoder );
        this.LockTimeout = decoder.DecodeInt();
    }

}
