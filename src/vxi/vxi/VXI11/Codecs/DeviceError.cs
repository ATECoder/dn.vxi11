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
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_Error</c>. </para><para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// typedef long Device_ErrorCode;
/// struct Device_Error
/// {
///     Device_ErrorCode error;
/// };
/// </code>
/// 
/// The result of any remote procedure call is a data structure whose first element has the type
/// of <see cref="DeviceErrorCode"/>. A value of <see cref="DeviceErrorCodeValue.NoError"/> (0)
/// indicates that the call was successfully completed and the results are valid. Any other value
/// indicates that during the execution of the call, the network instrument server detected an
/// error. All other error codes are reserved. <para>
///
/// DeviceFlagsCodec and DeviceErrorCodec are represented as integers, which simplifies the code
/// quite a bit and matches the VXI-11 specifications. <see cref="DeviceLink"/> codec is kept
/// even though it also is defined as a <c>typedef long</c> because Device Link is an argument in
/// some of the RPC calls whereas <see cref="DeviceOperationFlags"/> and <see cref="DeviceErrorCodeValue"/>
/// are only included as members of codec classes. </para>
/// </remarks>
public class DeviceError : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceError() : this( new DeviceErrorCode() )
    { }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="deviceErrorCode">    The <see cref="DeviceErrorCode"/> codec. </param>
    public DeviceError( DeviceErrorCode deviceErrorCode )
    {
        this._errorCode = deviceErrorCode;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceError( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceError"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceError"/>. </returns>
    public static DeviceError DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceError( decoder );
    }

    private DeviceErrorCode _errorCode;
    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error. </value>
    public DeviceErrorCode ErrorCode { get => this._errorCode; set => this._errorCode = value ?? new(); }

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
