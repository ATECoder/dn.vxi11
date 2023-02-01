using System.ComponentModel;

using cc.isr.VXI11.EnumExtensions;

namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceErrorCode"/> class defines the XDR codec used with the <see cref="DeviceErrorCode"/>
/// XDR Codec.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_ErrorCode</c>. </para><para>
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
/// 
/// DeviceFlagsCodec and DeviceErrorCodec are represented as integers, which simplifies the code
/// quite a bit and matches the VXI-11 specifications. <see cref="DeviceLink"/> codec is kept
/// even though it also is defined as a <c>typedef long</c> because Device Link is an argument in
/// some of the RPC calls whereas <see cref="DeviceOperationFlags"/> and <see cref="DeviceErrorCodeValue"/>
/// are only included as members of codec classes.
///
/// </remarks>
public class DeviceErrorCode : IXdrCodec
{
    /// <summary>   Default constructor. </summary>
    public DeviceErrorCode()
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="errorCode">    The error code value. </param>
    public DeviceErrorCode( DeviceErrorCodeValue errorCode )
    {
        this.ErrorCodeValue = errorCode;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceErrorCode( XdrDecodingStreamBase decoder )
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceErrorCode"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceErrorCode"/>. </returns>
    public static DeviceErrorCode DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceErrorCode( decoder );
    }

    /// <summary>   Gets or set the <see cref="DeviceErrorCodeValue"/>. </summary>
    /// <value> The <see cref="DeviceErrorCodeValue"/>. </value>
    public DeviceErrorCodeValue ErrorCodeValue { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        (( int ) this.ErrorCodeValue).Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCodeValue = decoder.DecodeInt().ToDeviceErrorCodeValue();
    }
}

