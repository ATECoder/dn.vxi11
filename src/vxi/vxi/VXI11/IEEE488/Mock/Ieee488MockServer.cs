using System.Net;
using System.Reflection;

using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Visa;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.IEEE488.Mock;

/// <summary>   An IEEE488 Mock server. </summary>
/// <remarks>   
/// Closing a client connected to the Mock local server no longer throws an exception when destroying the link.
/// </remarks>
public partial class Ieee488MockServer : DeviceCoreServerStubBase
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public Ieee488MockServer() : this( null, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port number where the server will wait for incoming calls. </param>
    public Ieee488MockServer( int port ) : this( null, port )
    {
    }

    public Ieee488MockServer( IPAddress? bindAddr, int port ) : this( new Ieee488Device(), bindAddr, port )
    {
    }

    public Ieee488MockServer( Ieee488Device device ) : this( device, null, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Ieee488MockServer( Ieee488Device device, IPAddress? bindAddr, int port ) : base( bindAddr ?? IPAddress.Any, port )
    {
        this._device = device;
        this._interfaceDeviceString = string.Empty;
        this._readMessage = string.Empty;
        this._writeMessage = string.Empty;
        this.AbortPortNumber = AbortChannelServerBase.AbortPortDefault;
        this.MaxReceiveLength = Ieee488Client.MaxReceiveLengthDefault;
    }

    #endregion

    #region " abort server "

    private int _abortPortNumber;
    /// <summary>   Gets or sets the abort port number. </summary>
    /// <value> The abortPort number. </value>
    public int AbortPortNumber
    {
        get => this._abortPortNumber;
        set => _ = this.SetProperty( ref this._abortPortNumber, value );
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
    /// Gets or sets the encoding to use when serializing strings. If <see langcref="null" />, the
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

    #region " mock device "

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

    #endregion

    #region " LXI-11 ONC/RPC Calls "

    private int _linkId = 0;

    /// <summary>   Create a device connection; Opens a link to a device. </summary>
    /// <param name="linkInfo"> Information describing the link. </param>
    /// <returns>   The new link to a device. </returns>
    public override CreateLinkResp CreateLink( CreateLinkParms linkInfo )
    {
        CreateLinkResp reply = new() {
            DeviceLink = new DeviceLink() { LinkId = this._linkId++ },
            MaxReceiveSize = this.MaxReceiveLength,
            AbortPort = ( short ) this.AbortPortNumber
        };

        Logger.Writer.LogVerbose( $"creating link to {linkInfo.Device}" );

        this.InterfaceDevice = new DeviceAddress( linkInfo.Device );
        reply.ErrorCode = this.InterfaceDevice.IsValid()
            ? new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.NoError }
            : new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.InvalidLinkIdentifier };
        return reply;
    }

    /// <summary>   Destroy a connection. </summary>
    /// <param name="deviceLink">   The device link. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DestroyLink( DeviceLink deviceLink )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Create an interrupt channel. </summary>
    /// <param name="deviceRemoteFunction"> The device remote function. </param>
    /// <returns>   The new interrupt channel 1. </returns>
    public override DeviceError CreateIntrChan( DeviceRemoteFunc deviceRemoteFunction )
    {
        DeviceError result = new() { ErrorCode = new DeviceErrorCode( ( int ) OncRpcExceptionReason.OncRpcSuccess ) };
        return result;
    }

    /// <summary>   Destroy an interrupt channel. </summary>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DestroyIntrChan()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Device clear. </summary>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceClear( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device executes a command. </summary>
    /// <param name="deviceCommandParameters">  device command parameters. </param>
    /// <returns>   A Device_DocmdResp. </returns>
    public override DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms deviceCommandParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device enables or does not enable the Send Request service. </summary>
    /// <param name="deviceEnableSrqParameters">    Device enable SRQ parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceEnableSrq( DeviceEnableSrqParms deviceEnableSrqParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device local control. </summary>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceLocal( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device remote control. </summary>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceRemote( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Returns the device status byte. </summary>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_ReadStbResp. </returns>
    public override DeviceReadStbResp DeviceReadStb( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Performs a trigger. </summary>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceTrigger( DeviceGenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Lock the device. </summary>
    /// <param name="deviceLockParameters"> Device lock parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceLock( DeviceLockParms deviceLockParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Unlock the device. </summary>
    /// <param name="deviceLink">   The device link parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceUnlock( DeviceLink deviceLink )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Read a message. </summary>
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
            readRes.SetData( this._readBuffer );
            readRes.ErrorCode = new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.IOError }; // timeout
            readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
            return readRes;
        }

        if ( this.CurrentOperationType == Ieee488OperationType.Read )
        {
            readRes.SetData( this._readBuffer );
            readRes.ErrorCode = new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.NoError };
            readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
        }
        this.CurrentOperationType = Ieee488OperationType.None; //Reset the action type
        return readRes;
    }

    /// <summary>   Process the device write procedure. </summary>
    /// <param name="deviceWriteParameters"> Device write parameters. </param>
    /// <returns>   A Device_WriteResp. </returns>
    public override DeviceWriteResp DeviceWrite( DeviceWriteParms deviceWriteParameters )
    {
        // get the write command.
        string cmd = this.CharacterEncoding.GetString( deviceWriteParameters.GetData() );
        Logger.Writer.LogVerbose( $"link ID: {deviceWriteParameters.Link.LinkId} -> Received：{cmd}" );
        DeviceWriteResp result = new() { ErrorCode = new DeviceErrorCode( ( int ) OncRpcExceptionReason.OncRpcSuccess ) };

        // holds one or more SCPI commands each with its arguments
        string[] scpiCommands = cmd.Split( new char[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries );

        if ( scpiCommands.Length == 0 )
        {
            // The instruction is incorrect or undefined
            result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.SyntaxError );
            return result;
        }

        // process all the SCPI commands
        for ( int n = 0; n < scpiCommands.Length; n++ )
        {
            string spciCommand = scpiCommands[n]; // select a complete SCPI command with optional arguments
            Logger.Writer.LogVerbose( $"Process the instruction： {spciCommand}" );
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
                            Logger.Writer.LogMemberWarning( $"The attribute of method {method} is marked incorrectly as {scpiAtt.OperationType}。" );
                            break;
                        case Ieee488OperationType.Write:
                            this.WriteMessage = scpiCommands[n];
                            // invoke the corresponding method
                            res = method.Invoke( this._device, scpiArgs );
                            result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.NoError );
                            break;
                        case Ieee488OperationType.Read://Query instructions
                            this.WriteMessage = scpiCommands[n];
                            res = method.Invoke( this._device, scpiArgs );
                            if ( res is not null )
                            {
                                this.ReadMessage = res.ToString();
                                this._readBuffer = this.CharacterEncoding.GetBytes( res.ToString()! );
                                Logger.Writer.LogVerbose( $"Query results： {res}。" );
                            }
                            else
                            {
                                this.ReadMessage = "null";
                                Logger.Writer.LogVerbose( "Query results：NULL。" );
                                result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.NoError );
                            }
                            break;
                    }
                }
                catch ( Exception ex )
                {
                    Logger.Writer.LogMemberError( $"An error occurred when the method was called：{method}", ex );
                    // Parameter error
                    result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.ParameterError );
                }
            }
            else
            {
                Logger.Writer.LogMemberWarning( $"No method found： {spciCommand}" );
                result.ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.SyntaxError ); // The instruction is incorrect or undefined
                this.CurrentOperationType = Ieee488OperationType.None;
            }
            _ = this._asyncLocker.Set();//Reset block
        }


        return result;
    }

    #endregion

}
