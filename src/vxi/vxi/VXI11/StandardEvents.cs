using System.ComponentModel;

namespace cc.isr.VXI11;

/// <summary> Enumerates the status byte flags of the standard event register. </summary>
/// <remarks>
/// Enumerates the Standard Event Status Register Bits. Read this information using ESR? or
/// status.standard.event. Use *ESE or status.standard.enable or event status enable to enable
/// this register.''' These values are used when reading or writing to the standard event
/// registers. Reading a status register returns a value. The binary equivalent of the returned
/// value indicates which register bits are set. The least significant bit of the binary number
/// is bit 0, and the most significant bit is bit 15. For example, assume value 9 is returned for
/// the enable register. The binary equivalent is
/// 0000000000001001. This value indicates that bit 0 (OPC) and bit 3 (DDE)
/// are set.
/// </remarks>
[Flags()]
public enum StandardEvents
{

    /// <summary> The None option. </summary>
    [Description( "None" )]
    None = 0,

    /// <summary>
    /// Bit B0, Operation Complete (OPC). Set bit indicates that all
    /// pending selected device operations are completed and the unit is ready to
    /// accept new commands. The bit is set in response to an *OPC command.
    /// The ICL function OPC() can be used in place of the *OPC command.
    /// </summary>
    [Description( "Operation Complete (OPC)" )]
    OperationComplete = 1,

    /// <summary>
    /// Bit B1, Request Control (RQC). Set bit indicates that....
    /// </summary>
    [Description( "Request Control (RQC)" )]
    RequestControl = 0x2,

    /// <summary>
    /// Bit B2, Query Error (QYE). Set bit indicates that you attempted
    /// to read data from an empty Output Queue.
    /// </summary>
    [Description( "Query Error (QYE)" )]
    QueryError = 0x4,

    /// <summary>
    /// Bit B3, Device-Dependent Error (DDE). Set bit indicates that a
    /// device operation did not execute properly due to some internal
    /// condition.
    /// </summary>
    [Description( "Device Dependent Error (DDE)" )]
    DeviceDependentError = 0x8,

    /// <summary>
    /// Bit B4 (16), Execution Error (EXE). Set bit indicates that the unit
    /// detected an error while trying to execute a command.
    /// This is used by QUATECH to report No Contact.
    /// </summary>
    [Description( "Execution Error (EXE)" )]
    ExecutionError = 0x10,

    /// <summary>
    /// Bit B5 (32), Command Error (CME). Set bit indicates that a
    /// command error has occurred. Command errors include:<p>
    /// IEEE-488.2 syntax error — unit received a message that does not follow
    /// the defined syntax of the IEEE-488.2 standard.  </p><p>
    /// Semantic error — unit received a command that was misspelled or received
    /// an optional IEEE-488.2 command that is not implemented.  </p><p>
    /// The device received a Group Execute Trigger (GET) inside a program
    /// message.  </p>
    /// </summary>
    [Description( "Command Error (CME)" )]
    CommandError = 0x20,

    /// <summary>
    /// Bit B6 (64), User Request (URQ). Set bit indicates that the LOCAL
    /// key on the SourceMeter front panel was pressed.
    /// </summary>
    [Description( "User Request (URQ)" )]
    UserRequest = 0x40,

    /// <summary>
    /// Bit B7 (128), Power ON (PON). Set bit indicates that the device
    /// has been turned off and turned back on since the last time this register
    /// has been read.
    /// </summary>
    [Description( "Power Toggled (PON)" )]
    PowerToggled = 0x80,
}

/// <summary>   A vxi 11 enum extensions. </summary>
/// <remarks>   2023-06-02. </remarks>
public static partial class Vxi11EnumExtensions
{
    private static byte _standardEventsAll;
    /// <summary>   Device Operation Flags all; a value that consists of all <see cref="StandardEvents"/>. </summary>
    /// <returns>   An int. </returns>
    public static byte StandardEventsAll()
    {
        if ( _standardEventsAll != 0 ) return _standardEventsAll;
        _standardEventsAll = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( StandardEvents ) ) )
        {
            _standardEventsAll |= ( byte ) ( int ) enumValue;
        }
        return _standardEventsAll;
    }

    /// <summary>   An int extension method that converts a value to a <see cref="StandardEvents"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the StandardEvents. </returns>
    public static StandardEvents ToStandardEvents( this byte value )
    {
        return Enum.IsDefined( typeof( StandardEvents ), value ) || ((value & StandardEventsAll()) == value)
            ? ( StandardEvents ) ( int ) value
            : throw new ArgumentException( $"{typeof( byte )} value of {value} cannot be cast to {nameof( StandardEvents )}" );
    }

    /// <summary>
    /// An int extension method that converts a value to a <see cref="StandardEvents"/>.
    /// </summary>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the StandardEvents. </returns>
    public static StandardEvents ToStandardEvents( this string value )
    {
        return Convert.ToByte( value ).ToStandardEvents();

    }
}
