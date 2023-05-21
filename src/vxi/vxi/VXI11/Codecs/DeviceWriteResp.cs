using cc.isr.VXI11.EnumExtensions;

namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceWriteResp"/> class defines the response XDR codec for the <see cref="Vxi11Message.DeviceWriteProcedure"/>
/// RPC message.
/// </summary>
/// <remarks>
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those
/// hosts have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the network as 32-bit
/// integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_WriteResp</c>. </para> <para>
/// 
/// VXI-11 Specifications: </para>
/// <code>
/// typedef long Device_ErrorCode;
/// struct device_write_resp {
///    Device_ErrorCode error;
///    unsigned long size; /* Number of bytes written */
/// };
/// </code>
/// 
/// DeviceFlagsCodec and DeviceErrorCodeCodec are represented as integers, which simplifies the code
/// quite a bit and matches the VXI-11 specifications. <see cref="DeviceLink"/> codec is kept
/// even though it also is defined as a <c>typedef long</c> because Device Link is an argument in
/// some of the RPC calls whereas <see cref="DeviceOperationFlags"/> and <see cref="DeviceErrorCode"/>
/// are only included as members of codec classes.
/// </remarks>
public class DeviceWriteResp : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceWriteResp()
    { }
    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceWriteResp( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceWriteResp"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceWriteResp"/>. </returns>
    public static DeviceWriteResp DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceWriteResp( decoder );
    }

    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error as <see cref="DeviceErrorCode"/>. </value>
    public DeviceErrorCode ErrorCode { get; set; }

    /// <summary>   Gets or sets the size; the number of bytes written. </summary>
    /// <remarks>
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned integer because the written size is not expected to exceed the maximum
    /// integer value.
    /// </remarks>
    /// <value> The number of bytes written. </value>
    public int Size { get; set; }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.ErrorCode.Encode( encoder );
        this.Size.Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCode = decoder.DecodeInt().ToDeviceErrorCode();
        this.Size = decoder.DecodeInt();
    }

}
