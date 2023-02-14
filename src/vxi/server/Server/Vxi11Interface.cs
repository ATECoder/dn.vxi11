using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.EnumExtensions;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.Server;

/// <summary>   Implementation of the <see cref="IVxi11Interface"/>. </summary>
/// <remarks>
/// This class implements a 'physical' instrument that is the end point for the instrument client
/// Virtual Instrument. The remote procedure call initiated at the VXI-11 client side, passes to
/// the instrument through a <see cref="Vxi11Device"/>, which links the <see cref="Vxi11Server"/>
/// and the 'physical' <see cref="Vxi11Interface"/>.
/// 
/// Implementations of VXI-11 servers should inherit from the <see cref="Vxi11Interface"/> and,
/// perhaps also, from the <see cref="Vxi11Device"/>.
/// 
/// Instrument classes inheriting from the <see cref="Vxi11Interface"/> might override a few
/// methods as necessary for implementing the designed behavior.
/// 
/// The <see cref="Vxi11Server"/> and <see cref="Vxi11Device"/> classes implement the device_xxx
/// remote procedure calls as specified in the
/// <see href="https://vxibus.org/specifications.html">VXI-11 TCP/IP Instrument Protocol
/// Specification</see> VXI-11 Version 1.0 document.
/// 
/// The VXI-11 device procedures are from the host perspective, i.e., a device write is writes to
/// the 'physical' instrument (also called 'Network Instrument') and device read reads from the
/// instrument.
/// </remarks>
public partial class Vxi11Interface : IVxi11Interface
{
    #region " construction and cleanup "

    public Vxi11Interface()
    {
        this.MessageLog = new CircularList<(int LinkId, char IO, DateTimeOffset Timestamp, String Value)>( IOMessageCapacity );
    }

    #endregion

    #region " Interface methods "

    /// <summary>   Sends a command; <see cref="InterfaceCommand.SendCommand"/> </summary>
    /// <param name="data"> The data. </param>
    /// <returns>   A byte[]. </returns>
    public virtual byte[] SendCommand( byte[] data )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads bus status; <see cref="InterfaceCommand.BusStatus"/>. </summary>
    /// <param name="interfaceCommand"> The interface command option <see cref="InterfaceCommandOption"/>. </param>
    /// <returns>   An int. </returns>
    public virtual int ReadBusStatus( InterfaceCommandOption interfaceCommand )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Read REN line; <see cref="InterfaceCommandOption.RemoteStatus"/>. </summary>
    /// <returns>   1 if the REN message is true, 0 otherwise. </returns>
    public virtual int ReadRenLine()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads service request (SRQ) line; <see cref="InterfaceCommandOption.ServiceRequestStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   1 if the SRQ message is true, 0 otherwise. </returns>
    public virtual int ReadServiceRequest()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads <see cref="InterfaceCommandOption.NotDataAcceptedLineStatus"/> NDAC line. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   1 if the NDAC message is true, 0 otherwise. </returns>
    public virtual int ReadNdacLine()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Check if interface device is a system controller; <see cref="InterfaceCommandOption.SystemControllerStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is in the
    /// system control active state, <see langword="false"/> otherwise. </returns>
    public virtual bool IsSystemController()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Check if interface device is the controller-in-charge; <see cref="InterfaceCommandOption.ControllerInChargeStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is not in the controller
    /// idle state, <see langword="false"/> otherwise.
    /// </returns>
    public virtual bool IsControllerInCharge()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Check if interface device is addressed as a talker; <see cref="InterfaceCommandOption.TalkerStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is addressed to talk, <see langword="false"/>
    /// otherwise.
    /// </returns>
    public virtual bool IsTalker()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Check if interface device is addressed as a listener; <see cref="InterfaceCommandOption.ListenerStatus"/>. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is addressed to listen, <see langword="false"/>
    /// </returns>
    public virtual bool IsListener()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Get interface device bus address; <see cref="InterfaceCommandOption.BusAddressStatus"/> </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <returns>   The TCP/IP-IEEE 488.1 Interface Device's address (0-30). </returns>
    public virtual int GetBusAddress()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region " Set bus commands "

