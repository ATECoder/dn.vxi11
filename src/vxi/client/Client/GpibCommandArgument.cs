namespace cc.isr.VXI11.Client;

/// <summary>   Values that represent gpib command arguments. </summary>
/// <remarks>
/// Quite often, a SAD is expressed as the secondary address plus 96, so (from the previous
/// example) SAD 4 would be equivalent to SAD 100 (4 plus 96). Why the difference in notation?
/// The secondary address is actually a value from 0 to 30, just like the primary address, but
/// when you send address information across the GPIB, you send it as a byte (8 bits) of
/// information: the first five bits make up the address, the next two bits are used to make
/// Talker/Listener assignments, and the last bit is not used (so it is set to zero). For primary
/// addresses, only one Talker/Listener bit is used (so a board or a device is either sending
/// data or receiving data). But for secondary addresses, both Talker/Listener bits are set high
/// (to indicate that it is a secondary address). As it happens, 96 decimal (60 hex) is the value
/// of SAD 0 as expressed in this byte of information. All other secondary addresses are just 96
/// plus the SAD, for a total range of 96 to 126 for the SAD.
/// https://knowledge.ni.com/KnowledgeArticleDetails?id=kA03q000000x2XyCAI
/// </remarks>
public enum GpibCommandArgument
{
    /// <summary>   An enum constant representing the go to local argument. <para>
    ///
    /// GPIB_CMD_GTL
    /// </para> </summary>
    GoToLocal = 0x01,

    /// <summary>   An enum constant representing the selected device clear argument. <para>
    ///
    /// GPIB_CMD_SDC
    /// </para> </summary>
    SelectedDeviceClear = 0x04,

    /// <summary>   An enum constant representing the parallel poll config argument. <para>
    ///
    /// GPIB_CMD_PPC
    /// </para></summary>
    ParallelPollConfig = 0x05,

    /// <summary>   An enum constant representing the group execute trigger argument. <para>
    ///
    /// GPIB_CMD_GET
    /// </para></summary>
    GroupExecuteTrigger = 0x08,

    /// <summary>   An enum constant representing the take control argument. <para>
    ///
    /// GPIB_CMD_TCT
    /// </para></summary>
    TakeControl = 0x09,

    /// <summary>   An enum constant representing the local lockout argument. <para>
    ///
    /// GPIB_CMD_LLO
    /// </para></summary>
    LocalLockout = 0x11,

    /// <summary>   An enum constant representing the device clear argument. <para>
    ///
    /// GPIB_CMD_DCL
    /// </para></summary>
    DeviceClear = 0x14,

    /// <summary>   An enum constant representing the parallel poll un-configure argument. <para>
    ///
    /// GPIB_CMD_PPU
    /// </para> </summary>
    ParallelPollUnconfigure = 0x15,

    /// <summary>   serial poll enable argument. <para>
    ///
    /// GPIB_CMD_SPE
    /// </para></summary>
    SerialPollEnable = 0x18,

    /// <summary>   An enum constant representing the serial poll disable argument. <para>
    ///
    /// GPIB_CMD_SPD
    /// </para></summary>
    SerialPollDisable = 0x19,

    /// <summary>   An enum constant representing the listen address (base) argument. <para>
    ///
    /// GPIB_CMD_LAD
    /// </para></summary>
    ListenAddress = 0x20,

    /// <summary>   unlisten argument. <para>
    ///
    /// GPIB_CMD_UNL
    /// </para></summary>
    Unlisten = 0x3F,

    /// <summary>   An enum constant representing the talk address (base) argument. <para>
    ///
    /// GPIB_CMD_TAD
    /// </para></summary>
    TalkAddress = 0x40,

    /// <summary>   An enum constant representing the untalk argument. <para>
    ///
    /// GPIB_CMD_UNT
    /// </para></summary>
    Untalk = 0x5F,

    /// <summary>   An enum constant representing the secondary address argument (base). <para>
    /// If you are using an IEEE 488.2 function, such as Send or Receive, and you need to use a 
    /// secondary address, you can use the create a packed address for you. The packed address 
    /// consists of two bytes of information: a high byte (secondary address) and a low byte 
    /// (primary address). You can express this packed address in hexadecimal, where the SAD 
    /// is a value from 60 hex (96 decimal) to 7E hex (126 decimal) and the PAD is a value from 
    /// 0 hex (0 decimal) to 1E hex (30 decimal).
    ///
    /// GPIB_CMD_SAD
    /// </para></summary>
    SecondaryAddress = 0x60,

    /// <summary>   An enum constant representing the parallel poll enable argument (base). <para>
    ///
    /// GPIB_CMD_PPE
    /// </para></summary>
    ParallelPollEnable = 0x60,

    /// <summary>   An enum constant representing the parallel poll disable argument. <para>
    ///
    /// GPIB_CMD_PPD
    /// </para></summary>
    ParallelPollDisable = 0x70,
}

/// <summary>   A vxi 11 enum extensions. </summary>
/// <remarks>   2023-06-02. </remarks>
public static partial class Vxi11ClientEnumExtensions
{
    /// <summary>   An int extension method that converts a value to a <see cref="GpibCommandArgument"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the IEEE488OperationType. </returns>
    public static GpibCommandArgument ToGpibCommandArgument( this int value )
    {
        return Enum.IsDefined( typeof( GpibCommandArgument ), value )
            ? ( GpibCommandArgument ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( GpibCommandArgument )}" );
    }

}

