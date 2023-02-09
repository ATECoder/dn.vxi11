using System.ComponentModel;

namespace cc.isr.VXI11
{
    /// <summary>   Values that represent device error codes. </summary>
    /// <remarks>
    /// Negative values were added to address errors not defined by the VXI-11 specifications for
    /// exceptions such as <see cref="NotImplementedException"/>.
    /// </remarks>
    public enum DeviceErrorCode
    {
        /// <summary>   An enum constant representing the no error option. </summary>
        [Description( "No error." )] NoError = 0,

        /// <summary>   An enum constant representing the syntax error option. </summary>
        [Description( "Syntax error." )]
        SyntaxError = 1,

        /// <summary>   An enum constant representing the device not accessible option. </summary>
        [Description( "Device not accessible." )]
        DeviceNotAccessible = 3,

        /// <summary>   An enum constant representing the invalid link identifier option. </summary>
        [Description( "Invalid link identifier." )]
        InvalidLinkIdentifier = 4,

        /// <summary>   An enum constant representing the parameter error option. </summary>
        [Description( "Parameter error." )]
        ParameterError = 5,

        /// <summary>   An enum constant representing the channel not established option. </summary>
        [Description( "Channel not Established." )]
        ChannelNotEstablished = 6,

        /// <summary>   An enum constant representing the operation not supported option. </summary>
        [Description( "Operation not supported." )]
        OperationNotSupported = 8,

        /// <summary>   An enum constant representing the out of resources option. </summary>
        [Description( "Out of resources." )]
        OutOfResources = 9,

        /// <summary>   An enum constant representing the device locked by another link option. </summary>
        [Description( "Device locked by another link,." )]
        DeviceLockedByAnotherLink = 11,

        /// <summary>   An enum constant representing the no lock held by this link option. </summary>
        [Description( "No lock held by this link." )]
        NoLockHeldByThisLink = 12,

        /// <summary>   An enum constant representing the I/O timeout option. </summary>
        [Description( "I/O timeout." )]
        IOTimeout = 15,

        /// <summary>   An enum constant representing the I/O error option. </summary>
        [Description( "I/O error." )]
        IOError = 17,

        /// <summary>   An enum constant representing the invalid address option. </summary>
        [Description( "Invalid address." )]
        InvalidAddress = 21,

        /// <summary>   An enum constant representing the abort option. </summary>
        [Description( "Abort." )]
        Abort = 23,

        /// <summary>   An enum constant representing the channel already established option. </summary>
        [Description( "Channel already established." )]
        ChannelAlreadyEstablished = 29,

        /// <summary>   An enum constant representing the procedure not implemented option. </summary>
        [Description( "Procedure not implemented." )]
        NoImplemented = -1,

    }
}

namespace cc.isr.VXI11.EnumExtensions
{
    public static partial class Vxi11EnumExtensions
    {

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

    }
}



