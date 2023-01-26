namespace cc.isr.VXI11;

/// <summary>   A service request codec. </summary>
/// <remarks>   2023-01-25. </remarks>
public class ServiceRequestCodec : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public ServiceRequestCodec() : this( new byte[40] )   
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <param name="handle">   The handle. </param>
    public ServiceRequestCodec( byte[] handle )
    {
        this._handle = handle;
    }

    /// <summary>   Constructor. </summary>
    /// <param name="decoder">  The XDR Decoding stream </param>
    public ServiceRequestCodec( XdrDecodingStreamBase decoder ) : this()
    {
        this.Decode( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="ServiceRequestCodec"/>. </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    /// <returns>   The <see cref="ServiceRequestCodec"/>. </returns>
    public static ServiceRequestCodec DecodeInstance( XdrDecodingStreamBase decoder )
    {
        return new ServiceRequestCodec( decoder );
    }

    /// <summary>   Decodes an instance of a <see cref="ServiceRequestCodec"/>. </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <param name="handle">   The handle. </param>
    /// <returns>   The <see cref="ServiceRequestCodec"/>. </returns>
    public static ServiceRequestCodec DecodeInstance( byte[] handle )
    {
        XdrBufferDecodingStream decoder  = new XdrBufferDecodingStream( handle );
        return new ServiceRequestCodec( decoder );
    }

    /// <summary>   Gets or sets the identifier of the event. </summary>
    /// <value> The identifier of the event. </value>
    public int EventId { get; set; }

    /// <summary>   Gets or sets the handle. Host specific data for handling the service request. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="ServiceRequestCodec.GetHandle()"/>
    /// when a service request occurs. <para>
    /// The network instrument client should send in the handle parameter a unique link identifier. This will
    /// allow the network instrument client to identify the link associated with subsequent 
    /// <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/> RPCs. </para>
    /// </remarks>
    /// <value> The handle. </value>
    private byte[] _handle;

    /// <summary>   Gets the handle. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="ServiceRequestCodec.GetHandle()"/>
    /// when a service request occurs. </remarks>
    /// <returns>   An array of byte. </returns>
    public byte[] GetHandle()
    {
        return this._handle;
    }

    /// <summary>   Sets a handle. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="ServiceRequestCodec.GetHandle()"/>
    /// when a service request occurs. </remarks>
    /// <param name="handle">   The handle. </param>
    public void SetHandle( byte[] handle )
    {
        this._handle = handle ?? new byte[40];
    }

    /// <summary>   Sets a handle. </summary>
    /// <remarks>
    /// The handle is passed back to the client with <see cref="ServiceRequestCodec.GetHandle()"/>
    /// when a service request occurs.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void SetHandle( XdrBufferEncodingStream encoder )
    {
        Array.Copy( encoder.GetEncodedData(), this._handle, this._handle.Length );
    }

    /// <summary>   Sets the handle array. </summary>
    /// <remarks>
    /// The handle is passed back to the client with <see cref="ServiceRequestCodec.GetHandle()"/>
    /// when a service request occurs.
    /// </remarks>
    /// <param name="decoder">  The XDR Decoding stream. </param>
    public void SetHandle( XdrBufferDecodingStream decoder )
    {
        Array.Copy( decoder.GetEncodedData(), this._handle, this._handle.Length );
    }

    /// <summary>
    /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void Encode( XdrEncodingStreamBase encoder )
    {
        encoder.EncodeInt( this.EventId );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.EventId = decoder.DecodeInt();
    }

}
