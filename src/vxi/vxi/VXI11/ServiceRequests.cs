using System.ComponentModel;
namespace cc.isr.VXI11;

/// <summary> Gets or sets the status byte bits of the service request register. </summary>
/// <remarks>
/// Enumerates the Status Byte Register Bits. Use *STB? or status.request_event to read this
/// register. Use *SRE or status.request_enable to enable these services. This attribute is used
/// to read the status byte, which is returned as a numeric value. The binary equivalent of the
/// returned value indicates which register bits are set. <para>
/// (c) 2005 Integrated Scientific Resources, Inc. All rights reserved. </para><para>
/// Licensed under The MIT License. </para>
/// </remarks>
[Flags()]
public enum ServiceRequests
{

    /// <summary> The None option. </summary>
    [Description( "None" )]
    None = 0,

    /// <summary>
    /// Bit B0, Measurement Summary Bit (MSB). Set summary bit indicates
    /// that an enabled measurement event has occurred.
    /// </summary>
    [Description( "Measurement Summary Bit (MSB)" )]
    MeasurementEvent = 0x1,

    /// <summary>
    /// Bit B1, System Summary Bit (SSB). Set summary bit indicates
    /// that an enabled system event has occurred.
    /// </summary>
    [Description( "System Summary Bit (SSB)" )]
    SystemEvent = 0x2,

    /// <summary>
    /// Bit B2, Error Available (EAV). Set summary bit indicates that
    /// an error or status message is present in the Error Queue.
    /// </summary>
    [Description( "Error Available (EAV)" )]
    ErrorAvailable = 0x4,

    /// <summary>
    /// Bit B3, Questionable Summary Bit (QSB). Set summary bit indicates
    /// that an enabled questionable event has occurred.
    /// </summary>
    [Description( "Questionable Summary Bit (QSB)" )]
    QuestionableEvent = 0x8,

    /// <summary>
    /// Bit B4 (16), Message Available (MAV). Set summary bit indicates that
    /// a response message is present in the Output Queue.
    /// </summary>
    [Description( "Message Available (MAV)" )]
    MessageAvailable = 0x10,

    /// <summary>Bit B5, Event Summary Bit (ESB). Set summary bit indicates
    /// that an enabled standard event has occurred.
    /// </summary>
    [Description( "Event Summary Bit (ESB)" )]
    StandardEvent = 0x20, // (32) ESB

    /// <summary>
    /// Bit B6 (64), Request Service (RQS)/Master Summary Status (MSS).
    /// Set bit indicates that an enabled summary bit of the Status Byte Register
    /// is set. Depending on how it is used, Bit B6 of the Status Byte Register
    /// is either the Request for Service (RQS) bit or the Master Summary Status
    /// (MSS) bit: When using the GPIB serial poll sequence of the unit to obtain
    /// the status byte (serial poll byte), B6 is the RQS bit. When using
    /// status.condition or the *STB? common command to read the status byte,
    /// B6 is the MSS bit.
    /// </summary>
    [Description( "Request Service (RQS)/Master Summary Status (MSS)" )]
    RequestingService = 0x40,

    /// <summary>
    /// Bit B7 (128), Operation Summary (OSB). Set summary bit indicates that
    /// an enabled operation event has occurred.
    /// </summary>
    [Description( "Operation Summary Bit (OSB)" )]
    OperationEvent = 0x80,
}

public static partial class Vxi11EnumExtensions
{
    private static byte _serviceRequestsAll;
    /// <summary>   Device Operation Flags all; a value that consists of all <see cref="ServiceRequests"/>. </summary>
    /// <returns>   An int. </returns>
    public static byte ServiceRequestsAll()
    {
        if ( _serviceRequestsAll != 0 ) return _serviceRequestsAll;
        _serviceRequestsAll = 0;
        foreach ( var enumValue in Enum.GetValues( typeof( ServiceRequests ) ) )
        {
            _serviceRequestsAll |= ( byte ) ( int ) enumValue;
        }
        return _serviceRequestsAll;
    }

    /// <summary>   An int extension method that converts a value to a <see cref="ServiceRequests"/>. </summary>
    /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
    ///                                         illegal values. </exception>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the ServiceRequests. </returns>
    public static ServiceRequests ToServiceRequests( this byte value )
    {
        return Enum.IsDefined( typeof( ServiceRequests ), value ) || ((value & ServiceRequestsAll()) == value)
            ? ( ServiceRequests ) ( int ) value
            : throw new ArgumentException( $"{typeof( byte )} value of {value} cannot be cast to {nameof( ServiceRequests )}" );
    }

    /// <summary>
    /// An int extension method that converts a value to a <see cref="ServiceRequests"/>.
    /// </summary>
    /// <remarks>   2023-02-08. </remarks>
    /// <param name="value">    An enum constant representing the enum value. </param>
    /// <returns>   Value as the ServiceRequests. </returns>
    public static ServiceRequests ToServiceRequests( this string value )
    {
        return Convert.ToByte( value ).ToServiceRequests();

    }
}
