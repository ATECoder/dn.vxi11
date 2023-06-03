namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceDoCmdResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11Message.DeviceDoCommandProcedure"/> RPC message.
/// </summary>
/// <remarks> 
/// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
/// have different integer representations. <para>
/// 
/// All integers defined by the VXI-11 specification are passed over the
/// network as 32-bit integers, either signed or unsigned as defined. </para><para>
/// 
/// Renamed from <c>Device_DoCmdResp</c>. </para><para>
///  
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_DoCmdResp {
/// Device_ErrorCode error; /* returned status */
/// opaque data_out{}; /* returned data parameter */
/// };
/// </code>
/// </remarks>
public class DeviceDoCmdResp : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceDoCmdResp()
    {
        this.ErrorCode = DeviceErrorCode.NoError;
        this._dataOut = Array.Empty<byte>();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceDoCmdResp( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="DeviceDoCmdResp"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="DeviceDoCmdResp"/>. </returns>
    public static DeviceDoCmdResp DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new DeviceDoCmdResp( decoder );
    }

    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error as <see cref="DeviceErrorCode"/>. </value>
    public DeviceErrorCode ErrorCode { get; set; }

    private byte[] _dataOut;

    /// <summary>   Gets data out. </summary>
    /// <returns>   An array of byte. </returns>
    public byte[] GetDataOut()
    {
        return this._dataOut;
    }

    /// <summary>   Sets data out. </summary>
    /// <param name="dataOut">  The data out. </param>
    public void SetDataOut( byte[] dataOut )
    {
        this._dataOut = dataOut;
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.ErrorCode.Encode( encoder );
        this._dataOut.EncodeDynamicOpaque( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCode = decoder.DecodeInt().ToDeviceErrorCode();
        this._dataOut = decoder.DecodeDynamicOpaque();
    }

}
