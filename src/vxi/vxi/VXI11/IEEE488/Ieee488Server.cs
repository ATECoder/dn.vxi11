using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

using cc.isr.ONC.RPC;
using cc.isr.ONC.RPC.Portmap;

using VXI11;

#nullable enable

namespace cc.isr.VXI11.IEEE488;

/// <summary>   An IEEE488 server. </summary>
/// <remarks>   2022-12-15. 
/// <list type="bullet">Mapped error codes:<item>
/// <see cref="OncRpcException.OncRpcSuccess"/> -- Visa32.VISA.VI_SUCCESS</item><item>
/// <see cref="OncRpcException.OncRpcSystemError"/> -- Visa32.VISA.VI_ERROR_SYSTEM_ERROR</item><item>
/// <see cref="OncRpcException.OncRpcProgramNotAvailable"/> -- Visa32.VISA.VI_ERROR_INV_EXPR</item><item>
/// </item>
/// </list>
/// TODO: Closing a client connected to the Mock local server throws an exception when destroying the link.
/// </remarks>
public partial class Ieee488Server : DeviceCoreServerStubBase
{

    #region " Construction and Cleanup "

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
        this._InterfaceDeviceString = string.Empty;
        this._IPv4Address = bindAddr is null ? string.Empty : bindAddr.ToString();
        this._ReadMessage = string.Empty;
        this._WriteMessage= string.Empty;   
    }

    #endregion

    #region " Server Properties "

    private int _PortNumber;
    /// <summary>   Gets or sets the port number. </summary>
    /// <value> The port number. </value>
    public int PortNumber
    {
        get => this._PortNumber;
        set => _ = this.SetProperty( ref this._PortNumber, value );
    }

    private string _IPv4Address;
    /// <summary>   Gets or sets the IPv4 address. </summary>
    /// <value> The IPv4 address. </value>
    public string IPv4Address
    {
        get => this._IPv4Address;
        set => _ = this.SetProperty( ref this._IPv4Address, value );
    }

    #endregion

    #region " I/O messages "

    private string _WriteMessage;
    /// <summary>   Gets or sets a message that was sent to the device. </summary>
    /// <value> The message that was sent to the device. </value>
    public string WriteMessage
    {
        get => this._WriteMessage;
        set => _ = this.SetProperty( ref this._WriteMessage, value );
    }

    private string _ReadMessage;
    /// <summary>   Gets or sets a message that was received from the device. </summary>
    /// <value> A message that was received from the device. </value>
    public string ReadMessage
    {
        get => this._ReadMessage;
        set => _ = this.SetProperty( ref this._ReadMessage, value );
    }

    #endregion

    #region " IEEE488 properties "

    private Encoding _encodingRule = Encoding.ASCII;
    /// <summary>   Encoding rules Default ASCII. </summary>
    /// <value> The encoding rule. </value>
    public Encoding EncodingRule
    {
        get => this._encodingRule;
        set => _ = this.SetProperty( ref this._encodingRule , value );
    }

    private int _WaitOnOutTime = 1000;
    /// <summary>   Timeout wait time ms. </summary>
    /// <value> The wait on out time. </value>
    public int WaitOnOutTime
    {
        get => this._WaitOnOutTime;
        set => _ = this.SetProperty( ref this._WaitOnOutTime, value );
    }

    /// <summary>   The current operation instruction type. </summary>
    /// <value> The type of the current operation. </value>
    public Ieee488OperationType CurrentOperationType { get; private set; } = Ieee488OperationType.None;

    private string _InterfaceDeviceString;
    /// <summary>   Gets or sets the interface device string. </summary>
    /// <value> The interface device string. </value>
    public string InterfaceDeviceString
    {
        get => this._InterfaceDeviceString;
        private set => _ = this.SetProperty( ref this._InterfaceDeviceString, value );
    }

    private Ieee488InterfaceDevice _InterfaceDevice;
    /// <summary>   Gets or sets the interface device. </summary>
    /// <value> The interface device. </value>
    public Ieee488InterfaceDevice InterfaceDevice
    {
        get => this._InterfaceDevice;
        set
        {
            _ = this.SetProperty( ref this._InterfaceDevice, value );
            this.InterfaceDeviceString = this._InterfaceDevice.InterfaceDeviceString;
        }
    }

    #endregion

    #region " LXI-11 ONC/RPC Calls "

    private int _lidID = 0;

    /// <summary>   Create a device connection; Opens a link to a device. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="linkInfo"> Information describing the link. </param>
    /// <returns>   The new link to a device. </returns>
    public override Create_LinkResp create_link_1( Create_LinkParms linkInfo )
    {
        Create_LinkResp result = new();
        this._lidID++;
        result.lid = new Device_Link() { value = _lidID };

        Logger.Writer.ConsoleWriteMessage( $"creating link to {linkInfo.device}" );

        this.InterfaceDevice = new Ieee488InterfaceDevice( linkInfo.device );
        result.error = this.InterfaceDevice.IsValid()
            ? new Device_ErrorCode() { value = OncRpcException.OncRpcSuccess }
            : new Device_ErrorCode() { value = OncRpcException.OncRpcSystemError };
        return result;
    }

    /// <summary>   Destroy a connection. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceLink">   The device link. </param>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error destroy_link_1( Device_Link deviceLink )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Create an interrupt channel. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceRemoteFunction"> The device remote function. </param>
    /// <returns>   The new interrupt channel 1. </returns>
    public override Device_Error create_intr_chan_1( Device_RemoteFunc deviceRemoteFunction )
    {
        Device_Error result = new() { error = new Device_ErrorCode( OncRpcException.OncRpcSuccess ) };
        return result;
    }

    /// <summary>   Destroy an interrupt channel. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error destroy_intr_chan_1()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Device clear. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error device_clear_1( Device_GenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceCommandParameters">  device command parameters. </param>
    /// <returns>   A Device_DocmdResp. </returns>
    public override Device_DocmdResp device_docmd_1( Device_DocmdParms deviceCommandParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device enables or does not enable the Send Request service. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceEnableSrqParameters">    Device enable SRQ parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error device_enable_srq_1( Device_EnableSrqParms deviceEnableSrqParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device local control. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error device_local_1( Device_GenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device remote control. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error device_remote_1( Device_GenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Returns the device status byte. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_ReadStbResp. </returns>
    public override Device_ReadStbResp device_readstb_1( Device_GenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Performs a trigger. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceGenericParameters">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error device_trigger_1( Device_GenericParms deviceGenericParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Lock the device. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceLockParameters"> Device lock parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error device_lock_1( Device_LockParms deviceLockParameters )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Unlock the device. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceLink">   The device link parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public override Device_Error device_unlock_1( Device_Link deviceLink )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Read a message. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceReadParameters"> Device read parameters. </param>
    /// <returns>   A Device_ReadResp. </returns>
    public override Device_ReadResp device_read_1( Device_ReadParms deviceReadParameters )
    {
        Device_ReadResp readRes = new();
        if ( this.CurrentOperationType == Ieee488OperationType.None || this.CurrentOperationType == Ieee488OperationType.Write )
        {
            this._readBuffer = Array.Empty<byte>();
            _ = this._asyncLocker.Reset();
        }
        if ( !this._asyncLocker.WaitOne( this.WaitOnOutTime ) )
        {
            readRes.data = this._readBuffer;
            readRes.error = new Device_ErrorCode() { value = OncRpcException.OncRpcProgramNotAvailable }; // timeout
            readRes.reason = 3;
            return readRes;
        }

        if ( this.CurrentOperationType == Ieee488OperationType.Read )
        {
            readRes.data = this._readBuffer;
            readRes.error = new Device_ErrorCode() { value = OncRpcException.OncRpcSuccess }; 
            readRes.reason = 3;
        }
        this.CurrentOperationType = Ieee488OperationType.None; //Reset the action type
        return readRes;
    }

    /// <summary>   Process the device write procedure. </summary>
    /// <remarks>   2022-12-15. </remarks>
    /// <param name="deviceWriteParameters"> Device write parameters. </param>
    /// <returns>   A Device_WriteResp. </returns>
    public override Device_WriteResp device_write_1( Device_WriteParms deviceWriteParameters )
    {
        // get the write command.
        string cmd = this.EncodingRule.GetString( deviceWriteParameters.data );
        Logger.Writer.ConsoleWriteMessage( $"link ID: {deviceWriteParameters.lid.value} -> Received：{cmd}" );
        Device_WriteResp result = new() { error = new Device_ErrorCode( OncRpcException.OncRpcSuccess ) };

        // holds one or more SCPI commands each with its arguments
        string[] scpiCommands = cmd.Split( new char[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries );

        if ( scpiCommands.Length == 0 )
        {
            // The instruction is incorrect or undefined
            result.error = new Device_ErrorCode( OncRpcException.OncRpcProgramNotAvailable ); 
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
            MethodInfo? method = this._device!.GetType().GetMethods().ToList().Find( p =>
            {
                var att = p.GetCustomAttribute( typeof( Ieee488Attribute ) );
                if ( att == null || att is not Ieee488Attribute ) return false;
                Ieee488Attribute scpiAtt = ( Ieee488Attribute ) att;

                // return success if the command matches the method attribute
                return String.Equals( scpiAtt.Content, spciCommand, StringComparison.OrdinalIgnoreCase );
            } );

            if ( method != null )
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
                            result.error = new Device_ErrorCode( OncRpcException.OncRpcSuccess ); 
                            break;
                        case Ieee488OperationType.Read://Query instructions
                            this.WriteMessage = scpiCommands[n];
                            res = method.Invoke( this._device, scpiArgs );
                            if ( res != null )
                            {
                                this.ReadMessage = res.ToString();
                                this._readBuffer = this.EncodingRule.GetBytes( res.ToString()! );
                                Logger.Writer.ConsoleWriteMessage( $"Query results： {res}。" );
                            }
                            else
                            {
                                this.ReadMessage = "null";
                                Logger.Writer.ConsoleWriteMessage( "Query results：NULL。" );
                                result.error = new Device_ErrorCode( OncRpcException.OncRpcSuccess );
                            }
                            break;
                    }
                }
                catch ( Exception ex )
                {
                    Logger.Writer.ConsoleWriteException( $"An error occurred when the method was called：{method}",ex );
                    //Parameter error
                    result.error = new Device_ErrorCode( OncRpcException.OncRpcProgramNotAvailable ); 
                }
            }
            else
            {
                Logger.Writer.ConsoleWriteMessage( $"No method found： {spciCommand}", ConsoleColor.DarkYellow );
                result.error = new Device_ErrorCode( OncRpcException.OncRpcProcedureNotAvailable ); // The instruction is incorrect or undefined
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

	    // Create embedded portmap service and check whether is has sprung
        // into action.

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
        catch ( OncRpcException e )
        {
            Logger.Writer.ConsoleWriteException( "ERROR: failed:", e );
        }

        Logger.Writer.ConsoleWriteMessage( "Passed." );
    }

    #endregion

    #region " START / STOP "

    private bool _Listening;
    /// <summary>   Gets or sets a value indicating whether the listening. </summary>
    /// <value> True if listening, false if not. </value>
    public bool Listening
    {
        get => this._Listening;
        set => _ = this.SetProperty( ref this._Listening, value );
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
    /// transports have been closed by calling the 
    /// <see cref="cc.isr.ONC.RPC.Server.OncRpcServerStubBase.Close(ONC.RPC.Server.OncRpcServerTransportBase[])"/>
    /// method of the server.
    /// </remarks>
    public override void StopRpcProcessing()
    {
        this.Listening = false;
        base.StopRpcProcessing();
    }

#endregion

}
