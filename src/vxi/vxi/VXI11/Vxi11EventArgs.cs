namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="Vxi11EventArgs"/> defines the event arguments for the 
/// an event fired by the <see cref="InterruptChannelServer">Interrupt server</see> whenever replies to a
/// <see cref="InterruptChannelClient.DeviceIntrSrq(cc.isr.VXI11.Codecs.DeviceSrqParams)"/> message
/// is received.
/// </summary>
public class Vxi11EventArgs : EventArgs
{

    /// <summary>
    /// Creates a new <see cref="Vxi11EventArgs"/> object and initializes its state.
    /// </summary>
    /// <param name="handle">   The handle. </param>
    public Vxi11EventArgs( byte[] handle ) : this( Vxi11EventCodec.DecodeInstance( handle ) )
    { }

    /// <summary>
    /// Creates a new <see cref="Vxi11EventArgs"/> object and initializes its state.
    /// </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <param name="serviceRequestCodec">  The service request codec. </param>
    public Vxi11EventArgs( Vxi11EventCodec serviceRequestCodec )
    {
        this.ServiceRequestCodec = serviceRequestCodec ?? new Vxi11EventCodec();
    }

    /// <summary>   Gets or sets the service request codec. </summary>
    /// <value> The service request codec. </value>
    public Vxi11EventCodec ServiceRequestCodec { get; private set; }
}
