namespace cc.isr.VXI11.Codecs;

/// <summary>
/// The <see cref="DeviceReadResp"/> class defines the response XDR
/// codec for the <see cref="Vxi11Message.DeviceReadProcedure"/> RPC message.
/// </summary>
/// <remarks>   Renamed from <c>Device_ReadResp</c>. <para>
///  
/// VXI-11 Specifications: </para>
/// <code>
/// struct Device_ReadResp {
///   Device_ErrorCode error;
///   long reason; /* Reason(s) read completed */
///   opaque data; /* data.len and data.val */
/// };
/// </code>
/// </remarks>
public class DeviceReadResp : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public DeviceReadResp()
    {
        this._data = Array.Empty<byte>();
        this._errorCode = new();
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public DeviceReadResp( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    private DeviceErrorCode _errorCode;
    /// <summary>   Gets or sets the <see cref="DeviceErrorCode"/> (return status). </summary>
    /// <value> The error. </value>
    public DeviceErrorCode ErrorCode { get => this._errorCode; set => this._errorCode = value ?? new(); }

    /// <summary>   Gets or sets the reason(s) read completed as defined in <see cref="DeviceReadReasons"/>. </summary>
    /// <remarks>
    /// If reason is not set (value of 0) and error is zero, then the network instrument client could issue
    /// <see cref="Vxi11Message.DeviceReadProcedure"/> calls until one of the other three termination
    /// conditions is encountered.
    /// </remarks>
    /// <value> The reason(s) read completed. </value>
    public DeviceReadReasons Reason { get; set; }

    private byte[] _data;

    /// <summary>   Gets the data. </summary>
    /// <remarks>
    /// In the case of <see cref="Vxi11Message.DeviceWriteProcedure"/> and <see cref="Vxi11Message.DeviceReadProcedure"/> 
    /// the XDR opaque type is used by the network instrument protocol not because the data being represented is truly opaque, 
    /// but to avoid the overhead associated with character data (8 bits being promoted to 32 bits). Since the data parameters for
    /// <see cref="Vxi11Message.DeviceWriteProcedure"/> and <see cref="Vxi11Message.DeviceReadProcedure"/> are arrays, 
    /// a structure is passed which contains a pointer to the data, data.data_val, and the number of elements, data.data_len. <para>
    /// 
    /// A 'RESPONSE MESSAGE TERMINATOR' is send by putting a newline as the last character in data and setting the end flag in reason. </para>
    /// </remarks>
    /// <returns>   An array of byte. </returns>
    public byte[] GetData()
    {
        return this._data;
    }

    /// <summary>   Sets a data. </summary>
    /// <remarks>
    /// In the case of <see cref="Vxi11Message.DeviceWriteProcedure"/> and <see cref="Vxi11Message.DeviceReadProcedure"/> 
    /// the XDR opaque type is used by the network instrument protocol not because the data being represented is truly opaque, 
    /// but to avoid the overhead associated with character data (8 bits being promoted to 32 bits). Since the data parameters for
    /// <see cref="Vxi11Message.DeviceWriteProcedure"/> and <see cref="Vxi11Message.DeviceReadProcedure"/> are arrays, 
    /// a structure is passed which contains a pointer to the data, data.data_val, and the number of elements, data.data_len. <para>
    /// 
    /// A 'RESPONSE MESSAGE TERMINATOR' is send by putting a newline as the last character in data and setting the end flag in reason. </para>
    /// </remarks>
    /// <param name="data"> Gets or sets the data. </param>
    public void SetData( byte[] data )
    {
        this._data = data ?? Array.Empty<byte>();
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        this.ErrorCode.Encode( encoder );
        encoder.EncodeInt( ( int ) this.Reason );
        encoder.EncodeDynamicOpaque( this._data );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ErrorCode = new DeviceErrorCode( decoder );
        this.Reason = ( DeviceReadReasons ) decoder.DecodeInt();
        this._data = decoder.DecodeDynamicOpaque();
    }

}

/// <summary>   Values that represent device read reasons. </summary>
/// <remarks>   
/// Upon successfully completing a <see cref="Vxi11Message.DeviceReadProcedure"/> RPC, a network instrument server:
/// <list type="number"><item>
/// Transfer bytes into the data parameter until one of the following termination conditions are met:  
/// <list type="number"><item>
/// a. If an END indicator is read. The END bit in reason is set. </item><item>
/// b. If <see cref="DeviceReadParms.RequestSize"/> bytes are transferred. The <see cref="DeviceReadReasons.RequestCountIndicator"/>   
/// (<c>REQCNT</c>) bit in reason is set. This termination condition is be used if <see cref="DeviceReadParms.RequestSize"/> is zero. </item><item>
/// c. If <see cref="DeviceOperationFlags.TerminationCharacterSet"/> is set in <see cref="DeviceReadParms.Flags"/> and a   
/// character which matches <see cref="DeviceReadParms.TermChar"/> is transferred. 
/// The <see cref="DeviceReadReasons.TermCharIndicator"/> (<c>CHR</c>) bit in reason is set. </item><item>
/// d. If the buffer used to return the response is full. No bits in reason are set.</item></list></item><item>
/// Return with error set to 0, no error, to indicate successful completion. 
/// If more than one termination condition is valid, reason contains the bitwise inclusive OR of all the
/// reasons.</item></list>
/// </remarks>
[Flags]
public enum DeviceReadReasons
{
    Unknown = 0,

    /// <summary>   A binary constant representing the request count indicator flag.
    /// This bit is set if <see cref="DeviceReadParms.RequestSize"/> bytes are transferred </summary>
    RequestCountIndicator = 1,

    /// <summary>   A binary constant representing the termination Character indicator flag. 
    /// this bit is set if <see cref="DeviceOperationFlags.TerminationCharacterSet"/> is set in
    /// <see cref="DeviceReadParms.Flags"/> and a character which matches <see cref="DeviceReadParms.TermChar"/> is transferred
    ///  </summary>
    TermCharIndicator = 2,

    /// <summary>   A binary constant representing the end indicator flag.
    /// This bit is set if an END indicator is read. </summary>
    EndIndicator = 4,
}
