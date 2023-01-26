using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="DeviceErrorEventArgs"/> defines the event arguments for the 
/// an event fired by the <see cref="AbortChannelServer">Abort server</see> whenever replies to a
/// <see cref="AbortChannelClient.DeviceAbort(cc.isr.VXI11.Codecs.DeviceLink)"/> message is received.
/// </summary>
public class DeviceErrorEventArgs : EventArgs
{

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2023-01-26. </remarks>
    public DeviceErrorEventArgs() : this( 0 )
    { }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="deviceLinkId"> Identifier for the device link. </param>
    public DeviceErrorEventArgs( int deviceLinkId ) : this( DeviceErrorCodeValue.NoError, deviceLinkId )
    { }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="errorCodeValue">   The error code value. </param>
    /// <param name="deviceLinkId">     Identifier for the device link. </param>
    public DeviceErrorEventArgs(  DeviceErrorCodeValue errorCodeValue, int deviceLinkId )
    {
        this.ErrorCodeValue = errorCodeValue;
        this.DeviceLinkId = deviceLinkId;
    }

    /// <summary>   Gets or sets the error code value. </summary>
    /// <value> The error code value. </value>
    public DeviceErrorCodeValue ErrorCodeValue { get; set; }

    public int DeviceLinkId { get; private set; }
}
