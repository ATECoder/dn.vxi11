using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11;

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
    /// 
    /// <b>Wait Lock (bit 0):</b> If the flag is set to one (1), then the network instrument server suspends (blocks) the
    /// requested operation if it cannot be performed due to a lock held by another link for at least
    /// <c>lock_timeout</c> milliseconds. If the flag is reset to zero (0), then the network instrument server sets the
    /// error value to 11 and returns if the operation cannot be performed due to a lock held by another link. </para>
    /// </summary>
    Waitlock = 1,

    /// <summary>   An enum constant representing the end indicator option. 
    /// <b>EOI Enabled (bit 3)</b> If the flag is set to one (1) then the last byte in the buffer is sent with an END indicator.
    /// This flag is only valid for <see cref="Vxi11Message.DeviceWriteProcedure"/>. </summary>
    EndIndicator = 8,

    /// <summary>   An enum constant representing the termination character set option. 
    /// <b>Term Char Set ( bit 7):</b> This flag is set to one (1) if a termination character is specified on a read.
    /// The actual termination character itself is passed in the <see cref="DeviceReadParms.TermChar"/> parameter. 
    /// This flag is only valid for <see cref="Vxi11Message.DeviceReadProcedure"/>.
    /// </summary>
    TerminationCharacterSet = 80
}

