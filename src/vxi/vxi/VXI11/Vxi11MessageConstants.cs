namespace cc.isr.VXI11;

/// <summary>   A collection of VXI-11 message constants. </summary>
/// <remarks>
/// The 17 messages define the network instrument protocol. These messages are expected to be
/// supported by all devices that claim to be network instrument compliance. <para>
/// The client identifies the remote procedure, or message, by a unique number. This number is
/// then encoded into a message along with the procedure's argument types and values. The message
/// is sent to the server machine where it is decoded by the server. The server uses the unique
/// identifier to dispatch the request. When the request is completed, the return values are
/// encoded into a message which is sent back to the client machine. </para> <para>
/// The interface definition gives the function prototypes as well as the unique identifiers for
/// the procedures. For ONC/RPC, the unique identifier is a combination of a program number (also
/// known as an interface id), a procedure number, and a version number. </para> <para>
/// The messages are sent over three different channels: a core synchronous command channel, a
/// secondary abort channel (for aborting core channel operations), and an interrupt channel.
/// </para>
/// </remarks>
internal static class Vxi11MessageConstants
{

    /// <summary>  Abort Channel: device aborts an in-progress call. <para>
    /// Renamed from <c>device_abort_1 = 1</c>. </para> </summary>
    public const int DeviceAbortProcedure = 1;

    /// <summary>  Core channel: opens a link to a device and returns a <see cref="Codecs.DeviceLink"/>. <para>
    /// <para> This link is identified on subsequent RPCs by the a <see cref="Codecs.DeviceLink"/> value
    /// that is returned from the network instrument server.</para> <para>
    /// Renamed from <c>create_link_1 = 10</c>. </para> </summary>
    public const int CreateLinkProcedure = 10;

    /// <summary>  Core channel: device receives a message; Send a message to the device. <para>
    /// Renamed from <c>device_write_1 = 11</c>. </para> </summary>
    public const int DeviceWriteProcedure = 11;

    /// <summary>  Core channel: device returns a result; Receive a message from the device. <para>
    /// Renamed from <c>device_read_1 = 12</c>. </para> </summary>
    public const int DeviceReadProcedure = 12;

    /// <summary>  Core channel: device returns its status byte. <para>
    /// Renamed from <c>device_readstb_1 = 13</c>. </para> </summary>
    public const int DeviceReadStbProcedure = 13;

    /// <summary>  Core channel: device executes a trigger. <para>
    /// Renamed from <c>device_trigger_1 = 14</c>. </para> </summary>
    public const int DeviceTriggerProcedure = 14;

    /// <summary>  Core channel: device clears itself. <para>
    /// Renamed from <c>device_clear_1 = 15</c>. </para> </summary>
    public const int DeviceClearProcedure = 15;

    /// <summary>  Core channel: device disables its front panel. <para>
    /// Renamed from <c>device_remote_1 = 16</c>. </para> </summary>
    public const int DeviceRemoteProcedure = 16;

    /// <summary>  Core channel: device enables its front panel. <para>
    /// Renamed from <c>device_local_1 = 17</c>. </para> </summary>
    public const int DeviceLocalProcedure = 17;

    /// <summary>  Core channel: device is locked. <para>
    /// Renamed from <c>device_lock_1 = 18</c>. </para> </summary>
    public const int DeviceLockProcedure = 18;

    /// <summary>  Core channel: device is unlocked. <para>
    /// Renamed from <c>device_unlock_1 = 19</c>. </para> </summary>
    public const int DeviceUnlockProcedure = 19;

    /// <summary>  Core channel: device enables/disables sending of service requests. <para>
    /// Renamed from <c>device_enable_srq_1 = 20</c>. </para> </summary>
    public const int DeviceEnableSrqProcedure = 20;

    /// <summary>  Core channel: device executes a command. <para>
    /// Renamed from <c>device_docmd_1 = 22</c>. </para> </summary>
    public const int DeviceDoCommandProcedure = 22;

    /// <summary>  Core channel: closes a link to a device. <para>
    /// This call closes the identified link. The network instrument server recovers resources
    /// associated with the link. </para> <para>
    /// Renamed from <c>destroy_link_1 = 23</c>. </para> </summary>
    public const int DestroyLinkProcedure = 23;

    /// <summary>  Core channel: device creates interrupt channel. <para>
    /// Renamed from <c>create_intr_chan_1 = 25</c>. </para> </summary>
    public const int CreateInterruptChannelProcedure = 25;

    /// <summary>  Core channel: device destroys interrupt channel. <para>
    /// Renamed from <c>destroy_intr_chan_1 = 26</c>. </para> </summary>
    public const int DestroyInterruptChannelProcedure = 26;

    /// <summary>  Interrupt Channel: used by device to send a service request. <para>
    /// Renamed from <c>device_intr_srq_1 = 30</c>. </para> </summary>
    public const int DeviceInterruptSrqProcedure = 30;

}
