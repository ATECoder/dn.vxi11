namespace cc.isr.VXI11;

/// <summary>   Values that represent VXI-11 event types. </summary>
/// <remarks>   2023-01-26. </remarks>
public enum Vxi11EventType
{
    /// <summary>   An enum constant representing the none option. </summary>
    None,

    /// <summary>   An enum constant representing the service request option. </summary>
    ServiceRequest = 1,

}

/// <summary>   A vxi 11 enum extensions. </summary>
/// <remarks>   2023-06-02. </remarks>
public static partial class Vxi11EnumExtensions
{
    /// <summary>   An int extension method that converts a value to a <see cref="Vxi11EventType"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the <see cref="Vxi11EventType"/>. </returns>
    public static Vxi11EventType ToVxi11EventType( this int value )
    {
        return Enum.IsDefined( typeof( Vxi11EventType ), value )
            ? ( Vxi11EventType ) value
            : throw new ArgumentException(
                $"{typeof( int )} value of {value} cannot be cast to {nameof( Vxi11EventType )}" );
    }
}