    /// <summary>   Set ATN line; <see cref="InterfaceCommand.AttentionControl"/>. </summary>
    /// <remarks>
    /// TCP/IP-IEEE 488.1 Interface Device sets the ATN line as follows:
    /// <list type="number"><item>
    /// If the `data_in` parameter is non-zero, then set the ATN line true. </item><item>
    /// If the `data_in` parameter is zero, then set the ATN line false. </item></list>
    /// The returned `data_out` is the same as the received `data_in`.
    /// </remarks>
    /// <param name="value">  The value. Note that a <see cref="bool"/> <see langword="true"/> 
    /// is XDR encoded as 1.
    /// </param>
    public virtual bool SetAtnLine( bool value )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Sets REN line; <see cref="InterfaceCommand.RemoteEnableControl"/>. </summary>
    /// <remarks>
    /// TCP/IP-IEEE 488.1 Interface Device sets the REN line as follows:
    /// <list type="number"><item>
    /// If the `data_in` parameter is non-zero, then set the SRE( send remote enable) message true.  </item>
    /// <item>
    /// If the `data_in` parameter is zero, then set the SRE( send remote enable) message false.  </item>
    /// </list>
    /// The returned `data_out` is be same as the received `data_in`.
    /// </remarks>
    /// <param name="value">    The value. Note that a <see cref="bool"/> <see langword="true"/>
    ///                         is XDR encoded as 1. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    public virtual bool SetRenLine( bool value )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Pass control to another controller; <see cref="InterfaceCommand.PassControl"/>. </summary>
    /// <remarks>
    /// The TCP/IP-IEEE 488.1 Interface Device executes the `PASS CONTROL` control sequence described
    /// in IEEE 488.2, 16.2.14 where the talk address is constructed from the value in `data_in`
    /// bitwise OR-ed with 0x80. The returned `data_out` is the same as the received `data_in`.
    /// </remarks>
    /// <param name="addr"> The address. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    public virtual bool PassControl( int addr )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Set interface device bus address; <see cref="InterfaceCommand.BusAddress"/>. </summary>
    /// <remarks>
    /// the TCP/IP-IEEE 488.1 Interface Device sets its address to the contents of `data_in`. If
    /// `data_in` does not contain a legal value, device_docmd returns immediately with error set to
    /// `parameter error` (5). The returned `data_out` is the same as the received `data_in`.
    /// </remarks>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <param name="addr"> The address. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    public virtual bool SetBusAddress( int addr )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Send Interface Clear; <see cref="InterfaceCommand.InterfaceClearControl"/> (IFC). </summary>
    /// <remarks>   TCP/IP-IEEE 488.1 Interface Device
    /// executes the `SEND IFC` control sequence described in IEEE 488.2, 16.2.8. The returned `data_out`
    /// has `data_out.data_out_len` set to zero. </remarks>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <returns>   A byte[]. </returns>
    public virtual byte[] SendInterfaceClear()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region " instrument state "

    private bool _lockEnabled;
    /// <summary>   Gets or sets a value indicating whether lock is requested on the device. </summary>
    /// <value> True if lock enabled, false if not. </value>
    public bool LockEnabled
    {
        get => this._lockEnabled;
        set => _ = this.OnPropertyChanged( ref this._lockEnabled, value );
    }

    private bool _remoteEnabled;
    /// <summary>   Gets or sets a value indicating whether the remote is enabled. </summary>
    /// <value> True if remote enabled, false if not. </value>
    public bool RemoteEnabled
    {
        get => this._remoteEnabled;
        set => _ = this.OnPropertyChanged( ref this._remoteEnabled, value );
    }

    #endregion

    #region " RPC operation members "

