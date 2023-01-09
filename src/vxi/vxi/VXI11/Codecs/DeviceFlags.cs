namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceFlags"/> class defines the XDR codec used with the 
/// <see cref="DeviceDoCmdParms"/>, <see cref="DeviceGenericParms"/>,
/// <see cref="DeviceLockParms"/>, <see cref="DeviceReadParms"/>, and
/// <see cref="DeviceWriteParms"/> XDR Codecs.
/// </summary>
/// <remarks>
/// Renamed from <c>Device_Flags</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// typedef long Device_Flags;
/// </code>
/// </remarks>
public class DeviceFlags : IXdrCodec
{

    /// <summary>   Gets or sets the <see cref="DeviceOperationFlags"/> value. </summary>
    /// <value> The value. </value>
    public DeviceOperationFlags Value { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceFlags()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="value">    The <see cref="DeviceOperationFlags"/> value. </param>
    public DeviceFlags( DeviceOperationFlags value )
    {
        this.Value = value;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceFlags( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( ( int ) this.Value );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Value = ( DeviceOperationFlags ) decoder.DecodeInt();
    }

}

/// <summary>   Values that represent device flags values. </summary>
/// <remarks>
/// The operation flags are passed on many of the calls to communicate additional information
/// concerning how the request is carried out. Undefined bits are reserved for future
/// use.Controllers send undefined bits as zero (0). These flags are sent from the network
/// instrument client to the network instrument server as parameters to several of the RPCs.
/// </remarks>
[Flags]
public enum DeviceOperationFlags
{
    None = 0,

    /// <summary>   An enum constant representing the wait lock option. <para>
    /// <b>Wait Lock (bit 0):</b> If the flag is set to one (1), then the network instrument server suspends (blocks) the
    /// requested operation if it cannot be performed due to a lock held by another link for at least
    /// lock_timeout milliseconds. If the flag is reset to zero (0), then the network instrument server sets the
    /// error value to 11 and returns if the operation cannot be performed due to a lock held by another link. </para>
    /// </summary>
    WaitLock = 1,

    /// <summary>   An enum constant representing the end indicator option. 
    /// <b>EOI Enabled (bit 3)</b> If the flag is set to one (1) then the last byte in the buffer is sent with an END indicator.
    /// This flag is only valid for <see cref="Vxi11MessageConstants.DeviceWriteProcedure"/>. </summary>
    EndIndicator = 8,

    /// <summary>   An enum constant representing the termination character set option. 
    /// <b>Term Char Set ( bit 7):</b> This flag is set to one (1) if a termination character is specified on a read.
    /// The actual termination character itself is passed in the <see cref="DeviceReadParms.TermChar"/> parameter. 
    /// This flag is only valid for <see cref="Vxi11MessageConstants.DeviceReadProcedure"/>.
    /// </summary>
    TerminationCharacterSet = 80
}
