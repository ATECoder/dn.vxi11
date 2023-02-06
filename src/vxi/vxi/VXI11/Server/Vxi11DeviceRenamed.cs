using System.Net;
using System.Reflection;

using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.Server;

/// <summary>   Implements the VXI device. </summary>
public partial class Vxi11DeviceRenamed : IVxi11Device
{

    #region " construction and cleanup "

    /// <summary>   Constructor. </summary>
    /// <param name="identity"> (Optional) Device identification string. </param>
    public Vxi11DeviceRenamed( string identity = "INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434" )
    {
        this._identity = identity;
        this.WriteMessage = string.Empty;
        this._writeMessage = string.Empty;
        this.ReadMessage = string.Empty;
        this._readMessage = string.Empty;
        this._readBuffer = Array.Empty<byte>();
        this.CharacterEncoding = CoreChannelClient.EncodingDefault;
        this._characterEncoding = CoreChannelClient.EncodingDefault;
        this._host = string.Empty;
        this._interfaceDeviceString = string.Empty;
        this.DeviceLink = null;
        this._deviceLink = null;
        this.MaxReceiveLength = VXI11.Client.Vxi11Client.MaxReceiveLengthDefault;
        this.AbortPortNumber = AbortChannelServer.AbortPortDefault;
        this.Host = string.Empty;
        this._host = string.Empty;
        this.ReadTermination = VXI11.Client.Vxi11Client.ReadTerminationDefault;
        this.WriteTermination = VXI11.Client.Vxi11Client.WriteTerminationDefault;
        this._writeTermination = VXI11.Client.Vxi11Client.WriteTerminationDefault;
    }

    #endregion

    #region " device operations "

    /// <summary>   Clears status: <see cref="Vxi11InstrumentCommands.CLS"/>. </summary>
    /// <remarks>
    /// Clear Status Command. Clears the event registers in all register groups. Also clears the
    /// error queue.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.CLS, Vxi11InstrumentOperationType.Write )]
    public bool CLS()
    {
        return true;
    }

    /// <summary>   Enables Standard Event Status: <see cref="Vxi11InstrumentCommands.ESE"/>. </summary>
    /// <remarks>
    /// Enables bits in the enable register for the Standard Event Register group. The selected bits
    /// are then reported to bit 5 of the Status Byte Register. Accepts the decimal sum of the bits
    /// in the register; default 0. For example, to enable bit 2 (value 4), bit 3 (value 8), and bit
    /// 7 (value 128), the decimal sum would be 140 (4 + 8 + 128). For example, *ESE 48 enables bit 4
    /// (value 16) and bit 5 (value 32) in the enable register.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.ESE, Vxi11InstrumentOperationType.Write )]
    public bool ESE()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads Standard Event Status: *ESE?
    /// </summary>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.ESERead, Vxi11InstrumentOperationType.Read )]
    public string ESERead()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Standard Event Status Register Query: <see cref="Vxi11InstrumentCommands.ESRRead"/>. </summary>
    /// <remarks>
    /// Queries the event register for the Standard Event Register group. Register is read-only; bits
    /// not cleared when read. Any or all conditions can be reported to the Standard Event summary
    /// bit through the enable register.To set the enable register mask, write a decimal value to the
    /// register using *ESE. Once a bit is set, it remains set until cleared by this query or *CLS.
    /// </remarks>
    /// <returns>   A string. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.ESRRead, Vxi11InstrumentOperationType.Read )]
    public string ESRRead()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads the device identity string: <see cref="Vxi11InstrumentCommands.IDNRead"/></summary>
    /// <returns>   A string. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.IDNRead, Vxi11InstrumentOperationType.Read )]
    public string IDNRead()
    {
        return this.Identity;
    }

    /// <summary>   Operation completion instruction: <see cref="Vxi11InstrumentCommands.OPC"/>. </summary>
    /// <remarks>
    /// Sets "Operation Complete" (bit 0) in the Standard Event register at the completion of the
    /// current operation. The purpose of this command is to synchronize your application with the
    /// instrument. Used in triggered sweep, triggered burst, list, or arbitrary waveform sequence
    /// modes to provide a way to poll or interrupt the computer when the *TRG or
    /// INITiate[:IMMediate] is complete. Other commands may be executed before Operation Complete
    /// bit is set. The difference between *OPC and *OPC? is that *OPC? returns "1" to the output
    /// buffer when the current operation completes. This means that no further commands can be sent
    /// after an *OPC? until it has responded. In this way an explicit polling loop can be avoided.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.OPC, Vxi11InstrumentOperationType.Write )]
    public bool OPC()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads the operation completion status: <see cref="Vxi11InstrumentCommands.OPCRead"/> </summary>
    /// <remarks>
    /// Returns 1 to the output buffer after all pending commands complete. : The purpose of this
    /// command is to synchronize your application with the instrument. Other commands cannot be
    /// executed until this command completes. The difference between *OPC and
    /// *OPC? is that *OPC? returns "1" to the output buffer when the current operation
    /// completes. This means that no further commands can be sent after an *OPC? until it has
    /// responded. In this way an explicit polling loop can be avoided.That is, the IO driver will
    /// wait for the response.
    /// </remarks>
    /// <returns>   Returns 1 when all previous commands complete. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.OPCRead, Vxi11InstrumentOperationType.Read )]
    public string OPCRead()
    {
        return "1";
    }

