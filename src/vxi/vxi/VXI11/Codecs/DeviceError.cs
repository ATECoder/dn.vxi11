namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceError"/> class defines the response XDR
/// codec for the <see cref="Vxi11Message.DeviceTriggerProcedure"/>,
/// <see cref="Vxi11Message.DeviceClearProcedure"/>,
/// <see cref="Vxi11Message.DeviceRemoteProcedure"/>,
/// <see cref="Vxi11Message.DeviceLocalProcedure"/>,
/// <see cref="Vxi11Message.DeviceLockProcedure"/>,
/// <see cref="Vxi11Message.DeviceUnlockProcedure"/>,
/// <see cref="Vxi11Message.DeviceEnableSrqProcedure"/>,
/// <see cref="Vxi11Message.DestroyLinkProcedure"/>,
/// <see cref="Vxi11Message.CreateInterruptChannelProcedure"/>, and
/// <see cref="Vxi11Message.DestroyInterruptChannelProcedure"/> RPC messages.
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

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2023-01-04. </remarks>
    public DeviceError()
    {
        this._errorCode = new();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceError( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    private DeviceErrorCode _errorCode;
    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error. </value>
    public DeviceErrorCode ErrorCode { get => this._errorCode; set => this._errorCode = value ?? new (); }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.ErrorCode.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCode = new DeviceErrorCode( decoder );
    }
}
