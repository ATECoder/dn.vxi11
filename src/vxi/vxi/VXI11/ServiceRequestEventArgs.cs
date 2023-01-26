namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="ServiceRequestEventArgs"/> defines the event arguments for the 
/// an event fired by <see cref="DeviceIntrServerStubBase">Interrupt servers</see> whenever replies to a
/// <see cref="DeviceIntrClient.DeviceIntrSrq(cc.isr.VXI11.Codecs.DeviceSrqParms)"/>
/// is received.  
/// </summary>
public class ServiceRequestEventArgs : EventArgs
{

    /// <summary>
    /// Creates a new <see cref="ServiceRequestEventArgs"/> object and initializes its state.
    /// </summary>
    /// <param name="handle">   The handle. </param>
    public ServiceRequestEventArgs( byte[] handle ) : this( ServiceRequestCodec.DecodeInstance( handle ) )
    {
    }

    /// <summary>
    /// Creates a new <see cref="ServiceRequestEventArgs"/> object and initializes its state.
    /// </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <param name="serviceRequestCodec">  The service request codec. </param>
    public ServiceRequestEventArgs( ServiceRequestCodec serviceRequestCodec )
    {
        this.ServiceRequestCode = serviceRequestCodec;
    }

    public ServiceRequestCodec ServiceRequestCode { get; private set; }
}
