using System.ComponentModel;

namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceErrorCode"/> class defines the XDR codec used with the <see cref="DeviceErrorCode"/>
/// XDR Codec.
/// </summary>
/// <remarks>
/// Renamed from <c>Device_ErrorCode</c>. <para>
///  
/// VXI-11 Specifications: </para>
/// <code>
/// typedef long Device_ErrorCode;
/// </code>
/// The result of any remote procedure call is a data structure whose first element has the type
/// of <see cref="DeviceErrorCode"/>. A value of <see cref="DeviceErrorCodeValue.NoError"/> (0)
/// indicates that the call was successfully completed and the results are valid. Any other value
/// indicates that during the execution of the call, the network instrument server detected an
/// error. All other error codes are reserved.
/// </remarks>
public class DeviceErrorCode : IXdrCodec
{
    /// <summary>   The <see cref="DeviceErrorCodeValue"/>. </summary>
    public DeviceErrorCodeValue Value { get; set; }

    /// <summary>   Default constructor. </summary>
    public DeviceErrorCode()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="value">    The value. </param>
    public DeviceErrorCode( DeviceErrorCodeValue value )
    {
        this.Value = value;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceErrorCode( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( ( int ) this.Value );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.Value = ( DeviceErrorCodeValue ) decoder.DecodeInt();
    }
}

/// <summary>   Values that represent device error code values. </summary>
/// <remarks>   2023-01-07. </remarks>
public enum DeviceErrorCodeValue
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
}
