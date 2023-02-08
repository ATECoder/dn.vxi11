using System.ComponentModel;
using System.Reflection;

using cc.isr.VXI11.Client;
using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Server;

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

    /// <summary>   An int extension method that converts a value to a <see cref="DeviceErrorCode"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the <see cref="DeviceErrorCode"/>. </returns>
    public static DeviceErrorCode ToDeviceErrorCode( this int value )
    {
        return Enum.IsDefined( typeof( DeviceErrorCode ), value )
            ? ( DeviceErrorCode ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( DeviceErrorCode )}" );
    }

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


    /// <summary>   An int extension method that converts a value to a <see cref="TransportProtocol"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the <see cref="TransportProtocol"/>. </returns>
    public static TransportProtocol ToTransportProtocol( this int value )
    {
        return Enum.IsDefined( typeof( TransportProtocol ), value )
            ? ( TransportProtocol ) value
            : throw new ArgumentException(
                $"{typeof( int )} value of {value} cannot be cast to {nameof( TransportProtocol )}" );
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

    /// <summary>   An int extension method that converts a value to a <see cref="Vxi11InstrumentOperationType"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the <see cref="Vxi11InstrumentOperationType"/>. </returns>
    public static Vxi11InstrumentOperationType ToVxi11InstrumentOperationType( this int value )
    {
        return Enum.IsDefined( typeof( Vxi11InstrumentOperationType ), value )
            ? ( Vxi11InstrumentOperationType ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( Vxi11InstrumentOperationType )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="InterfaceCommand"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the Ieee4888InterfaceCommand. </returns>
    public static InterfaceCommand ToInterfaceCommand( this int value )
    {
        return Enum.IsDefined( typeof( InterfaceCommand ), value )
            ? ( InterfaceCommand ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( InterfaceCommand )}" );
    }

    /// <summary>   An int extension method that converts a value to a <see cref="InterfaceCommandOption"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the Ieee4888InterfaceCommandOption. </returns>
    public static InterfaceCommandOption ToInterfaceCommandOption( this int value )
    {
        return Enum.IsDefined( typeof( InterfaceCommandOption ), value )
            ? ( InterfaceCommandOption ) value
            : throw new ArgumentException( $"{typeof( int )} value of {value} cannot be cast to {nameof( InterfaceCommandOption )}" );
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
