namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="DeviceException"/> indicates VXI-11 conditions that a reasonable
/// application might want to catch.
/// </summary>
/// <remarks>
/// The class <see cref="DeviceException"/> also defines a set of VXI-11 error codes as defined
/// by VXI-11 Specifications.
/// </remarks>
[Serializable]
public class DeviceException : Exception
{

    /// <summary>
    /// Constructs an <see cref="DeviceException"/> with the specified detail reason and message.
    /// </summary>
    /// <param name="reason">  The detail reason. </param>
    /// <param name="message"> The detail message. </param>
    public DeviceException( DeviceErrorCode reason, string message ) : base( message )
    {
        this.Reason = reason;
    }

    /// <summary>
    /// Constructs an <see cref="DeviceException"/> with the specified detail reason.
    /// </summary>
    /// <remarks>   The detail message is derived automatically from the <paramref name="reason"/>. </remarks>
    /// <param name="reason">   The reason. This can be one of the constants -- oops, that should be
    ///                         "public final static integers" -- defined in this interface. </param>
    public DeviceException( DeviceErrorCode reason ) : base( Support.GetDescription( reason ) )
    {
        this.Reason = reason;
    }

    /// <summary>
    /// Constructs an <see cref="DeviceException"/> with the specified detail reason and inner exception.
    /// </summary>
    /// <param name="reason">           The detail reason. </param>
    /// <param name="innerException">   The inner exception. </param>
    public DeviceException( DeviceErrorCode reason, Exception innerException ) : base( Support.GetDescription( reason ), innerException )
    {
        this.Reason = reason;
    }

    /// <summary>
    /// Constructs an <see cref="DeviceException"/> with the specified detail reason, suffix message and inner exception.
    /// </summary>
    /// <param name="suffixMessage">    Message describing the suffix. </param>
    /// <param name="reason">           The detail reason. </param>
    /// <param name="innerException">   The inner exception. </param>
    public DeviceException( string suffixMessage, DeviceErrorCode reason,
                            Exception innerException ) : base( Support.GetDescription( reason ) + suffixMessage, innerException )
    {
        this.Reason = reason;
    }

    /// <summary>
    /// Constructs an <see cref="DeviceException"/> with the specified detail reason and a suffix message.
    /// </summary>
    /// <param name="suffixMessage">    Message describing the suffix. </param>
    /// <param name="reason">           The detail reason. </param>
    public DeviceException( string suffixMessage, DeviceErrorCode reason ) : base( Support.GetDescription( reason ) + suffixMessage )
    {
        this.Reason = reason;
    }

    /// <summary>
    /// Specific (reason) for this <see cref="DeviceException"/> <see cref="DeviceException"/>, like
    /// the ONC/RPC error code, as defined by the constants of this class.
    /// </summary>
    /// <value>
    /// The error reason of this <see cref="DeviceException"/> object.
    /// </value>
    public DeviceErrorCode Reason { get; private set; }

}
