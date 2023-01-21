using System.ComponentModel;
using System.Reflection;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.EnumExtensions;

/// <summary>   A support class for VXI-11 enum extensions. </summary>
public static class Vxi11EnumExtensions
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

    /// <summary>   An int extension method that converts a value to a <see cref="DeviceErrorCodeValue"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the DeviceErrorCodeValue. </returns>
    public static DeviceErrorCodeValue ToDeviceErrorCodeValue( this int value )
    {
        return Enum.IsDefined( typeof( DeviceErrorCodeValue ), value )
            ? ( DeviceErrorCodeValue ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( DeviceErrorCodeValue )}" );
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

    private static int _allDeviceOperationFlags;
    /// <summary>   Device Operation Flags all; a value that consists of all <see cref="DeviceOperationFlags"/>. </summary>
    /// <returns>   An int. </returns>
    public static int DeviceOperationFlagsAll()
    {
        if ( _allDeviceOperationFlags != 0 ) return _allDeviceOperationFlags;
        _allDeviceOperationFlags = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceOperationFlags ) ) )
        {
            _allDeviceOperationFlags |= ( int ) enumValue;
        }
        return _allDeviceOperationFlags;
    }

    /// <summary>   An int extension method that converts a value to a <see cref="DeviceOperationFlags"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the DeviceOperationFlags. </returns>
    public static DeviceOperationFlags ToDeviceOperationFlags( this int value )
    {
        return Enum.IsDefined( typeof( DeviceOperationFlags ), value ) || ((value & DeviceOperationFlagsAll()) == value)
            ? ( DeviceOperationFlags ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( DeviceOperationFlags )}" );
    }


    private static int _allDeviceReasons;
    /// <summary>   Device reasons all; a value that consists of all <see cref="DeviceReadReasons"/>. </summary>
    /// <returns>   An int. </returns>
    public static int DeviceReasonsAll()
    {
        if ( _allDeviceReasons != 0 ) return _allDeviceReasons;
        _allDeviceReasons = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( DeviceReadReasons ) ) )
        {
            _allDeviceReasons |= ( int ) enumValue;
        }
        return _allDeviceReasons;
    }

    /// <summary>   An int extension method that converts a value to a <see cref="DeviceReadReasons"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the DeviceReadReasons. </returns>
    public static DeviceReadReasons ToDeviceReadReasons( this int value )
    {
        return Enum.IsDefined( typeof( DeviceReadReasons ), value ) || ((value & DeviceReasonsAll()) == value)
            ? ( DeviceReadReasons ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( DeviceReadReasons )}" );
    }

}
