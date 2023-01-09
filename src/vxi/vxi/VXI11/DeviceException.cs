
using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="DeviceException"/> indicates VXI-11 conditions that a reasonable
/// application might want to catch.
/// </summary>
/// <remarks>
/// The class <see cref="DeviceException"/> also defines a set of VXI-11 error codes as
/// defined by VXI-11 Specifications.
/// </remarks>
[Serializable]
public class DeviceException : Exception
{

    /// <summary>
    /// Constructs an <see cref="DeviceException"/> with the specified detail reason and message.
    /// </summary>
    /// <param name="reason">  The detail reason. </param>
    /// <param name="message"> The detail message. </param>
    public DeviceException( DeviceErrorCodeValue reason, string message ) : base()
    {
        this.Reason = reason;
        this._message = message;
    }

    /// <summary>
    /// Constructs an <see cref="DeviceException"/> with the specified detail reason.
    /// </summary>
    /// <remarks>   The detail message is derived automatically from the <paramref name="reason"/>. </remarks>
    /// <param name="reason">   The reason. This can be one of the constants -- oops, that should be
    ///                         "public final static integers" -- defined in this interface. </param>
    public DeviceException( DeviceErrorCodeValue reason ) : base()
    {
        this.Reason = reason;
        this._message = Support.GetDescription( reason );
    }

    private readonly string _message;

    /// <summary>   Returns the error message string of this ONC/RPC object. </summary>
    /// <value>
    /// The error message string of this <see cref="DeviceException"/>
    /// object if it was created either with an error message string or an ONC/RPC error code.
    /// </value>
    public override string Message => this._message;

    /// <summary>
    /// Specific (reason) for this <see cref="DeviceException"/> <see cref="DeviceException"/>, like
    /// the ONC/RPC error code, as defined by the constants of this class.
    /// </summary>
    /// <value>
    /// The error reason of this <see cref="DeviceException"/> object.
    /// </value>
    public DeviceErrorCodeValue Reason { get; private set; }

}
