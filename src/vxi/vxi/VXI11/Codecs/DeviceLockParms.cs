namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceLockParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.DeviceLockProcedure"/> RPC message.
/// </summary>
/// <remarks> <para>
///
/// Renamed from <c>Device_LockParms</c>. </para><para>
/// 
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

    /// <summary>   Default constructor. </summary>
    public DeviceLockParms()
    {
        this._link = new();
        this._flags = new();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceLockParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceLockParms"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceLockParms"/>. </returns>
    public static DeviceLockParms DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceLockParms( decoder );
    }

    private DeviceLink _link;
    /// <summary>
    /// Gets or sets the <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    /// call.
    /// </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink Link { get => this._link; set => this._link = value ?? new(); }

    private DeviceFlags _flags;
    /// <summary>   Gets or sets the <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/> options. </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get => this._flags; set => this._flags = value ?? new(); }

    /// <summary>   Gets or sets the lock timeout; time to wait to acquire lock. </summary>
    /// <remarks>
    /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a lock
    /// to be released. If the device is locked by another link and the <see cref="LockTimeout"/> is non-zero,
    /// the network instrument server allows at least <see cref="LockTimeout"/> milliseconds for a lock to be 
    /// released.
    /// </remarks>
    /// <value> The lock timeout. </value>
    public int LockTimeout { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.Link.Encode( encoder );
        this.Flags.Encode( encoder );
        this.LockTimeout.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Link = new DeviceLink( decoder );
        this.Flags = new DeviceFlags( decoder );
        this.LockTimeout = decoder.DecodeInt();
    }

}
