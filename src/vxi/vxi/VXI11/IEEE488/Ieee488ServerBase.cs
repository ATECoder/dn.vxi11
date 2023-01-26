using System.Net;
using System.Reflection;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Visa;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.IEEE488;

/// <summary>   An IEEE488 server base. </summary>
/// <remarks>   
/// Closing a client connected to the Mock local server no longer throws an exception when destroying the link.
/// </remarks>
public abstract partial class Ieee488ServerBase : DeviceCoreServerStubBase
{
    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public Ieee488ServerBase() : this( 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port number where the server will wait for incoming calls. </param>
    public Ieee488ServerBase( int port ) : this( IPAddress.Any, port )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Ieee488ServerBase( IPAddress bindAddr, int port ) : base( bindAddr, port ) 
    {
        this._interfaceDeviceString = string.Empty;
        this._readMessage = string.Empty;
        this._writeMessage = string.Empty;
        this.MaxReceiveLength = Ieee488Client.MaxReceiveLengthDefault;
    }

    #endregion

    #region " I/O messages "

    private string _writeMessage;
    /// <summary>   Gets or sets a message that was sent to the device. </summary>
    /// <value> The message that was sent to the device. </value>
    public string WriteMessage
    {
        get => this._writeMessage;
        set => _ = this.SetProperty( ref this._writeMessage, value );
    }

    private string _readMessage;
    /// <summary>   Gets or sets a message that was received from the device. </summary>
    /// <value> A message that was received from the device. </value>
    public string ReadMessage
    {
        get => this._readMessage;
        set => _ = this.SetProperty( ref this._readMessage, value );
    }

    #endregion

    #region " IEEE488 properties "

    private string _interfaceDeviceString;
    /// <summary>   Gets or sets the interface device string. </summary>
    /// <value> The interface device string. </value>
    public string InterfaceDeviceString
    {
        get => this._interfaceDeviceString;
        private set => _ = this.SetProperty( ref this._interfaceDeviceString, value );
    }

    private DeviceAddress _interfaceDevice;
    /// <summary>   Gets or sets the interface device. </summary>
    /// <value> The interface device. </value>
    public DeviceAddress InterfaceDevice
    {
        get => this._interfaceDevice;
        set {
            _ = this.SetProperty( ref this._interfaceDevice, value );
            this.InterfaceDeviceString = this._interfaceDevice.InterfaceDeviceAddress;
        }
    }

    private int _maxReceiveLength;
    public int MaxReceiveLength
    {
        get => this._maxReceiveLength;
        set => _ = this.SetProperty( ref this._maxReceiveLength, value );
    }

    #endregion

}
