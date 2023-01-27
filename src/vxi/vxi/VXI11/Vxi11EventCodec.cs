using cc.isr.VXI11.EnumExtensions;

namespace cc.isr.VXI11;

/// <summary>   A VXI-11 Event codec. </summary>
/// <remarks>   2023-01-25. </remarks>
public class Vxi11EventCodec : IXdrCodec
{

    /// <summary>   Default constructor. </summary>
    public Vxi11EventCodec() : this( new byte[40] )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <param name="handle">   The handle. </param>
    public Vxi11EventCodec( byte[] handle )
    {
        this._handle = handle;
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="eventType">    The event type. </param>
    public Vxi11EventCodec( Vxi11EventType eventType ) : this()
    {
        this.EventType = eventType;
    }

    /// <summary>   Decodes an instance of a <see cref="Vxi11EventCodec"/>. </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <param name="handle">   The handle. </param>
    /// <returns>   The <see cref="Vxi11EventCodec"/>. </returns>
    public static Vxi11EventCodec DecodeInstance( byte[] handle )
    {
        Vxi11EventCodec codec = new Vxi11EventCodec( handle );
        XdrBufferDecodingStream decoder = new XdrBufferDecodingStream( handle );
        codec.Decode( decoder );
        return codec;
    }

    /// <summary>   Encodes an instance of a <see cref="Vxi11EventCodec"/>. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="eventType">    The event type. </param>
    /// <returns>   A Vxi11EventCodec. </returns>
    public static Vxi11EventCodec EncodeInstance( Vxi11EventType eventType )
    {
        Vxi11EventCodec codec = new Vxi11EventCodec( eventType );
        XdrBufferEncodingStream encoder = new( codec._handle );
        encoder.EncodeInt( ( int ) eventType );
        codec.SetHandle( encoder.GetEncodedData() );
        return codec;
    }

    /// <summary>   Gets or sets the identifier that uniquely identifies the 
    /// client sending the event. This value is encoded into the <see cref="_handle"/></summary>
    public int ClientId { get; set; }

    /// <summary>   Gets or sets the <see cref="Vxi11EventType"/>. </summary>
    /// <value> The event type. </value>
    public Vxi11EventType EventType { get; set; }

    /// <summary>   Gets or sets the handle. Host specific data for handling the service request. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="Vxi11EventCodec.GetHandle()"/>
    /// when a service request occurs. <para>
    /// The network instrument client should send in the handle parameter a unique link identifier. This will
    /// allow the network instrument client to identify the link associated with subsequent 
    /// <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/> RPCs. </para>
    /// </remarks>
    /// <value> The handle. </value>
    private byte[] _handle;

    /// <summary>   Gets the handle. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="Vxi11EventCodec.GetHandle()"/>
    /// when a service request occurs. </remarks>
    /// <returns>   An array of byte. </returns>
    public byte[] GetHandle()
    {
        return this._handle;
    }

    /// <summary>   Sets a handle. </summary>
    /// <remarks> The handle is passed back to the client with <see cref="Vxi11EventCodec.GetHandle()"/>
    /// when a service request occurs. </remarks>
    /// <param name="handle">   The handle. </param>
    public void SetHandle( byte[] handle )
    {
        this._handle = handle ?? new byte[40];
    }

    /// <summary>   Sets a handle. </summary>
    /// <remarks>
    /// The handle is passed back to the client with <see cref="Vxi11EventCodec.GetHandle()"/>
    /// when a service request occurs.
    /// </remarks>
    /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
    public void SetHandle( XdrBufferEncodingStream encoder )
    {
        Array.Copy( encoder.GetEncodedData(), this._handle, this._handle.Length );
    }

    /// <summary>   Sets the handle array. </summary>
    /// <remarks>
    /// The handle is passed back to the client with <see cref="Vxi11EventCodec.GetHandle()"/>
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
        this.ClientId.Encode( encoder );
        ( ( int) this.EventType).Encode( encoder );
    }

    /// <summary>
    /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
    /// </summary>
    /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
    public void Decode( XdrDecodingStreamBase decoder )
    {
        this.ClientId = decoder.DecodeInt();
        this.EventType = decoder.DecodeInt().ToVxi11EventType();
    }

}