    /// <summary>   Query if 'command' is supported command. </summary>
    /// <remarks>   2023-02-10. </remarks>
    /// <param name="command">  The command. </param>
    /// <returns>   True if supported command, false if not. </returns>
    public bool IsSupportedCommand( int command )
    {
        return ( command >= ( int )  InterfaceCommandOption.ListenerStatus
                && command <= ( int ) InterfaceCommandOption.BusAddressStatus )
            || ( command >= ( int ) InterfaceCommand.SendCommand
                && command <= ( int ) InterfaceCommand.InterfaceClearControl );
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceDoCmdParms"/> to use
    ///                         with the remote procedure call. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="DeviceDoCmdResp"/>. </returns>
    public DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request )
    {
        // TODO: Implement interface operations here based on the parsing of the request.

        if ( !this.IsSupportedCommand(  request.Cmd ) )
        {
            Logger.Writer.LogVerbose( $"{request.Cmd} is not a supported interface command." );
            return new DeviceDoCmdResp();
        }
        else if ( request.Cmd < ( int ) InterfaceCommand.SendCommand )
        {
            InterfaceCommandOption cmd = ( ( int ) request.Cmd).ToInterfaceCommandOption();
            Logger.Writer.LogVerbose( $"Implementing： {cmd}({request.Cmd})" );
            return this.DeviceDoCmd( cmd );
        }
        else if ( request.Cmd <= ( int ) InterfaceCommand.InterfaceClearControl )
        {
            InterfaceCommand cmd = (( int ) request.Cmd).ToInterfaceCommand();
            Logger.Writer.LogVerbose( $"Implementing： {cmd}({request.Cmd})" );
            return this.DeviceDoCmd( request, cmd );
        }
        else
        {
            string message = $"{request.Cmd} is unexpected yet unsupported interface command.";
            Logger.Writer.LogVerbose( message );
            this.LogMessage( 'c', message );
        }

        return new DeviceDoCmdResp();
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-02-10. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceDoCmdParms"/> to use
    ///                         with the remote procedure call. </param>
    /// <param name="command">  The <see cref="InterfaceCommand"/> command. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="DeviceDoCmdResp"/>. </returns>
    public DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request, InterfaceCommand command )
    {
        // TODO: Finish implementing the commands.

        XdrBufferEncodingStream encoder = new( 128 );
        XdrBufferDecodingStream decoder = new( request.GetDataIn() );

        DeviceDoCmdResp reply = new();
        switch ( command )
        {
            case InterfaceCommand.SendCommand:
                reply.SetDataOut( this.SendCommand( request.GetDataIn() ) );
                break;
            case InterfaceCommand.BusStatus:
                break;
            case InterfaceCommand.AttentionControl:
                break;
            case InterfaceCommand.RemoteEnableControl:
                break;
            case InterfaceCommand.PassControl:
                encoder.EncodeBoolean( this.PassControl( decoder.DecodeInt() ) );
                break;
            case InterfaceCommand.BusAddress:
                break;
            case InterfaceCommand.InterfaceClearControl:
                reply.SetDataOut( this.SendInterfaceClear() );
                break;
            default:
                break;
        }

        return reply;
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-02-10. </remarks>
    /// <param name="command">  The <see cref="InterfaceCommand"/> command. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="DeviceDoCmdResp"/>. </returns>
    public DeviceDoCmdResp DeviceDoCmd( InterfaceCommandOption command )
    {
        // TODO: Finish implementing the commands.

        XdrBufferEncodingStream encoder = new( 128 );
        DeviceDoCmdResp reply = new();
        switch ( command )
        {
            case InterfaceCommandOption.RemoteStatus:
                encoder.EncodeInt( this.ReadRenLine() );
                reply.SetDataOut( encoder.GetEncodedData() );
                break;
            case InterfaceCommandOption.ServiceRequestStatus:
                break;
            case InterfaceCommandOption.NotDataAcceptedLineStatus:
                break;
            case InterfaceCommandOption.SystemControllerStatus:
                break;
            case InterfaceCommandOption.ControllerInChargeStatus:
                break;
            case InterfaceCommandOption.TalkerStatus:
                break;
            case InterfaceCommandOption.ListenerStatus:
                break;
            case InterfaceCommandOption.BusAddressStatus:
                encoder.EncodeInt( this.ReadBusStatus( command ) );
                reply.SetDataOut( encoder.GetEncodedData() );
                break;
            default:
                break;
        }
        return reply;
    }

    private int _activeClientId;
    /// <summary>   Gets or sets the identifier of the active client. </summary>
    /// <remarks> Used solely for generating log messages. </remarks>
    /// <value> The identifier of the active client. </value>
    public int ActiveClientId
    {
        get => this._activeClientId;
        set => _ = this.OnPropertyChanged( ref this._activeClientId, value );
    }

    /// <summary>   Gets or sets the i/o message capacity. </summary>
    /// <value> The i/o message capacity. </value>
    public static int IOMessageCapacity { get; set; } = 127;

    /// <summary>   Gets a <see cref="CircularList{T}"/> of (<see cref="DateTime"/> Timestamp, <see cref="String"/> Value)
    /// of the last messages that were sent to and received from the instrument. </summary>
    /// <value> The list of message tuples consisting of the Client Id, IO (R for read and W for write), 
    /// a timestamp and a value that were sent to or received from the instrument. </value>
    public List<(int ClientId, char IO, DateTimeOffset Timestamp, String Value)> MessageLog { get; }

    private int _messageLogCount;
    /// <summary>   Gets or sets the number of I/O messages. </summary>
    /// <value> The number of I/O messages, which, in fact, flags the property change flag that can be used to 
    /// indicate the availability of new messages. </value>
    public int MessageLogCount
    {
        get => this._messageLogCount;
        set => _ = this.OnPropertyChanged( ref this._messageLogCount, value );
    }

    /// <summary>   Logs a message. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="operationType">    Type of the operation. </param>
    /// <param name="value">            The value. </param>
    private void LogMessage( char operationType, string value )
    {
        this.MessageLog.Add( (this.ActiveClientId, operationType, DateTimeOffset.Now, value) );
        this.MessageLogCount++;
    }

    private DeviceErrorCode _lastDeviceError;
    /// <summary>   Gets or sets the last device error. </summary>
    /// <value> The las <see cref="DeviceErrorCode"/> . </value>
    public DeviceErrorCode LastDeviceError
    {
        get => this._lastDeviceError;
        set => _ = this.SetProperty( ref this._lastDeviceError, value );
    }

    #endregion

}
