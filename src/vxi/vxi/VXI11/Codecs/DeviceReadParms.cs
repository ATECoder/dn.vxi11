using cc.isr.VXI11.EnumExtensions;

namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceReadParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11Message.DeviceReadProcedure"/> RPC message.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_ReadParms</c>. </para><para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// typedef long Device_Flags;
/// typedef long Device_Link;
/// struct Device_ReadParms {
///    Device_Link lid;            /* link id from create_link */
///    unsigned long requestSize;  /* Bytes requested */
///    unsigned long io_timeout;   /* time to wait for I/O */
///    unsigned long lock_timeout; /* time to wait for lock */
///    Device_Flags flags;
///    char termChar;              /* valid if flags and 'Term Char Set' */
/// };
/// </code>
/// 
/// DeviceFlagsCodec and DeviceErrorCodeCodec are represented as integers, which simplifies the code
/// quite a bit and matches the VXI-11 specifications. <see cref="DeviceLink"/> codec is kept
/// even though it also is defined as a <c>typedef long</c> because Device Link is an argument in
/// some of the RPC calls whereas <see cref="DeviceOperationFlags"/> and <see cref="DeviceErrorCode"/>
/// are only included as members of codec classes.
/// </remarks>
public class DeviceReadParms : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceReadParms()
    {
        this._link = new();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceReadParms( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceReadParms"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceReadParms"/>. </returns>
    public static DeviceReadParms DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceReadParms( decoder );
    }

    private DeviceLink _link;
    /// <summary>   Gets or sets the <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/> call. </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink Link { get => this._link; set => this._link = value ?? new(); }

    /// <summary>   Gets or sets the request size (number of bytes). </summary>
    /// <value> The size of the request. </value>
    public int RequestSize { get; set; }

    /// <summary>   Gets or sets the i/o timeout. </summary>
    /// <remarks>
    /// The <see cref="IOTimeout"/> determines how long a network instrument server allows an I/O
    /// operation to take. If the <see cref="IOTimeout"/> is non-zero, the network instrument server
    /// allows at least <see cref="IOTimeout"/> milliseconds before returning control to the client
    /// with a timeout error. The time it takes for the I/O operation to complete does not include
    /// any time spent waiting for the lock. <para>
    /// 
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned integer because the timeout value is unlikely to exceed the maximum integer
    /// value. </para>
    /// </remarks>
    /// <value> The i/o timeout. </value>
    public int IOTimeout { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <remarks>
    /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a
    /// lock to be released. If the device is locked by another link and the <see cref="LockTimeout"/>
    /// is non-zero, the network instrument server allows at least <see cref="LockTimeout"/>
    /// milliseconds for a lock to be released. <para>
    /// 
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned integer because the timeout value is unlikely to exceed the maximum integer
    /// value. </para>
    /// </remarks>
    /// <value> The lock timeout. </value>
    public int LockTimeout { get; set; }

    /// <summary>   Gets or sets the <see cref="DeviceOperationFlags"/> options. </summary>
    /// <value> The flags. </value>
    public DeviceOperationFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the termination character; valid if flags <see cref="DeviceOperationFlags.TerminationCharacterSet"/>
    /// is set.
    /// </summary>
    /// <value> The term character. </value>
    public byte TermChar { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.Link.Encode( encoder );
        this.RequestSize.Encode( encoder );
        this.IOTimeout.Encode( encoder );
        this.LockTimeout.Encode( encoder );
        this.Flags.Encode( encoder );
        this.TermChar.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Link = new DeviceLink( decoder );
        this.RequestSize = decoder.DecodeInt();
        this.IOTimeout = decoder.DecodeInt();
        this.LockTimeout = decoder.DecodeInt();
        this.Flags = decoder.DecodeInt().ToDeviceOperationFlags();
        this.TermChar = decoder.DecodeByte();
    }

}