    /// <summary>   Resets the device: <see cref="Vxi11InstrumentCommands.RST"/>. </summary>
    /// <remarks>
    /// Resets instrument to factory default state, independent of MEMory:STATe:RECall:AUTO setting.
    /// Does not affect stored instrument states, stored arbitrary waveforms, or I/O settings; these
    /// are stored in non-volatile memory. Aborts a sweep or burst in progress.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.RST, Vxi11InstrumentOperationType.Write )]
    public bool RST()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables the service request events: <see cref="Vxi11InstrumentCommands.SRE"/>. </summary>
    /// <remarks>
    /// This command enables bits in the enable register for the Status Byte Register group.
    /// Parameters consists of the decimal sum of the bits in the register; default 0. For example,
    /// to enable bit 2 (value 4), bit 3 (value 8), and bit 7 (value 128), the decimal sum would be
    /// 140 (4 + 8 + 128). for example, *SRE 24 enables bits 3 and 4 in the enable register. To
    /// enable specific bits, specify the decimal value corresponding to the binary-weighted sum of
    /// the bits in the register.The selected bits are summarized in the "Master Summary" bit (bit 6)
    /// of the Status Byte Register. If any of the selected bits change from 0 to 1, the instrument
    /// generates a Service Request signal.
    /// *CLS clears the event register, but not the enable register.
    /// *PSC (power-on status clear) determines whether Status Byte enable register is cleared at
    /// power on.For example, *PSC 0 preserves the contents of the enable register through power
    /// cycles. Status Byte enable register is not cleared by *RST.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.SRE, Vxi11InstrumentOperationType.Write )]
    public bool SRE()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Reads the service request enabled status: <see cref="Vxi11InstrumentCommands.SRERead"/> </summary>
    /// <returns>   A string. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.SRERead, Vxi11InstrumentOperationType.Read )]
    public string SRERead()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Read the status byte: *STB?
    /// </summary>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.STBRead, Vxi11InstrumentOperationType.Read )]
    public string STBRead()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Trigger command: *TRG. </summary>
    /// <remarks>
    /// Triggers a sweep, burst, arbitrary waveform advance, or LIST advance from the remote
    /// interface if the bus (software) trigger source is currently selected (TRIGger[1|2]:SOURce
    /// BUS).
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.TRG, Vxi11InstrumentOperationType.Write )]
    public bool TRG()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Runs a self test and reads its status: <see cref="Vxi11InstrumentCommands.TSTRead"/>. </summary>
    /// <remarks>
    /// Self-Test Query. Performs a complete instrument self-test. If test fails, one or more error
    /// messages will provide additional information. Use SYSTem:ERRor? to read error queue.
    /// </remarks>
    /// <returns>   Returns +0 (pass) or +1 (one or more tests failed). </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.TSTRead, Vxi11InstrumentOperationType.Read )]
    public string TSTRead()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Wait until all pending operations complete. <see cref="Vxi11InstrumentCommands.WAI"/>. </summary>
    /// <remarks>
    /// Configures the instrument to wait for all pending operations to complete before executing any
    /// additional commands over the interface. For example, you can use this with the *TRG command
    /// to ensure that the instrument is ready for a trigger:
    /// *TRG;*WAI;*TRG.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.WAI, Vxi11InstrumentOperationType.Write )]
    public bool WAI()
    {
        throw new NotImplementedException();
    }

    /// <summary>   The current operation instruction type. </summary>
    /// <value> The type of the current operation. </value>
    public Vxi11InstrumentOperationType CurrentOperationType { get; set; } = Vxi11InstrumentOperationType.None;

    #endregion

    #region " thread exception handler "

    /// <summary>
    /// Event queue for all listeners interested in ThreadExceptionOccurred events.
    /// </summary>
    public event ThreadExceptionEventHandler? ThreadExceptionOccurred;

    /// <summary>   Executes the <see cref="ThreadExceptionOccurred"/> event. </summary>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    protected virtual void OnThreadException( ThreadExceptionEventArgs e )
    {
        var handler = this.ThreadExceptionOccurred;
        handler?.Invoke( this, e );
    }

    #endregion

    #region " run long operation in the background "

    private bool _longOperationRunning;
    /// <summary>   Gets or sets a value indicating whether the long operation running. </summary>
    /// <value> True if long operation running, false if not. </value>
    public bool LongOperationRunning
    {
        get => this._longOperationRunning;
        set => _ = this.OnPropertyChanged( ref this._longOperationRunning, value );
    }

    /// <summary>   Starts long operation asynchronous. </summary>
    /// <param name="cancelSource"> The cancellation source that allows processing to be canceled. </param>
    /// <returns>   A Task. </returns>
    private async Task StartLongOperationAsync( CancellationTokenSource cancelSource )
    {
        await Task.Factory.StartNew( () => { this.RunLongOperation( cancelSource ); } )
                .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ),
                                                                              TaskContinuationOptions.OnlyOnFaulted );
    }

    /// <summary>
    /// Override this method to execute a long operation.
    /// </summary>
    /// <param name="cancelSource"> The cancellation source that allows processing to be canceled. </param>
    protected virtual void RunLongOperation( CancellationTokenSource cancelSource ) { }

    /// <summary>   Stops the long operation. </summary>
    /// <param name="cancelSource"> The cancel source. </param>
    private void StopLongOperation( CancellationTokenSource cancelSource )
    {
        cancelSource.Cancel();
    }

    /// <summary>
    /// Gets or sets (<see langword="private"/>) the notification flag for signaling the server to stop processing
    /// incoming remote procedure calls and to shut down.
    /// </summary>
    /// <value> The shutdown signal. </value>
    internal object ShutdownSignal { get; private set; } = new();

    /// <summary>
    /// Run a long operation until the operation completes and sends the <see cref="ShutdownSignal"/>.
    /// </summary>
    public virtual void RunLongOperation()
    {

        this.LongOperationRunning = true;

        // Create the cancellation source.
        CancellationTokenSource cts = new();

        try
        {

            _ = this.StartLongOperationAsync( cts );

            // Loop and wait for the shutdown flag to become signaled. If the
            // server's main task gets interrupted it will not shut itself
            // down. It can only be stopped by signaling the shutdownSignal
            // object.
            for (; ; )
                lock ( this.ShutdownSignal )
                    try
                    {
                        _ = Monitor.Wait( this.ShutdownSignal );
                        break;
                    }
                    catch ( Exception )
                    {
                    }
        }
        catch ( Exception )
        {
            throw;
        }
        finally
        {

            this.StopLongOperation( cts );

            this.LongOperationRunning = false;
        }

    }

    /// <summary>   Starts the long operation using the all inclusive <see cref="RunLongOperation()"/> asynchronously. </summary>
    /// <returns>   A Task. </returns>
    public virtual async Task RunLongOperationAsync()
    {
        await Task.Factory.StartNew( () => { this.RunLongOperation(); } )
                    .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ),
                                                                                TaskContinuationOptions.OnlyOnFaulted );
    }

    /// <summary>
    /// Notify the long running operation to stop as soon as possible.
    /// </summary>
    public virtual void StopLongOperation()
    {
        if ( this.ShutdownSignal is not null )
            lock ( this.ShutdownSignal )
                Monitor.PulseAll( this.ShutdownSignal );
    }

    public bool TryStopLongOperation( int timeout = 100, int loopDelay = 5 )
    {
        List<Exception> exceptions = new();

        try
        {
            if ( this.LongOperationRunning )
                this.StopLongOperation();
        }
        catch ( Exception ex )
        { exceptions.Add( ex ); }

        try
        {
            DateTime endTime = DateTime.Now.AddMilliseconds( timeout );
            while ( this.LongOperationRunning && endTime > DateTime.Now )
            {
                Task.Delay( loopDelay ).Wait();
            }
            if ( this.LongOperationRunning )
                throw new InvalidOperationException( "Long operation still running after sending the stop signal." );
        }
        catch ( Exception ex )
        { exceptions.Add( ex ); }
        finally
        {
        }

        if ( exceptions.Any() )
        {
            AggregateException aggregateException = new( exceptions );
            throw aggregateException;
        }
        else
            return !this.LongOperationRunning;

    }

    /// <summary>   Shutdown asynchronous. </summary>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    /// <returns>   A Task. </returns>
    public virtual async Task<bool> TryStopLongOperationAsync( int timeout = 1000, int loopDelay = 25 )
    {
        bool result = false;
        _ = await Task<bool>.Factory.StartNew( () => {
            result = this.TryStopLongOperation( timeout, loopDelay );
            return result;
        } )
              .ContinueWith(
            failedTask => {
                result = false;
                this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) );
                return result;
            }, TaskContinuationOptions.OnlyOnFaulted );
        return result;
    }

    #endregion

    #region " Vxi11Device Interface implementation "
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

    /// <summary>   Handles abort request. </summary>
    /// <param name="e">        Device error event information. </param>
    public void HandleAbortRequest( DeviceErrorEventArgs e )
    {
        Task<bool> t = this.TryStopLongOperationAsync();
        t.Wait();
        e.ErrorCodeValue = t.Result ? DeviceErrorCode.Abort : DeviceErrorCode.IOError;
    }

    #endregion

    #region " interrupt client "

    private bool _interruptEnabled;
    public bool InterruptEnabled
    {
        get => this._interruptEnabled;
        set => _ = this.OnPropertyChanged( ref this._interruptEnabled, value );
    }

    private int _clientId;
    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    public int ClientId
    {
        get => this._clientId;
        set => _ = this.OnPropertyChanged( ref this._clientId, value );
    }

    /// <summary>
    /// Call the remote interrupt procedure <see cref="Vxi11Message.DeviceInterruptSrqProcedure"/> sending an 
    /// interrupt request to the client of this server device .
    /// </summary>
    /// <param name="handle">   The handle as it was received from the Core device. </param>
    public void DeviceIntrSrq( byte[] handle )
    {
        // TODO: Implement
    }

    /// <summary>   Sends an asynchronous Interrupt an in-progress call. </summary>
    /// <param name="handle">   The handle as it was received from the Core device. </param>
    public void Interrupt( byte[] handle )
    {
        // TODO: Implement
    }

    #endregion

    #region " service requested event handler "

    public event EventHandler<Vxi11EventArgs>? ServiceRequested;

    /// <summary>   Executes the <see cref="ServiceRequested"/> event. </summary>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    protected virtual void OnServiceRequested( Vxi11EventArgs e )
    {
        // TODO: not sure if we needs this. Probably belongs on teh client only.
        var handler = this.ServiceRequested;
        handler?.Invoke( this, e );
    }

    #endregion

    #region " members "

    private string _host;
    /// <summary>   Gets or sets the host IPv4 Address of this device server. </summary>
    /// <value> The host. </value>
    public string Host
    {
        get => this._host;
        set => _ = this.OnPropertyChanged( ref this._host, value );
    }

    /// <summary>   Gets the IP address. </summary>
    /// <value> The IP address. </value>
    public IPAddress IPAddress => string.IsNullOrEmpty( this.Host ) ? IPAddress.Any : IPAddress.Parse( this.Host );

    private string _interfaceDeviceString;
    /// <summary>
    /// Gets or sets the interface device string, .e.g, inst0, gpib0,5, or usb0[...].
    /// </summary>
    /// <value> The interface device string. </value>
    public string InterfaceDeviceString
    {
        get => this._interfaceDeviceString;
        set => _ = this.OnPropertyChanged( ref this._interfaceDeviceString, value );
    }

    /// <summary>   Query if this device has valid interface device string. </summary>
    /// <remarks> This is required for validating the interface device string when creating the link. </remarks>
    /// <returns>   True if valid interface device string, false if not. </returns>
    public bool IsValidInterfaceDeviceString()
    {
        InsterfaceDeviceStringParser parser = new( this.InterfaceDeviceString );
        return parser.IsValid();
    }

    private bool _eoi;
    /// <summary>
    /// Gets or sets a value indicating whether the end-or-identify (EOI) terminator is enabled.
    /// </summary>
    /// <remarks>
    /// The driver must be configured so that when talking on the bus it sends a write termination
    /// string (e.g., a line-feed or line-feed followed by a carriage return) with EOI as the
    /// terminator, and when listening on the bus it expects a read termination (e.g., a line-feed)
    /// with EOI as the terminator. The IEEE-488.2 EOI (end-or-identify) message is interpreted as a
    /// <c>new line</c> character and can be used to terminate a message in place of a <c>new line</c>
    /// character. A <c>carriage return</c> followed by a <c>new line</c> is also accepted. Message
    /// termination will always reset the current SCPI message path to the root level.
    /// </remarks>
    /// <value> True if EOI is enabled, false if not. </value>
    public bool Eoi
    {
        get => this._eoi;
        set => _ = this.OnPropertyChanged( ref this._eoi, value );
    }

    private byte _readTermination;
    /// <summary>   Gets or sets the read termination. </summary>
    /// <value> The read termination. </value>
    public byte ReadTermination
    {
        get => this._readTermination;
        set => _ = this.OnPropertyChanged( ref this._readTermination, value );
    }

    private bool _connected;
    /// <summary>   Gets or sets a value indicating whether the VXI Core Client is connected. </summary>
    /// <value> True if connected, false if not. </value>
    public bool Connected
    {
        get => this._connected;
        set => _ = this.OnPropertyChanged( ref this._connected, value );
    }


    private int _connectTimeout;
    /// <summary>   Gets or sets the connect timeout. </summary>
    /// <remarks>
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned integer because the timeout value is unlikely to exceed the maximum integer
    /// value.
    /// </remarks>
    /// <value> The connect timeout. </value>
    public int ConnectTimeout
    {
        get => this._connectTimeout;
        set => _ = this.OnPropertyChanged( ref this._connectTimeout, value );
    }

    private string _identity;
    /// <summary>
    /// Device identification string
    /// </summary>
    public string Identity
    {
        get => this._identity;
        set => _ = this.OnPropertyChanged( ref this._identity, value );
    }

    private int _ioTimeout;
    /// <summary>   Gets or sets the I/O timeout. </summary>
    /// <value> The I/O timeout. </value>
    public int IOTimeout
    {
        get => this._ioTimeout;
        set => _ = this.OnPropertyChanged( ref this._ioTimeout, value );
    }

    private int _transmitTimeout;
    /// <summary>
    /// Gets or sets the timeout during the phase where data is sent within RPC calls, or data is
    /// received within RPC replies. The <see cref="TransmitTimeout"/> timeout must be greater than 0.
    /// </summary>
    /// <value> The Transmit timeout. </value>
    public int TransmitTimeout
    {
        get => this._transmitTimeout;
        set => _ = this.OnPropertyChanged( ref this._transmitTimeout, value );
    }

    private DeviceErrorCode _lastDeviceError;
    /// <summary>   Gets or sets the last device error. </summary>
    /// <value> The las <see cref="DeviceErrorCode"/> . </value>
    public DeviceErrorCode LastDeviceError
    {
        get => this._lastDeviceError;
        set => _ = this.SetProperty( ref this._lastDeviceError, value );
    }

    private int _lockTimeout;
    /// <summary>   Gets or sets the lock timeout in milliseconds. </summary>
    /// <remarks>
    /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a
    /// lock to be released. If the device is locked by another link and the <see cref="LockTimeout"/>
    /// is non-zero, the network instrument server allows at least <see cref="LockTimeout"/>
    /// milliseconds for a lock to be released. <para>
    /// 
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned integer because the timeout value is unlikely to exceed the maximum integer
    /// value. </para>
    /// </remarks>
    /// <value> The lock timeout. </value>
    public int LockTimeout
    {
        get => this._lockTimeout;
        set => _ = this.OnPropertyChanged( ref this._lockTimeout, value );
    }

    private int _maxReceiveLength;
    /// <summary>   Gets or sets the maximum length of the receive. </summary>
    /// <value> The maximum length of the receive. </value>
    public int MaxReceiveLength
    {
        get => this._maxReceiveLength;
        set => _ = this.OnPropertyChanged( ref this._maxReceiveLength, value );
    }

    private byte[] _writeTermination;
    /// <summary>   Gets or sets the write termination. </summary>
    /// <value> The write termination. </value>
    public byte[] WriteTermination
    {
        get => this._writeTermination;
        set => _ = this.OnPropertyChanged( ref this._writeTermination, value );
    }

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

    private Encoding _characterEncoding;
    /// <summary>
    /// Gets or sets the encoding to use when serializing strings. If <see langcref="null" />, the
    /// system's default encoding is to be used.
    /// </summary>
    /// <value> The character encoding. </value>
    public Encoding CharacterEncoding
    {
        get => this._characterEncoding;
        set => _ = this.SetProperty( ref this._characterEncoding, value );
    }

    private int _waitOnOutTime = 1000;
    /// <summary>   Timeout wait time ms. </summary>
    /// <value> The wait on out time. </value>
    public int WaitOnOutTime
    {
        get => this._waitOnOutTime;
        set => _ = this.SetProperty( ref this._waitOnOutTime, value );
    }


    #endregion

    #region " Device state "

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

    #region " RPC implementations "

    private DeviceLink? _deviceLink;
    /// <summary>   Gets or sets the device link to the actual single device. </summary>
    /// <value> The device link. </value>
    public DeviceLink? DeviceLink
    {
        get => this._deviceLink;
        set => _ = this.OnPropertyChanged( ref this._deviceLink, value );
    }

    /// <summary>
    /// Query if the server can create a new device link given that this is a
    /// single device server.
    /// </summary>
    /// <remarks>   2023-01-28. </remarks>
    /// <returns>   True if device free, false if not. </returns>
    public bool CanCreateNewDeviceLink()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Aborts and returns the <see cref="DeviceError"/>. </summary>
    /// <remarks>
    /// To successfully complete a <c>device_abort</c> RPC, a network instrument server SHALL: <para>
    /// 
    /// 1. Initiate termination of any core channel, in-progress RPC associated with the link except
    /// destroy_link, device_enable_srq, and device_unlock. </para><para>
    /// 
    /// 2. Return with error set to 0, no error, to indicate successful completion </para><para>
    /// 
    /// The intent of this rule is to handle the <c>device_abort</c> RPC ahead of the other operations, but
    /// due to operating system specific implementation details the timeliness cannot be guaranteed. </para>
    /// <para>
    /// 
    /// The <c>device_abort</c> RPC only aborts an in-progress RPC, not a queued RPC. </para><para>
    /// 
    /// After replying to the <c>device_abort</c> call, the network instrument server SHALL reply to the
    /// original in-progress call which was aborted with error set to 23, aborted.  </para><para>
    /// 
    /// Receiving 0 on the abort call at the network instrument client only means that the abort was
    /// successfully delivered to the network instrument server. </para><para>
    /// 
    /// The <c>link id</c> parameter is compared against the active link identifiers . If none match,
    /// <c>device_abort</c> SHALL terminate with error set to 4 invalid link identifier.  </para><para>
    /// 
    /// The operation of <c>device_abort</c> SHALL NOT be affected by locking  </para>
    /// </remarks>
    /// <returns>   A DeviceError. </returns>
    public DeviceError Abort()
    {
        return new DeviceError();
    }

    /// <summary>   Create a device connection; Opens a link to a device. </summary>
    /// <remarks>
    /// To successfully complete a create_link RPC, a network instrument server SHALL: <para>
    /// 1. If lockDevice is set to true, acquire the lock for the device. </para><para>
    /// 2. Return in <c>link id</c> a link identifier to be used with future calls. The value of <c>
    /// link id</c> SHALL be
    /// unique for all currently active links within a network instrument server.  </para><para>
    /// 3. Return in maxRecvSize the size of the largest data parameter the network instrument server
    /// can accept in a <c>device_write</c>  RPC.This value SHALL be at least 1024.  </para><para>
    /// 4. Return in asyncPort the port number for asynchronous RPCs. See device_abort.  </para><para>
    /// 5. Return with error set to 0, no error, to indicate successful completion.  </para><para>
    /// 
    /// The device parameter is a string which identifies the device for communications.See the
    /// document(s) referred to in section A.6, Related Documents, for definitions of this string.  </para>
    /// <para>
    /// 
    /// A network instrument server should be able to maintain at least two separate links
    /// simultaneously over a single network instrument connection. </para><para>
    /// 
    /// The network instrument client sends an identifying number in the clientId parameter.While
    /// this protocol requires no special behavior based on the value of clientId, the device may
    /// provide a local means to examine its value to help a user identify communication problems. </para>
    /// <para>
    /// 
    /// The network instrument server SHALL NOT alter its function based on the clientId. </para><para>
    /// 
    /// If create_link is called when another link is not available, create_link SHALL terminate and
    /// set error to 9. </para><para>
    /// 
    /// The operation of create_link SHALL ignore locks if lockDevice is false. </para><para>
    /// If lockDevice is true and the lock is not freed after at least <c>lock_timeout</c>
    /// milliseconds, create_link SHALL terminate without creating a link and return with error set
    /// to 11, device locked by another link.Page 26 Section B: Network Instrument Protocol October 4,
    /// 2000 Printing VXIbus Specification: VXI-11 Revision 1.0 </para><para>
    /// 
    /// The execution of create_link SHALL have no effect on the state of any device associated with
    /// the network instrument server. </para><para>
    /// 
    /// A create_link RPC cannot be aborted since a valid link identifier is not yet available.A
    /// network instrument client should set <c>lock_timeout</c> to a reasonable value to avoid
    /// locking up the server. </para>
    /// </remarks>
    /// <param name="request">  The request of type <see cref="CreateLinkParms"/> to use with the
    ///                         remote procedure call. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="DeviceError"/>. </returns>
    public CreateLinkResp CreateLink( CreateLinkParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Destroy a connection. </summary>
    /// <remarks>
    /// To successfully complete a destroy_link RPC, a network instrument server SHALL: <para>
    /// 1. Deactivate the link identifier and recover any resources associated with the link. </para><para>
    /// 2. If this link has the lock, free the lock (see <c>device_lock</c> and create_link). </para><para>
    /// 3. Disable this link from using the interrupt mechanism( see <c>device_enable_srq</c> ). </para>
    /// <para>
    /// 4. Return with error set to 0, no error, to indicate successful completion. </para><para>
    /// The Device_Link( link identifier ) parameter is compared against the active link identifiers.
    /// If none match, destroy_link SHALL terminate and set error to 4. </para><para>
    /// 
    /// After a destroy_link, the network instrument server typically becomes ready to execute a new
    /// create_link, assuming the resources have not already been utilized. </para><para>
    /// 
    /// The execution of destroy_link SHALL have no effect on the state of any device associated with
    /// the network instrument server. </para><para>
    /// 
    /// The operation of destroy_link SHALL NOT be affected by device_abort. </para>
    /// </remarks>
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceLink"/> to use
    ///                         with the remote procedure call. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="DeviceError"/>. </returns>
    public DeviceError DestroyLink( DeviceLink request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Create an interrupt channel. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceRemoteFunc"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>   The new interrupt channel 1. </returns>
    public DeviceError CreateIntrChan( DeviceRemoteFunc request )
    {
        throw new NotImplementedException();
    }


    /// <summary>   Destroy an interrupt channel. </summary>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceError"/>.
    /// </returns>
    public DeviceError DestroyIntrChan()
    {
        throw new NotImplementedException();
    }

    /// <summary>   Device clear. </summary>
    /// <remarks>
    /// Since not all devices directly support a clear operation, how this operation is executed
    /// depends upon the interface between the network instrument server and the device. <para>
    /// If the device does not support a clear operation and the network instrument server is able to
    /// detect this, device_clear SHALL terminate and set error to 8, operation not supported. </para>
    /// <para>
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match,
    /// device_clear SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// If some other link has the lock, device_clear SHALL examine the <c>waitlock</c> flag in <c>flags</c> . If the
    /// flag is set, device_clear SHALL block until the lock is free. If the flag is not set,
    /// device_clear SHALL terminate with error set to 11, device locked by another link. </para><para>
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, device_clear SHALL
    /// terminate with error set to 11, device locked by another device. </para><para>
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, device_clear SHALL
    /// terminate with error set to 15, I/O timeout. </para><para>
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// clear the device, device_clear SHALL terminate with error set to 17, I/O error. </para><para>
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, device_clear SHALL terminate
    /// with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceError"/>.
    /// </returns>
    public DeviceError DeviceClear( DeviceGenericParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceDoCmdParms"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceDoCmdResp"/>.
    /// </returns>
    public DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device enables or does not enable the Send Request service. </summary>
    /// <remarks>
    /// The <c>device_enable_srq</c> RPC contains a handle parameter. The same data contained in
    /// handle is passed back in the handle parameter of the <c>device_intr_srq</c> RPC. Since the
    /// same data is passed back, the network instrument client can identify the link associated with
    /// the <c>device_intr_srq</c>, The network instrument protocol recognizes one type of interrupt,
    /// service request. Note that the return type to the interrupt RPC is void, denoting a one-way
    /// RPC.
    /// </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceEnableSrqParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceError"/>.
    /// </returns>
    public DeviceError DeviceEnableSrq( DeviceEnableSrqParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device local control. </summary>
    /// <remarks>
    /// To successfully complete a <c>device_local</c> RPC, a network instrument server SHALL: <para>
    /// 1. Place the associated device in a local state. </para><para>
    /// 2. Return with error set to zero, no error, to indicate successful completion. </para><para>
    /// Since not all devices directly support a local state, how this operation is executed depends
    /// upon the interface between the network instrument server and the device. </para><para>
    /// If the device does not support a local state and the network instrument server is able to
    /// detect this, <c>device_local</c> SHALL terminate and set error to 8, operation not supported. </para>
    /// <para>
    /// 
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match,
    /// <c>device_local</c> SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// 
    /// If some other link has the lock, <c>device_local</c> SHALL examine the <c>waitlock</c> flag in 
    /// <c>flags</c>. If the flag is set, <c>device_local</c> SHALL block until the lock is free. If the flag is not set,
    /// <c>device_local</c> SHALL terminate with error set to 11, device locked by another link. </para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_local</c> SHALL
    /// terminate with error set to 11, device locked by another link. </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, <c>device_local</c> SHALL
    /// terminate with error set to 15, I/O timeout. </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// place the device in the local state, <c>device_local</c> SHALL terminate with error set to 17, I/O
    /// error. </para><para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_local</c> SHALL terminate
    /// with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="request">  device generic parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public DeviceError DeviceLocal( DeviceGenericParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Enables device remote control. </summary>
    /// <remarks>
    /// Since not all devices directly support a remote state, how this operation is executed depends
    /// upon the interface between the network instrument server and the device. <para>
    /// 
    /// If the device does not support a remote state and the network instrument server is able to
    /// detect this,
    /// <c>device_remote</c> SHALL terminate and set error to 8, operation not supported. </para><para>
    /// 
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match, <c>
    /// device_remote</c> SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// 
    /// If some other link has the lock, <c>device_remote</c> SHALL examine the <c>waitlock</c> flag
    /// in <c>flags</c> . If the flag is set, <c>device_remote</c> SHALL block until the lock is
    /// free. If the flag is not set, <c>device_remote</c> SHALL terminate with error set to 11,
    /// device locked by another link.  </para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_remote</c>
    /// SHALL terminate with error set to 11, device locked by another link.  </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, <c>device_remote</c>
    /// SHALL terminate with error set to 15, I/O timeout.  </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// place the device in the remote state, <c>device_remote</c> SHALL terminate with error set to
    /// 17, I/O error. </para><para>
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_remote</c>
    /// SHALL terminate with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>   A Device_Error. </returns>
    public DeviceError DeviceRemote( DeviceGenericParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Returns the device status byte. </summary>
    /// <remarks>
    /// To successfully complete a <c>device_readstb</c> RPC, the network instrument server SHALL: <para>
    /// 1. Return in the <c>STB</c> parameter the device's status byte. </para><para>
    /// 2. Return with error set to 0, no error, to indicate successful completion. </para><para>
    /// 
    /// Since not all devices directly support a status byte, how this operation is executed and the
    /// semantics of the <c>STB</c> parameter depend upon the interface between the network
    /// instrument server and the device.  </para><para>
    /// 
    /// If a status byte cannot be returned, <c>device_readstb</c> SHALL terminate and set error to 8,
    /// operation not supported.  </para><para>
    /// 
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match,
    /// <c>device_readstb</c> SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// 
    /// If some other link has the lock, the procedure examines the <c>waitlock</c> flag in <c>flags</c>
    /// . If the flag is set, <c>device_readstb</c> blocks until the lock is free before retrieving
    /// the status byte. If the flag is not set, <c>device_readstb</c> SHALL terminate and set error
    /// to 11, device locked by another link.</para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_readstb</c>
    /// SHALL terminate with error set to 11, device locked by another link.</para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, <c>device_readstb</c>
    /// SHALL terminate with error set to 15, I/O timeout.</para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// read the data, <c>device_readstb</c> SHALL terminate with error set to 17.</para><para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_readstb</c>
    /// SHALL terminate with error set to 23.</para>
    /// </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>   A Device_ReadStbResp. </returns>
    public DeviceReadStbResp DeviceReadStb( DeviceGenericParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Performs a trigger. </summary>
    /// <remarks>
    /// If the device does not support a trigger and the network instrument server is able to detect
    /// this, <c>device_trigger</c> SHALL terminate and set error to 8, operation not supported. <para>
    /// 
    /// IEEE 488.1 and similar interfaces may not be able to detect that the device does not support
    /// a trigger. </para><para>
    /// The <c>link id</c> parameter is compared against the link identifiers. If none match,
    /// <c>device_trigger</c> SHALL terminate and set error to 4, invalid link identifier. </para><para>
    /// 
    /// If some other link has the lock, <c>device_trigger</c> SHALL examine the <c>waitlock</c> flag
    /// in <c>flags</c> .If the flag is set, <c>device_trigger</c> SHALL block until the lock is free
    /// before sending the trigger. If the flag is not set, <c>device_trigger</c> SHALL terminate and
    /// set error to 11, device locked by another link. </para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_trigger</c>
    /// SHALL terminate with error set to 11, device locked by another link. </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, <c>device_trigger</c>
    /// SHALL terminate with error set to 15, I/O timeout. </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while sending to
    /// trigger , <c>device_trigger</c> SHALL terminate with error set to 17, I/O error. </para><para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_trigger</c>
    /// SHALL terminate with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>   A Device_Error. </returns>
    public DeviceError DeviceTrigger( DeviceGenericParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Lock the device. </summary>
    /// <remarks>
    /// To successfully complete a <c>device_lock</c> RPC, a network instrument server SHALL: <para>
    /// 1. Acquire the device's lock. </para><para>
    /// 2. Return with error set to zero, no error, to indicate successful completion. </para><para>
    /// 
    /// If this link already has the lock, the network instrument server SHALL terminate with error set to 11,
    /// device locked by another link. </para><para>
    /// 
    /// Multiple network instrument servers on the same host need to communicate with one another to
    /// implement locking since locks are global to all network instrument servers in a given host. </para><para>
    /// 
    /// The <c>link id</c> parameter is compared against the active link identifiers . If none match, <c>device_lock</c> SHALL
    /// terminate, before trying to acquire the device's lock, with error set to 4, invalid link identifier. </para><para>
    /// 
    /// If some other link has the lock, <c>device_lock</c> SHALL examine the <c>waitlock</c> flag in <c>flags</c> . If the flag is set,
    /// <c>device_lock</c> SHALL block until the lock is free. If the flag is not set, <c>device_lock</c> SHALL terminate with
    /// error set to 11, device locked by another link. </para><para>
    /// 
    /// The network instrument server blocks if another link has the lock, but does not block if another link is
    /// performing an I/O operation so long as the lock is available. </para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_lock</c> SHALL terminate with error
    /// set to 11, device locked by another link. </para><para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_lock</c> SHALL terminate with
    /// error set to 23, abort. </para><para>
    /// 
    /// The locks SHALL be tied to the core connection between the network instrument client and the network
    /// instrument server.This means that if the network instrument server detects a broken connection, it
    /// SHALL release all of the connection's locks. </para>
    /// </remarks>
    /// <param name="request"> Device lock parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public DeviceError DeviceLock( DeviceLockParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   Unlock the device. </summary>
    /// <remarks>
    /// To successfully complete a <c>device_unlock</c>, a network instrument server SHALL: <para>
    /// 1. Release the lock. </para><para>
    /// 2. Return with error set to zero, no error, to indicate successful completion. </para><para>
    /// 
    /// The Device_Link (link identifier) parameter is compared against the active link identifiers . If none
    /// match, <c>device_unlock</c> SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// 
    /// If this link does not have the lock, <c>device_unlock</c> SHALL terminate with error set to 12, no lock held by
    /// this link. </para><para>
    /// 
    /// The operation of <c>device_unlock</c> SHALL NOT be affected by device_abort. </para>
    /// </remarks>
    /// <param name="deviceLink">   The device link parameters. </param>
    /// <returns>   A Device_Error. </returns>
    public DeviceError DeviceUnlock( DeviceLink deviceLink )
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Thread synchronization locks
    /// </summary>
    private readonly ManualResetEvent _asyncLocker = new( false );

    /// <summary>
    /// Read cache buffer
    /// </summary>
    private byte[] _readBuffer = Array.Empty<byte>();

    /// <summary>   Read a message. </summary>
    /// <remarks>
    /// To successfully complete a <c>device_read</c> RPC, a network instrument server SHALL: <para>
    /// 1. Transfer bytes into the data parameter until one of the following termination conditions
    /// are met: a.An END indicator is read.The END bit in reason SHALL be set. </para><para>
    /// 
    /// b.requestSize bytes are transferred.The REQCNT bit in reason SHALL be set. This termination
    /// condition SHALL be used if requestSize is zero.  </para><para>
    /// 
    /// c.termchrset is set in flags and a character which matches termChar is transferred.The CHR
    /// bit in reason SHALL be set. </para><para>
    /// 
    /// d.The buffer used to return the response is full.No bits in reason SHALL BE set.
    /// 2. Return with error set to 0, no error, to indicate successful completion.  </para><para>
    /// 
    /// If more than one termination condition is valid, reason contains the bitwise inclusive OR of
    /// all the reasons.  </para><para>
    /// 
    /// If reason is not set (value of 0) and error is zero, then the network instrument client could
    /// issue <c>device_read</c> calls until one of the other three termination conditions is
    /// encountered. </para>
    /// 
    /// <list type="bullet">Abort shall cause the following errors: <item>
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match,
    /// device_read SHALL terminate with error set to 4, invalid link identifier. </item><item>
    /// 
    /// If some other link has the lock, <c>device_read</c> SHALL examine the wait lock flag in <c>
    /// flags</c> . If the flag is set, <c>device_read</c> SHALL block until the lock is free before
    /// transferring data.If the flag is not set, <c>device_read</c> SHALL terminate with error set
    /// to 11, device locked by another link. </item><item>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_read</c>
    /// SHALL terminate with error set to 11, device locked by another device and data.data_len set
    /// to zero. </item><item>
    /// 
    /// If the transfer takes longer than <c>io_timeout</c> milliseconds, <c>device_read</c> SHALL
    /// terminate with error set to 15, I/O timeout, <c>data.data_len</c> set to however many bytes
    /// were transferred, and reason set to zero. </item><item>
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// read the data, <c>device_read</c> SHALL terminate with error set to 17, I/O error. </item><item>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_read</c>
    /// SHALL terminate with error set to 23, abort. </item><item>
    /// 
    /// The number of bytes transferred from the device into data SHALL be returned in data.data_len
    /// even when <c>device_read</c> terminates due to a timeout or <c>device_abort</c>. </item></list>
    /// </remarks>
    /// <param name="deviceReadParameters"> Device read parameters. </param>
    /// <returns>   A Device_ReadResp. </returns>
    public DeviceReadResp DeviceRead( DeviceReadParms deviceReadParameters )
    {
        DeviceReadResp readRes = new();
        if ( this.CurrentOperationType == Vxi11InstrumentOperationType.None || this.CurrentOperationType == Vxi11InstrumentOperationType.Write )
        {
            this._readBuffer = Array.Empty<byte>();
            _ = this._asyncLocker.Reset();
        }
        if ( !this._asyncLocker.WaitOne( this.WaitOnOutTime ) )
        {
            readRes.SetData( this._readBuffer );
            readRes.ErrorCode = DeviceErrorCode.IOTimeout; // timeout
            readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
            return readRes;
        }

        if ( this.CurrentOperationType == Vxi11InstrumentOperationType.Read )
        {
            readRes.SetData( this._readBuffer );
            readRes.ErrorCode = DeviceErrorCode.NoError;
            readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
        }
        this.CurrentOperationType = Vxi11InstrumentOperationType.None; //Reset the action type
        return readRes;
    }

    /// <summary>   Process the device write procedure. </summary>
    /// <remarks>
    /// To a successfully complete a <c>device_write</c>  RPC, the network instrument server SHALL: <para>
    /// 1. Transfer the contents of data to the device. </para><para>
    /// 2. Return in size parameter the number of bytes accepted by the device. </para><para>
    /// 3. Return with error set to 0, no error. </para><para>
    /// 
    /// If the end flag in <c>flags</c>  is set, then an END indicator SHALL be associated with the
    /// last byte in data. </para><para>
    /// 
    /// If a controller needs to send greater than maxRecvSize bytes to the device at one time, then
    /// the network instrument client makes multiple calls to <c>device_write</c>  to accomplish the
    /// complete transaction.A network instrument server accepts at least 1,024 bytes in a single <c>
    /// device_write</c>
    /// call due to RULE B.6.3.  </para><para>
    /// The value of data.data_len may be zero, in which case no device actions are performed.  </para>
    /// <para>
    /// 
    /// The <c>link id</c> parameter is compared to the active link identifiers. If none match, <c>
    /// device_write</c>
    /// SHALL terminate and set error to 4, invalid link identifier. </para><para>
    /// 
    /// If data.data_len is greater than the value of maxRecvSize returned in create_link,
    /// <c>device_write</c>  SHALL terminate without transferring any bytes to the device and SHALL
    /// set error
    /// to 5.Section B: Network Instrument Protocol Page 29 October 4, 2000 Printing VXIbus
    /// Specification: VXI-11 Revision 1.0 </para><para>
    /// 
    /// If some other link has the lock, <c>device_write</c>  SHALL examine the <c>waitlock</c> flag
    /// in <c>flags</c> . If the flag is set, <c>device_write</c>  SHALL block until the lock is
    /// free. If the flag is not set,
    /// <c>device_write</c>  SHALL terminate and set error to 11, device already locked by another
    /// link. </para>
    /// <para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_write</c>
    /// SHALL terminate with error set to 11, device already locked by another link. </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds not all of data has been transferred to the
    /// device,
    /// <c>device_write</c>  SHALL terminate with error set to 15, I/O timeout. This timeout is based
    /// on the
    /// entire transaction and not the time required to transfer single bytes. </para><para>
    /// 
    /// The <c>io_timeout</c> value set by the application may need to change based on the size of
    /// data. </para>
    /// <para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_write</c>
    /// SHALL terminate with error set to 23, abort. </para><para>
    /// 
    /// The number of bytes transferred to the device SHALL be returned in size, even when the call
    /// terminates due to a timeout or device_abort. </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// write the data, <c>device_write</c>  SHALL terminate with error set to 17, I/O error. </para>
    ///  <list type="bullet">Abort shall cause the following errors: <item>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_write</c>
    /// terminate with error set to 23, abort. </item><item>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// write the data, <c>device_write</c>  SHALL terminate with error set to 17, I/O error. </item><item>
    /// 
    /// </item></list>
    /// </remarks>
    /// <param name="deviceWriteParameters">    Device write parameters. </param>
    /// <returns>   A <c>device_write</c> Resp. </returns>
    public DeviceWriteResp DeviceWrite( DeviceWriteParms deviceWriteParameters )
    {
        // get the write command.
        string cmd = this.CharacterEncoding.GetString( deviceWriteParameters.GetData() );
        Logger.Writer.LogVerbose( $"link ID: {deviceWriteParameters.Link.LinkId} -> Received：{cmd}" );
        DeviceWriteResp result = new() {
            Size = deviceWriteParameters.GetData().Length
        };

        // holds one or more SCPI commands each with its arguments
        string[] scpiCommands = cmd.Split( new char[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries );

        if ( scpiCommands.Length == 0 )
        {
            // The instruction is incorrect or undefined
            result.ErrorCode = DeviceErrorCode.SyntaxError;
            return result;
        }

        // process all the SCPI commands
        for ( int n = 0; n < scpiCommands.Length; n++ )
        {
            string spciCommand = scpiCommands[n]; // select a complete SCPI command with optional arguments
            Logging.Logger.Writer.LogVerbose( $"Process the instruction： {spciCommand}" );
            string[] scpiArgs = Array.Empty<string>(); // Holds the SCPI command arguments

            // split the command to the core command and its arguments:
            string[] scpiCmdElements = scpiCommands[n].Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
            spciCommand = scpiCmdElements[0].Trim();

            _ = this._asyncLocker.Reset(); // Block threads
            this._readBuffer = Array.Empty<byte>();

            // check if we have a query message (read) or a write message:
            this.CurrentOperationType = spciCommand[^1] == '?' ? Vxi11InstrumentOperationType.Read : Vxi11InstrumentOperationType.Write;

            // get the command arguments
            if ( scpiCmdElements.Length >= 2 )
                scpiArgs = scpiCmdElements[1].Split( new char[] { '，' }, StringSplitOptions.RemoveEmptyEntries );

            // find the mock server method that corresponds to the SCPI command.
            MethodInfo? method = this.GetType().GetMethods().ToList().Find( p => {
                var att = p.GetCustomAttribute( typeof( Vxi11InstrumentOperationAttribute ) );
                if ( att == null || att is not Vxi11InstrumentOperationAttribute ) return false;
                Vxi11InstrumentOperationAttribute scpiAtt = ( Vxi11InstrumentOperationAttribute ) att;

                // return success if the command matches the method attribute
                return string.Equals( scpiAtt.Content, spciCommand, StringComparison.OrdinalIgnoreCase );
            } );

            if ( method is not null )
            {
                Vxi11InstrumentOperationAttribute scpiAtt = ( Vxi11InstrumentOperationAttribute ) method.GetCustomAttribute( typeof( Vxi11InstrumentOperationAttribute ) )!;
                try
                {
                    object? res = null;
                    switch ( scpiAtt.OperationType )
                    {
                        case Vxi11InstrumentOperationType.None:
                            Logger.Writer.LogMemberWarning( $"The attribute of method {method} is marked incorrectly as {scpiAtt.OperationType}。" );
                            break;
                        case Vxi11InstrumentOperationType.Write:
                            this.WriteMessage = scpiCommands[n];
                            // invoke the corresponding method
                            res = method.Invoke( this, scpiArgs );
                            result.ErrorCode = DeviceErrorCode.NoError;
                            break;
                        case Vxi11InstrumentOperationType.Read://Query instructions
                            this.WriteMessage = scpiCommands[n];
                            res = method.Invoke( this, scpiArgs );
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
                                result.ErrorCode = DeviceErrorCode.NoError;
                            }
                            break;
                    }
                }
                catch ( Exception ex )
                {
                    Logger.Writer.LogMemberError( $"An error occurred when the method was called：{method}", ex );
                    // Parameter error
                    result.ErrorCode = DeviceErrorCode.ParameterError;
                }
            }
            else
            {
                Logger.Writer.LogMemberWarning( $"No method found： {spciCommand}" );
                result.ErrorCode = DeviceErrorCode.SyntaxError; // The instruction is incorrect or undefined
                this.CurrentOperationType = Vxi11InstrumentOperationType.None;
            }
            _ = this._asyncLocker.Set();//Reset block
        }

        return result;
    }

    #endregion

}
