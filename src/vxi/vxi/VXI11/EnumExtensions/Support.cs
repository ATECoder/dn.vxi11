using System.ComponentModel;
using System.Reflection;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.EnumExtensions;

/// <summary>   A support class for enum extensions. </summary>
public static class Support
{

    /// <summary>   Gets a description from an Enum. </summary>
    /// <remarks>   2023-01-07. </remarks>
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

    /// <summary>   An int extension method that converts a value to a <see cref="DeviceErrorCodeValue"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the DeviceErrorCodeValue. </returns>
    public static DeviceErrorCodeValue ToDeviceErrorCodeValue( this int value )
    {
        return Enum.IsDefined( typeof( DeviceErrorCodeValue ), value )
            ? ( DeviceErrorCodeValue ) value
            : throw new ArgumentException($"{typeof( int )} value of {value} cannot be cast to {nameof( DeviceErrorCodeValue )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="DeviceAddrFamily"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the DeviceAddrFamily. </returns>
    public static DeviceAddrFamily ToDeviceAddrFamily( this int value )
    {
        return Enum.IsDefined( typeof( DeviceAddrFamily ), value )
            ? ( DeviceAddrFamily ) value
            : throw new ArgumentException(
                $"{typeof( int )} value of {value} cannot be cast to {nameof( DeviceAddrFamily )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="DeviceOperationFlags"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the DeviceOperationFlags. </returns>
    public static DeviceOperationFlags ToDeviceOperationFlags( this int value )
    {
        return Enum.IsDefined( typeof( DeviceOperationFlags ), value )
            ? ( DeviceOperationFlags ) value
            : throw new ArgumentException($"{typeof( int )} value of {value} cannot be cast to {nameof( DeviceOperationFlags )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="DeviceReadReasons"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the DeviceReadReasons. </returns>
    public static DeviceReadReasons ToDeviceReadReasons( this int value )
    {
        return Enum.IsDefined( typeof( DeviceReadReasons ), value )
            ? ( DeviceReadReasons ) value
            : throw new ArgumentException($"{typeof( int )} value of {value} cannot be cast to {nameof( DeviceReadReasons )}" );
    }

}
