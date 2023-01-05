namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceError"/> class defines the response XDR
/// codec for the <see cref="Vxi11MessageConstants.DeviceTriggerProcedure"/>,
/// <see cref="Vxi11MessageConstants.DeviceClearProcedure"/>,
/// <see cref="Vxi11MessageConstants.DeviceRemoteProcedure"/>,
/// <see cref="Vxi11MessageConstants.DeviceLocalProcedure"/>,
/// <see cref="Vxi11MessageConstants.DeviceLockProcedure"/>,
/// <see cref="Vxi11MessageConstants.DeviceUnlockProcedure"/>,
/// <see cref="Vxi11MessageConstants.DeviceEnableSrqProcedure"/>,
/// <see cref="Vxi11MessageConstants.DestroyLinkProcedure"/>,
/// <see cref="Vxi11MessageConstants.CreateInterruptChannelProcedure"/>, and
/// <see cref="Vxi11MessageConstants.DestroyInterruptChannelProcedure"/> RPC messages.
/// </summary>
/// <remarks>   Renamed from <c>Device_Error</c>. <para>
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_Error
/// {
///     Device_ErrorCode error;
/// };
/// </code>
/// </remarks>
public class DeviceError : IXdrCodec
{
    /// <summary>   Gets or sets the error (return status). </summary>
    /// <value> The error. </value>
    public DeviceErrorCode Error { get; set; }

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2023-01-04. </remarks>
    public DeviceError()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public DeviceError( XdrDecodingStreamBase xdr )
    {
        this.Decode( xdr );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase xdr )
    {
        this.Error.Encode( xdr );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="xdr">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase xdr )
    {
        this.Error = new DeviceErrorCode( xdr );
    }
}
