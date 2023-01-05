namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceReadParms"/> class defines the request XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceReadProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_ReadParms</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_ReadParms {
///    Device_Link lid;            /* link id from create_link */
///    unsigned long requestSize;  /* Bytes requested */
///    unsigned long io_timeout;   /* time to wait for I/O */
///    unsigned long lock_timeout; /* time to wait for lock */
///    Device_Flags flags;
///    char termChar;              /* valid if flags and 'Term Char Set' */
/// };
/// </code>
/// </remarks>
public class DeviceReadParms : IXdrCodec
{
    /// <summary>
    /// Gets or sets the identifier of the device link 
    /// (from the <see cref="Vxi11MessageConstants.CreateLinkProcedure"/>).
    /// </summary>
    /// <value> The identifier of the device link. </value>
    public DeviceLink DeviceLinkId { get; set; }

    /// <summary>   Gets or sets the request size (number of bytes). </summary>
    /// <value> The size of the request. </value>
    public int RequestSize { get; set; }

    /// <summary>   Gets or sets the i/o timeout. </summary>
    /// <remarks>
    /// The i/o timeout value determines how long a network instrument server allows an I/O operation to take.
    /// </remarks>
    /// <value> The i/o timeout. </value>
    public int IOTimeout { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <remarks>
    /// The lock timeout determines how long a network instrument server will wait for a lock to be released.
    /// Units for both are in milliseconds.
    /// </remarks>
    /// <value> The lock timeout. </value>
    public int LockTimeout { get; set; }

    /// <summary>   Gets or sets the flags with options. </summary>
    /// <value> The flags. </value>
    public DeviceFlags Flags { get; set; }

    /// <summary>   Gets or sets the term character; valid if flags and 'Term Char Set' </summary>
    /// <value> The term character. </value>
    public byte TermChar { get; set; }


    public DeviceReadParms()
    {
    }

    public DeviceReadParms( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    public void Encode( XdrEncodingStreamBase xdr )
    {
        this.DeviceLinkId.Encode( xdr );
        xdr.EncodeInt( this.RequestSize );
        xdr.EncodeInt( this.IOTimeout );
        xdr.EncodeInt( this.LockTimeout );
        this.Flags.Encode( xdr );
        xdr.EncodeByte( this.TermChar );
    }

    public void Decode( XdrDecodingStreamBase xdr )
    {
        this.DeviceLinkId = new DeviceLink( xdr );
        this.RequestSize = xdr.DecodeInt();
        this.IOTimeout = xdr.DecodeInt();
        this.LockTimeout = xdr.DecodeInt();
        this.Flags = new DeviceFlags( xdr );
        this.TermChar = xdr.DecodeByte();
    }

}
