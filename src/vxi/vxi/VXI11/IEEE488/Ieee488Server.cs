using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.VXI11.Codecs;

#nullable enable

namespace cc.isr.VXI11.IEEE488;

/// <summary>   An IEEE488 server. </summary>
/// <remarks>   
/// Closing a client connected to the Mock local server no longer throws an exception when destroying the link.
/// </remarks>
public partial class Ieee488Server : DeviceCoreServerStubBase
{

    #region " construction and cleanup "

    /// <summary>
    /// current device
    /// </summary>
    private readonly IIeee488Device? _device = null;

    /// <summary>
    /// Thread synchronization locks
    /// </summary>
    private readonly ManualResetEvent _asyncLocker = new( false );

    /// <summary>
    /// Read cache buffer
    /// </summary>
    private byte[] _readBuffer = Array.Empty<byte>();

    /// <summary>   Default constructor. </summary>
    /// <remarks>   2022-12-15. </remarks>
    public Ieee488Server() : this( null, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="port"> The port number where the server will wait for incoming calls. </param>
    public Ieee488Server( int port ) : this( null, port )
    {
    }

    public Ieee488Server( IPAddress? bindAddr, int port ) : this( new Ieee488Device(), bindAddr, port )
    {
    }

    public Ieee488Server( Ieee488Device device ) : this( device, null, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Ieee488Server( Ieee488Device device, IPAddress? bindAddr, int port ) : base( bindAddr, port )
    {
        this._device = device;
        this._interfaceDeviceString = string.Empty;
        this._ipv4Address = bindAddr is null ? string.Empty : bindAddr.ToString();
        this._readMessage = string.Empty;
        this._writeMessage = string.Empty;
    }

    #endregion

    #region " Server Properties "

    private int _portNumber;
    /// <summary>   Gets or sets the port number. </summary>
    /// <value> The port number. </value>
    public int PortNumber
    {
        get => this._portNumber;
        set => _ = this.SetProperty( ref this._portNumber, value );
    }

    private string _ipv4Address;
    /// <summary>   Gets or sets the IPv4 address. </summary>
    /// <value> The IPv4 address. </value>
    public string IPv4Address
    {
        get => this._ipv4Address;
        set => _ = this.SetProperty( ref this._ipv4Address, value );
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

    /// <summary>
    /// Gets or sets the encoding to use when serializing strings. If <see langword="null" />, the
    /// system's default encoding is to be used.
    /// </summary>
    /// <value> The character encoding. </value>
    public override Encoding CharacterEncoding
    {
        get => base.CharacterEncoding;
        set => _ = this.SetProperty( base.CharacterEncoding!, value, () => base.CharacterEncoding = value );
    }

    private int _waitOnOutTime = 1000;
    /// <summary>   Timeout wait time ms. </summary>
    /// <value> The wait on out time. </value>
    public int WaitOnOutTime
    {
        get => this._waitOnOutTime;
        set => _ = this.SetProperty( ref this._waitOnOutTime, value );
    }

    /// <summary>   The current operation instruction type. </summary>
    /// <value> The type of the current operation. </value>
    public Ieee488OperationType CurrentOperationType { get; private set; } = Ieee488OperationType.None;

    private string _interfaceDeviceString;
    /// <summary>   Gets or sets the interface device string. </summary>
    /// <value> The interface device string. </value>
    public string InterfaceDeviceString
    {
        get => this._interfaceDeviceString;
        private set => _ = this.SetProperty( ref this._interfaceDeviceString, value );
    }

    private Ieee488InterfaceDevice _interfaceDevice;
    /// <summary>   Gets or sets the interface device. </summary>
    /// <value> The interface device. </value>
    public Ieee488InterfaceDevice InterfaceDevice
    {
        get => this._interfaceDevice;
        set {
            _ = this.SetProperty( ref this._interfaceDevice, value );
            this.InterfaceDeviceString = this._interfaceDevice.InterfaceDeviceString;
        }
    }

    #endregion

    #region " LXI-11 ONC/RPC Calls "

    private int _linkId = 0;

    /// <summary>   Create a device connection; Opens a link to a device. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="linkInfo"> Information describing the link. </param>
    /// <returns>   The new link to a device. </returns>
    public override CreateLinkResp CreateLink( CreateLinkParms linkInfo )
    {
        CreateLinkResp result = new();
        this._linkId++;
        result.DeviceLinkId = new DeviceLink() { Value = _linkId };

        Logger.Writer.ConsoleWriteMessage( $"creating link to {linkInfo.Device}" );

        this.InterfaceDevice = new Ieee488InterfaceDevice( linkInfo.Device );
        result.ErrorCode = this.InterfaceDevice.IsValid()
            ? new DeviceErrorCode() { Value = DeviceErrorCodeValue.NoError }
            : new DeviceErrorCode() { Value = DeviceErrorCodeValue.InvalidLinkIdentifier };
        return result;
    }

    /// <summary>   Destroy a connection. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceLink">   The device link. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DestroyLink( DeviceLink deviceLink )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Create an interrupt channel. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceRemoteFunction"> The device remote function. </param>
    /// <returns>   The new interrupt channel 1. </returns>
    public override DeviceError CreateIntrChan( DeviceRemoteFunc deviceRemoteFunction )
    {
        DeviceError result = new() { Error = new DeviceErrorCode( ( int ) OncRpcExceptionReason.OncRpcSuccess ) };
        return result;
    }

    /// <summary>   Destroy an interrupt channel. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DestroyIntrChan()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Device clear. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceClear( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceCommandParameters">  device command parameters. </param>
    /// <returns>   A Device_DocmdResp. </returns>
    public override DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms deviceCommandParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device enables or does not enable the Send Request service. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceEnableSrqParameters">    Device enable SRQ parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceEnableSrq( DeviceEnableSrqParms deviceEnableSrqParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device local control. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceLocal( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device remote control. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceRemote( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Returns the device status byte. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_ReadStbResp. </returns>
    public override DeviceReadStbResp DeviceReadStb( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Performs a trigger. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceTrigger( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Lock the device. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceLockParameters"> Device lock parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceLock( DeviceLockParms deviceLockParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Unlock the device. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceLink">   The device link parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceUnlock( DeviceLink deviceLink )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Read a message. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceReadParameters"> Device read parameters. </param>
    /// <returns>   A Device_ReadResp. </returns>
    public override DeviceReadResp DeviceRead( DeviceReadParms deviceReadParameters )
    {
        DeviceReadResp readRes = new();
        if ( this.CurrentOperationType == Ieee488OperationType.None || this.CurrentOperationType == Ieee488OperationType.Write )
        {
            this._readBuffer = Array.Empty<byte>();
            _ = this._asyncLocker.Reset();
        }
        if ( !this._asyncLocker.WaitOne( this.WaitOnOutTime ) )
        {
            readRes.Data = this._readBuffer;
            readRes.Error = new DeviceErrorCode() { Value = DeviceErrorCodeValue.IOError }; // timeout
            readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
            return readRes;
        }

        if ( this.CurrentOperationType == Ieee488OperationType.Read )
        {
            readRes.Data = this._readBuffer;
            readRes.Error = new DeviceErrorCode() { Value = DeviceErrorCodeValue.NoError };
            readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
        }
        this.CurrentOperationType = Ieee488OperationType.None; //Reset the action type
        return readRes;
    }

    /// <summary>   Process the device write procedure. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceWriteParameters"> Device write parameters. </param>
    /// <returns>   A Device_WriteResp. </returns>
    public override DeviceWriteResp DeviceWrite( DeviceWriteParms deviceWriteParameters )
    {
        // get the write command.
        string cmd = this.CharacterEncoding.GetString( deviceWriteParameters.Data );
        Logger.Writer.ConsoleWriteMessage( $"link ID: {deviceWriteParameters.DeviceLinkId.Value} -> Received：{cmd}" );
        DeviceWriteResp result = new() { Error = new DeviceErrorCode( ( int ) OncRpcExceptionReason.OncRpcSuccess ) };

        // holds one or more SCPI commands each with its arguments
        string[] scpiCommands = cmd.Split( new char[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries );

        if ( scpiCommands.Length == 0 )
        {
            // The instruction is incorrect or undefined
            result.Error = new DeviceErrorCode( DeviceErrorCodeValue.SyntaxError );
            return result;
        }

        // process all the SCPI commands
        for ( int n = 0; n < scpiCommands.Length; n++ )
        {
            string spciCommand = scpiCommands[n]; // select a complete SCPI command with optional arguments
            Logger.Writer.ConsoleWriteMessage( $"Process the instruction： {spciCommand}" );
            string[] scpiArgs = Array.Empty<string>(); // Holds the SCPI command arguments

            // split the command to the core command and its arguments:
            string[] scpiCmdElements = scpiCommands[n].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            spciCommand = scpiCmdElements[0].Trim();

            _ = this._asyncLocker.Reset(); // Block threads
            this._readBuffer = Array.Empty<byte>();

            // check if we have a query message (read) or a write message:
            this.CurrentOperationType = spciCommand[^1] == '?' ? Ieee488OperationType.Read : Ieee488OperationType.Write;

            // get the command arguments
            if ( scpiCmdElements.Length >= 2 )
                scpiArgs = scpiCmdElements[1].Split( new char[] { '，' }, StringSplitOptions.RemoveEmptyEntries );

            // find the mock server method that corresponds to the SCPI command.
            MethodInfo? method = this._device!.GetType().GetMethods().ToList().Find( p => {
                var att = p.GetCustomAttribute( typeof( Ieee488Attribute ) );
                if ( att == null || att is not Ieee488Attribute ) return false;
                Ieee488Attribute scpiAtt = ( Ieee488Attribute ) att;

                // return success if the command matches the method attribute
                return String.Equals( scpiAtt.Content, spciCommand, StringComparison.OrdinalIgnoreCase );
            } );

            if ( method is not null )
            {
                Ieee488Attribute scpiAtt = ( Ieee488Attribute ) method.GetCustomAttribute( typeof( Ieee488Attribute ) )!;
                try
                {
                    object? res = null;
                    switch ( scpiAtt.OperationType )
                    {
                        case Ieee488OperationType.None:
                            Logger.Writer.ConsoleWriteMessage( $"The attribute of method {method} is marked incorrectly as {scpiAtt.OperationType}。", ConsoleColor.Red );
                            break;
                        case Ieee488OperationType.Write:
                            this.WriteMessage = scpiCommands[n];
                            // invoke the corresponding method
                            res = method.Invoke( this._device, scpiArgs );
                            result.Error = new DeviceErrorCode( DeviceErrorCodeValue.NoError );
                            break;
                        case Ieee488OperationType.Read://Query instructions
                            this.WriteMessage = scpiCommands[n];
                            res = method.Invoke( this._device, scpiArgs );
                            if ( res is not null )
                            {
                                this.ReadMessage = res.ToString();
                                this._readBuffer = this.CharacterEncoding.GetBytes( res.ToString()! );
                                Logger.Writer.ConsoleWriteMessage( $"Query results： {res}。" );
                            }
                            else
                            {
                                this.ReadMessage = "null";
                                Logger.Writer.ConsoleWriteMessage( "Query results：NULL。" );
                                result.Error = new DeviceErrorCode( DeviceErrorCodeValue.NoError );
                            }
                            break;
                    }
                }
                catch ( Exception ex )
                {
                    Logger.Writer.ConsoleWriteException( $"An error occurred when the method was called：{method}", ex );
                    // Parameter error
                    result.Error = new DeviceErrorCode( DeviceErrorCodeValue.ParameterError );
                }
            }
            else
            {
                Logger.Writer.ConsoleWriteMessage( $"No method found： {spciCommand}", ConsoleColor.DarkYellow );
                result.Error = new DeviceErrorCode( DeviceErrorCodeValue.SyntaxError ); // The instruction is incorrect or undefined
                this.CurrentOperationType = Ieee488OperationType.None;
            }
            _ = this._asyncLocker.Set();//Reset block
        }


        return result;
    }

    #endregion

    #region " Port mapper "

    private static void EstablishPortmapService()
    {

        // Ignore all problems during unregistration.

        OncRpcEmbeddedPortmapService epm;

        Logger.Writer.ConsoleWriteMessage( "Checking for portmap service: " );
        bool externalPortmap = OncRpcEmbeddedPortmapService.IsPortmapRunning();
        if ( externalPortmap )
            Logger.Writer.ConsoleWriteMessage( "A portmap service is already running." );
        else
            Logger.Writer.ConsoleWriteMessage( "No portmap service available." );

        // Create embedded portmap service and check whether is has sprung into action.

        Logger.Writer.ConsoleWriteMessage( "Creating embedded portmap instance: " );
        try
        {
            epm = new OncRpcEmbeddedPortmapService();

            if ( !epm.EmbeddedPortmapInUse() )
                Logger.Writer.ConsoleWriteMessage( "embedded service not used: " );
            else
                Logger.Writer.ConsoleWriteMessage( "embedded service started: " );
            if ( epm.EmbeddedPortmapInUse() == externalPortmap )
            {
                Logger.Writer.ConsoleWriteMessage( "ERROR: no service available or both." );
                return;
            }
        }
        catch ( IOException e )
        {
            Logger.Writer.ConsoleWriteException( "ERROR: failed:", e );
        }
        catch ( DeviceException e )
        {
            Logger.Writer.ConsoleWriteException( "ERROR: failed:", e );
        }
        externalPortmap = OncRpcEmbeddedPortmapService.IsPortmapRunning();
        Logger.Writer.ConsoleWriteMessage( $"Port map service is {(externalPortmap ? "running" : "idle")}." );
    }

    #endregion

    #region " START / STOP "

    private bool _listening;
    /// <summary>   Gets or sets a value indicating whether the listening. </summary>
    /// <value> True if listening, false if not. </value>
    public bool Listening
    {
        get => this._listening;
        set => _ = this.SetProperty( ref this._listening, value );
    }

    /// <summary>
    /// All inclusive convenience method: register server transports with port mapper, then Runs the
    /// call dispatcher until the server is signaled to shut down, and finally deregister the
    /// transports.
    /// </summary>
    /// <remarks>
    /// All inclusive convenience method: register server transports with port mapper, then Runs the
    /// call dispatcher until the server is signaled to shut down, and finally deregister the
    /// transports.
    /// </remarks>
    public override void Run()
    {
        Ieee488Server.EstablishPortmapService();
        this.Listening = true;
        base.Run();
    }

    /// <summary>
    /// Notify the RPC server to stop processing of remote procedure call requests as soon as
    /// possible.
    /// </summary>
    /// <remarks>
    /// Notify the RPC server to stop processing of remote procedure call requests as soon as
    /// possible. Note that each transport has its own thread, so processing will not stop before the
    /// transports have been closed by calling the <see cref="cc.isr.ONC.RPC.Server.OncRpcServerStubBase.Close()"/>
    /// method of the server.
    /// </remarks>
    public override void StopRpcProcessing()
    {
        this.Listening = false;
        base.StopRpcProcessing();
    }

    #endregion

}
