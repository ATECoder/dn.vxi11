using System.Reflection;
using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.EnumExtensions;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.Server;

/// <summary>   Implementation of the <see cref="IVxi11Instrument"/>. </summary>
/// <remarks>
/// This class implements a 'physical' instrument that is the end point for the <see cref="Client.Vxi11InstrumentClient"/>
/// Virtual Instrument. The remote procedure call initiated at the <see cref="Client.Vxi11Client"/>
/// side, passes to the instrument through a <see cref="Vxi11Device"/>, which links the <see cref="Vxi11Server"/>
/// and the 'physical' <see cref="Vxi11Instrument"/>.
/// 
/// Implementations of VXI-11 servers should inherit from the <see cref="Vxi11Instrument"/> and,
/// perhaps also, from the <see cref="Vxi11Device"/>.
/// 
/// Instrument classes inheriting from the <see cref="Vxi11Instrument"/> might override a few
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
public partial class Vxi11Instrument : IVxi11Instrument
{

    #region " construction and cleanup "

    /// <summary>   Constructor. </summary>
    /// <param name="identity"> (Optional) Device identification string. </param>
    public Vxi11Instrument( string identity = "INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434" )
    {
        this.MessageLog = new CircularList<(int LinkId, char IO, DateTimeOffset Timestamp, String Value)>( IOMessageCapacity );
                this.Identity = identity;
        this._identity = identity;
        this._readBuffer = Array.Empty<byte>();
        this.CharacterEncoding = CoreChannelClient.EncodingDefault;
        this._characterEncoding = CoreChannelClient.EncodingDefault;
        this._cancelSource = new();

        this.StandardEventStatusMask = Vxi11EnumExtensions.StandardEventsAll();
        this.ServiceRequestEventMask = Vxi11EnumExtensions.ServiceRequestsAll();
    }

    #endregion

    #region " instrument operations "

    /// <summary>   Gets or sets the service request event mask. </summary>
    /// <value> The service request event mask. </value>
    protected int ServiceRequestEventMask { get; set; }

    private ServiceRequests _serviceRequestStatus;
    /// <summary>   Gets or sets the service request status. </summary>
    /// <value> The service request status. </value>
    public ServiceRequests ServiceRequestStatus
    {
        get => this._serviceRequestStatus;
        set
        {
            if ( this.OnPropertyChanged( ref this._serviceRequestStatus, value ) )
            {
                if ( this.InterruptEnabled
                    && ( ( byte ) this._serviceRequestStatus & this.ServiceRequestEventMask ) != 0 )
                    // note that the interrupt handle should include the id of the current client.
                    this.OnRequestingService( new Vxi11EventArgs( this._interruptHandle ) );
            }
        } 
    }

    /// <summary>   Gets or sets the standard event status mask. </summary>
    /// <value> The standard event status mask. </value>
    protected byte StandardEventStatusMask { get; set; }

    /// <summary>   Gets or sets the standard event status. </summary>
    /// <value> The standard event status. </value>
    protected StandardEvents StandardEventStatus { get; set; }

