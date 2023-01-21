using System.ComponentModel;
using System.Reflection;

namespace cc.isr.VXI11.IEEE488.EnumExtensions;

/// <summary>   A support class for VXI-11 IEEE 488 enum extensions. </summary>
public static class Ieee488EnumExtensions
{

    /// <summary>   Gets a description from an Enum. </summary>
    /// <param name="value">    An enum constant representing the value option. </param>
    /// <returns>   The description. </returns>
    public static string GetDescription( this Enum value )
    {
        return
            value
                .GetType()
                .GetMember( value.ToString() )
                .FirstOrDefault()
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description
            ?? value.ToString();
    }

    /// <summary>   An int extension method that converts a value to a <see cref="Ieee488OperationType"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the Ieee4888OperationType. </returns>
    public static Ieee488OperationType ToIeee488OperationType( this int value )
    {
        return Enum.IsDefined( typeof( Ieee488OperationType ), value )
            ? ( Ieee488OperationType ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( Ieee488OperationType )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="Ieee488InterfaceCommand"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the Ieee4888InterfaceCommand. </returns>
    public static Ieee488InterfaceCommand ToIeee488InterfaceCommand( this int value )
    {
        return Enum.IsDefined( typeof( Ieee488InterfaceCommand ), value )
            ? ( Ieee488InterfaceCommand ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( Ieee488InterfaceCommand )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="Ieee488InterfaceCommandOption"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the Ieee4888InterfaceCommandOption. </returns>
    public static Ieee488InterfaceCommandOption ToIeee488InterfaceCommandOption( this int value )
    {
        return Enum.IsDefined( typeof( Ieee488InterfaceCommandOption ), value )
            ? ( Ieee488InterfaceCommandOption ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( Ieee488InterfaceCommandOption )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="GpibCommandArgument"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the Ieee4888OperationType. </returns>
    public static GpibCommandArgument ToGpibCommandArgument( this int value )
    {
        return Enum.IsDefined( typeof( GpibCommandArgument ), value )
            ? ( GpibCommandArgument ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( GpibCommandArgument )}" );
    }

}