    /// <summary>   Clears status: <see cref="Vxi11InstrumentCommands.CLS"/>. </summary>
    /// <remarks>
    /// Clear Status Command. Clears the event registers in all register groups. Also clears the
    /// error queue.
    /// </remarks>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.CLS, Vxi11InstrumentOperationType.Write )]
    public bool CLS()
    {
        this.ServiceRequestStatus = ServiceRequests.None;
        this.StandardEventStatus = StandardEvents.None;
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
    /// <param name="standardEventStatusMask">  The standard event status mask. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.ESE, Vxi11InstrumentOperationType.Write )]
    public bool ESE( byte standardEventStatusMask )
    {
        this.StandardEventStatus &= ~StandardEvents.OperationComplete;
        this.StandardEventStatusMask = standardEventStatusMask;
        return true;
    }

    /// <summary>
    /// Reads Standard Event Status: <see cref="Vxi11InstrumentCommands.ESERead"/>
    /// </summary>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.ESERead, Vxi11InstrumentOperationType.Read )]
    public string ESERead()
    {
        this.StandardEventStatus &= ~StandardEvents.OperationComplete;
        this.ServiceRequestStatus |= ServiceRequests.MessageAvailable;
        return (( byte) this.StandardEventStatus).ToString();
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
        this.ServiceRequestStatus |= ServiceRequests.MessageAvailable;
        return (( byte ) this.StandardEventStatus).ToString();
    }

    /// <summary>   Reads the device identity string: <see cref="Vxi11InstrumentCommands.IDNRead"/></summary>
    /// <returns>   A string. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.IDNRead, Vxi11InstrumentOperationType.Read )]
    public string IDNRead()
    {
        this.ServiceRequestStatus |= ServiceRequests.MessageAvailable;
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
        this.StandardEventStatus |= StandardEvents.OperationComplete;
        return true;
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
        this.ServiceRequestStatus |= ServiceRequests.MessageAvailable;
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
        this.ServiceRequestStatus = ServiceRequests.None;
        this.StandardEventStatus = StandardEvents.None;
        return true;
    }

    /// <summary>
    /// Enables the service request events: <see cref="Vxi11InstrumentCommands.SRE"/>.
    /// </summary>
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
    /// <param name="serviceRequestEventMask">  The service request event mask. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.SRE, Vxi11InstrumentOperationType.Write )]
    public bool SRE( int serviceRequestEventMask )
    {
        this.ServiceRequestEventMask = serviceRequestEventMask;
        return true;
    }

    /// <summary>   Reads the service request enabled status: <see cref="Vxi11InstrumentCommands.SRERead"/> </summary>
    /// <returns>   A string. </returns>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.SRERead, Vxi11InstrumentOperationType.Read )]
    public string SRERead()
    {
        this.ServiceRequestStatus |= ServiceRequests.MessageAvailable;
        return (( byte) this.ServiceRequestEventMask).ToString();
    }

    /// <summary>
    /// Read the status byte: *STB?
    /// </summary>
    [Vxi11InstrumentOperation( Vxi11InstrumentCommands.STBRead, Vxi11InstrumentOperationType.Read )]
    public string STBRead()
    {
        this.ServiceRequestStatus |= ServiceRequests.MessageAvailable;
        return (( byte ) this.ServiceRequestStatus).ToString();
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
        // TODO: Add functionality here
        return true;
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
        this.ServiceRequestStatus |= ServiceRequests.MessageAvailable;
        return "0";
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
        // TODO: wait for operations to complete.
        return true;
    }

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
    /// <returns>   The start long operation. </returns>
    public virtual async Task<DeviceErrorCode> StartLongOperationAsync( CancellationTokenSource cancelSource )
    {
        this._cancelSource = cancelSource;
        DeviceErrorCode result = DeviceErrorCode.NoError;
        _ = await Task<DeviceErrorCode>.Factory.StartNew( () => {
            result = this.RunLongOperation( cancelSource );
            return result;
        } )
              .ContinueWith(
            failedTask => {
                result = DeviceErrorCode.IOError;
                this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) );
                return result;
            }, TaskContinuationOptions.OnlyOnFaulted );
        return result;
    }


    private CancellationTokenSource _cancelSource;
    /// <summary>
    /// Override this method to execute a long operation.
    /// </summary>
    /// <param name="cancelSource"> The cancellation source that allows processing to be canceled. </param>
    protected virtual DeviceErrorCode RunLongOperation( CancellationTokenSource cancelSource ) { return DeviceErrorCode.NoError; }

    /// <summary>   Stops the long operation. </summary>
    /// <param name="cancelSource"> The cancel source. </param>
    public void StopLongOperation( CancellationTokenSource cancelSource )
    {
        cancelSource.Cancel();
    }

    /// <summary>   Attempts to stop long operation an int from the given int. </summary>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <exception cref="AggregateException">           Thrown when an Aggregate error condition
    ///                                                 occurs. </exception>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool TryStopLongOperation( int timeout = 100, int loopDelay = 5 )
    {
        List<Exception> exceptions = new();

        try
        {
            if ( this.LongOperationRunning )
                this.StopLongOperation( this._cancelSource );
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

    /// <summary>   Try stop long operation asynchronously. </summary>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    /// <returns>   A Task. </returns>
    public virtual async Task<bool> TryStopLongOperationAsync( int timeout = 100, int loopDelay = 5 )
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

    #region  " instrument operation members "

    private string _identity;
    /// <summary>
    /// Device identification string
    /// </summary>
    public string Identity
    {
        get => this._identity;
        set => _ = this.OnPropertyChanged( ref this._identity, value );
    }

    #endregion

    #region " Sending interrupts (service requests) to the clients "

    private bool _interruptEnabled;
    /// <summary>   Gets or sets a value indicating whether the interrupt is enabled. </summary>
    /// <value> True if interrupt enabled, false if not. </value>
    public bool InterruptEnabled
    {
        get => this._interruptEnabled;
        set => _ = this.SetProperty( ref this._interruptEnabled, value );
    }

    /// <summary>   Event queue for all listeners interested in <see cref="RequestingService"/> events. </summary>
    public event EventHandler<cc.isr.VXI11.Vxi11EventArgs>? RequestingService;

    /// <summary>   Override this method to handle the <see cref="RequestingService"/> VXI-11 event. </summary>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    protected virtual void OnRequestingService( Vxi11EventArgs e )
    {
        if ( this.InterruptEnabled && e is not null ) RequestingService?.Invoke( this, e );
    }

    private int _clientId;
    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    public int ClientId
    {
        get => this._clientId;
        set => _ = this.OnPropertyChanged( ref this._clientId, value );
    }

    /// <summary>   the Handle of the interrupt as received when getting 
    ///             the <see cref="Vxi11Server.DeviceEnableSrq(DeviceEnableSrqParms)"/> RPC. </summary>
    private byte[] _interruptHandle = new byte[40];

    /// <summary>   Sets interrupt handle. </summary>
    /// <param name="interruptHandle">  the Handle of the interrupt as received when getting the 
    ///                                 <see cref="Vxi11Server.DeviceEnableSrq(DeviceEnableSrqParms)"/>
    ///                                 RPC. </param>
    public void SetInterruptHandle( byte[] interruptHandle )
    {
        this._interruptHandle = interruptHandle;
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

    #region " RPC implementations "

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
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match,
    /// <c>device_abort</c> SHALL terminate with error set to 4 invalid link identifier.  </para><para>
    /// 
    /// The operation of <c>device_abort</c> SHALL NOT be affected by locking  </para>
    /// </remarks>
    /// <returns>   A DeviceError. </returns>
    public DeviceError Abort()
    {
        if ( this.LongOperationRunning )
        {
            try
            {
                return this.TryStopLongOperation()
                    ? new DeviceError()
                    : new DeviceError( DeviceErrorCode.IOError );
            }
            catch ( Exception )
            {
                return new DeviceError( DeviceErrorCode.IOError );
            }
        }
        return new DeviceError();
    }

    #endregion

    #region " I/O operations "

    /// <summary>   The current operation instruction type. </summary>
    /// <value> The type of the current operation. </value>
    public Vxi11InstrumentOperationType CurrentOperationType { get; set; } = Vxi11InstrumentOperationType.None;

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
        }
        else
        {
            if ( this.CurrentOperationType == Vxi11InstrumentOperationType.Read )
            {
                readRes.SetData( this._readBuffer );
                readRes.ErrorCode = DeviceErrorCode.NoError;
                readRes.Reason = DeviceReadReasons.RequestCountIndicator | DeviceReadReasons.TermCharIndicator;
            }
        }
        this.CurrentOperationType = Vxi11InstrumentOperationType.None; //Reset the action type
        this.ServiceRequestStatus |= readRes.ErrorCode == DeviceErrorCode.NoError
            ? ServiceRequests.MessageAvailable
            : ServiceRequests.ErrorAvailable;
        this.LastDeviceError = readRes.ErrorCode;
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
    /// set error to 5.Section B: Network Instrument Protocol Page 29 October 4, 2000 Printing VXIbus
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
    /// on the entire transaction and not the time required to transfer single bytes. </para><para>
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
    /// <param name="compoundScpiCommand">  The compound SCPI command, which might consist of
    ///                                     commands separated with ';' or new line. </param>
    /// <returns>   A DeviceErrorCode. </returns>
    public DeviceErrorCode DeviceWrite( string compoundScpiCommand )
    {
        if ( string.IsNullOrWhiteSpace( compoundScpiCommand ) ) return DeviceErrorCode.IOError;

        // holds one or more SCPI commands each with its arguments
        string[] scpiCommands = compoundScpiCommand.Split( new char[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries );

        if ( scpiCommands.Length == 0 ) return DeviceErrorCode.SyntaxError;

        DeviceErrorCode result = DeviceErrorCode.NoError;
        foreach ( string scpiCommand in scpiCommands )
        {
            Logging.Logger.Writer.LogVerbose( $"Processing '{scpiCommand}'" );
            try
            {
                _ = this._asyncLocker.Reset(); // Block threads
                result = this.ProcessScpiCommand( scpiCommand );

            }
            catch ( Exception ex )
            {
                Logging.Logger.Writer.LogError( $"failed processing '{scpiCommand}'", ex );
                result = DeviceErrorCode.IOError;
            }
            finally
            {
                _ = this._asyncLocker.Set(); //Reset block
            }

            if ( result != DeviceErrorCode.NoError ) { break; }
        }
        if ( result != DeviceErrorCode.NoError )
            this.ServiceRequestStatus |= ServiceRequests.ErrorAvailable;
        this.LastDeviceError = result;
        return result;
    }


    /// <summary>   Lists the instrument operations. </summary>
    private List<MethodInfo> _instrumentOperations = new ();

    /// <summary>   Enumerates the instrument operation methods. </summary>
    /// <returns>   A List{MethodInfo}; </returns>
    public List<MethodInfo> InstrumentOperations()
    {
        if ( this._instrumentOperations is null || this._instrumentOperations.Count == 0 )
        {
            this._instrumentOperations = this.GetType().GetMethods().ToList().Where( (o) =>
            {
                return o.GetCustomAttribute( typeof( Vxi11InstrumentOperationAttribute ) ) is Vxi11InstrumentOperationAttribute;
            } ).ToList();
        }
        return this._instrumentOperations;
    }

    /// <summary>   Searches for the first instrument operation. </summary>
    /// <param name="operationName">    Name of the operation. </param>
    /// <returns>   The found instrument operation. </returns>
    private MethodInfo? FindInstrumentOperation( string operationName )
    {
        return this.InstrumentOperations().Find( p => {

            var att = p.GetCustomAttribute( typeof( Vxi11InstrumentOperationAttribute ) );
            if ( att == null || att is not Vxi11InstrumentOperationAttribute ) return false;
            Vxi11InstrumentOperationAttribute scpiAtt = ( Vxi11InstrumentOperationAttribute ) att;

            // return success if the command matches the method attribute
            return String.Equals( scpiAtt.Content, operationName, StringComparison.OrdinalIgnoreCase );
        } );
    }

    /// <summary>   Process the SCPI command described by <paramref name="fullScpiCommand"/>. </summary>
    /// <param name="fullScpiCommand">  A SCPI command and command arguments. </param>
    /// <returns>   A DeviceErrorCode. </returns>
    protected virtual DeviceErrorCode ProcessScpiCommand( string fullScpiCommand )
    {

        string[] scpiArgs = Array.Empty<string>(); // Holds the SCPI command arguments

        // split the command to the core command and its arguments:
        string[] scpiCmdElements = fullScpiCommand.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
        string coreScpiCommand = scpiCmdElements[0].Trim();

        this._readBuffer = Array.Empty<byte>();

        // check if we have a query message (read) or a write message:
        this.CurrentOperationType = coreScpiCommand[^1] == '?' ? Vxi11InstrumentOperationType.Read : Vxi11InstrumentOperationType.Write;

        // get the command arguments
        if ( scpiCmdElements.Length >= 2 )
            scpiArgs = scpiCmdElements[1].Split( new char[] { '，' }, StringSplitOptions.RemoveEmptyEntries );

        // find the instrument method that corresponds to this command.
        var method = this.FindInstrumentOperation( coreScpiCommand );

        DeviceErrorCode result = DeviceErrorCode.NoError;
        if ( method is not null )
        {
            Vxi11InstrumentOperationAttribute scpiAtt = ( Vxi11InstrumentOperationAttribute ) method.GetCustomAttribute( typeof( Vxi11InstrumentOperationAttribute ) )!;
            if ( scpiAtt != null )
            {
                try
                {
                    object? res = null;
                    switch ( scpiAtt.OperationType )
                    {
                        case Vxi11InstrumentOperationType.None:
                            string message = $"The attribute of method {method} is marked incorrectly as {scpiAtt.OperationType}";
                            Logger.Writer.LogMemberWarning( message );
                            this.LogMessage( scpiAtt.OperationType, message );
                            break;
                        case Vxi11InstrumentOperationType.Write:
                            this.LogMessage( scpiAtt.OperationType, fullScpiCommand );
                            // invoke the corresponding method
                            res = method.Invoke( this, scpiArgs );
                            result = DeviceErrorCode.NoError;
                            break;
                        case Vxi11InstrumentOperationType.Read://Query instructions
                            this.LogMessage( scpiAtt.OperationType, fullScpiCommand );
                            res = method.Invoke( this, scpiArgs );
                            if ( res is not null )
                            {
                                this.LogMessage( scpiAtt.OperationType, res.ToString() );
                                this._readBuffer = this.CharacterEncoding.GetBytes( res.ToString()! );
                                Logger.Writer.LogVerbose( $"Query results： {res}。" );
                            }
                            else
                            {
                                this.LogMessage( scpiAtt.OperationType, "null" );
                                Logger.Writer.LogVerbose( "Query results：NULL。" );
                                result = DeviceErrorCode.NoError;
                            }
                            break;
                    }
                }
                catch ( Exception ex )
                {
                    string message = $"An error occurred when the method was called：{method}; {ex.Message}";
                    Logger.Writer.LogMemberError( $"An error occurred when the method was called：{method}", ex );
                    this.LogMessage( 'e', message );
                    // Parameter error
                    result = DeviceErrorCode.ParameterError;
                }
            }
            else
            {
                string message = $"Attribute not found for method '{method}' parsed from the command '{fullScpiCommand}'";
                Logger.Writer.LogMemberWarning( message );
                this.LogMessage( 'e', message );
                result = DeviceErrorCode.SyntaxError; // The instruction is incorrect or undefined
                this.CurrentOperationType = Vxi11InstrumentOperationType.None;
            }
        }
        else
        {
            string message = $"No method found to match the command '{fullScpiCommand}'";
            Logger.Writer.LogMemberWarning( message );
            this.LogMessage( 'e', message );
            result = DeviceErrorCode.SyntaxError; // The instruction is incorrect or undefined
            this.CurrentOperationType = Vxi11InstrumentOperationType.None;
        }
        this.LastDeviceError = result;
        return result;
    }

    #endregion

    #region " RPC operation members "

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
    /// <param name="operationType">    Type of the operation. </param>
    /// <param name="value">            The value. </param>
    private void LogMessage( Vxi11InstrumentOperationType operationType, string value )
    {
        this.LogMessage( operationType.ToString()[0], value );
    }

    /// <summary>   Logs a message. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="operationType">    Type of the operation. </param>
    /// <param name="value">            The value. </param>
    private void LogMessage( char operationType, string value )
    {
        this.MessageLog.Add( (this.ClientId, operationType, DateTimeOffset.Now, value) );
        this.MessageLogCount++;
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

    /// <summary>
    /// Thread synchronization locks
    /// </summary>
    private readonly ManualResetEvent _asyncLocker = new( false );

    /// <summary>
    /// Read cache buffer
    /// </summary>
    private byte[] _readBuffer = Array.Empty<byte>();

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
