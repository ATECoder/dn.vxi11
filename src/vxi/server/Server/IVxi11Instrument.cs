using System.ComponentModel;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Server;

/// <summary>
/// Interface for a base LXI instrument, which implements standard IEEE 488.2 commands.
/// </summary>
/// <remarks>
/// This interface defines the implementation for a 'physical' instrument that is the end point
/// for the instrument client Virtual Instrument. The remote procedure call initiated at the VXI-
/// 11 client side, passes to the instrument through a <see cref="Vxi11Device"/>, which links the
/// <see cref="Vxi11Server"/> and the 'physical'
/// <see cref="Vxi11Instrument"/>.
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
/// The VXI-11 device procedures are from the host perspective, i.e., a device write writes to
/// the 'physical' instrument (also called 'Network Instrument') and device read reads from the
/// instrument.
/// </remarks>
public interface IVxi11Instrument : INotifyPropertyChanged
{

    #region " device name "

    /// <summary>
    /// Gets or sets the device name, .e.g, INST0, gpib0,5, or usb0[...].
    /// </summary>
    /// <value> The device name. </value>
    string DeviceName { get; set; }

    /// <summary>   Query if this device has valid device name. </summary>
    /// <remarks> This is required for validating the device name when creating the link. </remarks>
    /// <returns>   True if valid device name, false if not. </returns>
    bool IsValidDeviceName();

    #endregion

    #region " members "

    /// <summary>   Gets or sets the read termination. </summary>
    /// <value> The read termination. </value>
    public byte ReadTermination { get; set; }

    /// <summary>   Gets or sets the connect timeout. </summary>
    /// <remarks>
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned integer because the timeout value is unlikely to exceed the maximum integer
    /// value.
    /// </remarks>
    /// <value> The connect timeout. </value>
    public int ConnectTimeout { get; set; }

    /// <summary>   Gets or sets the I/O timeout. </summary>
    /// <value> The I/O timeout. </value>
    public int IOTimeout { get; set; }

    /// <summary>   
    /// Gets or sets the timeout during the phase where data is sent within RPC calls, or data is
    /// received within RPC replies. The <see cref="TransmitTimeout"/> timeout must be greater than 0.
    /// </summary>
    /// <value> The Transmit timeout. </value>
    public int TransmitTimeout { get; set; }

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
    public int LockTimeout { get; set; }

    /// <summary>   Gets or sets the write termination. </summary>
    /// <value> The write termination. </value>
    public byte[] WriteTermination { get; set; }

    /// <summary>
    /// Gets or sets the encoding to use when serializing strings. If <see langcref="null" />, the
    /// system's default encoding is to be used.
    /// </summary>
    /// <value> The character encoding. </value>
    public Encoding CharacterEncoding { get; set; }

    /// <summary>   Gets or sets the maximum length of the receive. </summary>
    /// <value> The maximum length of the receive. </value>
    public int MaxReceiveLength { get; set; }

    #endregion

    #region " client device link management "

    /// <summary>   Gets the number of linked clients. </summary>
    /// <value> The number of linked clients. </value>
    int LinkedClientsCount { get; }

    /// <summary>   Query if <see cref="ServerClientsRegistry"/> contains an link <paramref name="linkId"/>. </summary>
    /// <param name="linkId">   Identifier for the link. </param>
    /// <returns>   True if link created, false if not. </returns>
    bool ContainsLink( int linkId );

    /// <summary>   Query if the client with the specified <paramref name="linkId"/> locked its instrument. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">   The link identifier. </param>
    /// <returns>   True if locked, false if not. </returns>
    bool IsLocked( int linkId );

    /// <summary>   Releases the lock for the client with the specified <paramref name="linkId"/>. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">   The link identifier. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool ReleaseLock( int linkId );

    /// <summary>   Adds a client to the client collection and makes it the active client. </summary>
    /// <remarks>   2023-02-13. </remarks>
    /// <param name="createLinkParameters"> The parameters defining the created link. </param>
    /// <param name="linkId">               Identifier for the link. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool AddClient( CreateLinkParms createLinkParameters, int linkId );

    /// <summary>   Removes the client described by linkId. </summary>
    /// <param name="linkId">   Identifier for the link. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool RemoveClient( int linkId );

    /// <summary>   Attempts to get an existing a client using the <paramref name="linkId"/>. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">   The link identifier. </param>
    /// <param name="client">   [out] The client. </param>
    /// <returns>   The client. </returns>
    bool TryGetClient( int linkId, out ServerClientInfo client );

    /// <summary>   Attempts to select active client an int from the given int. </summary>
    /// <remarks>   2023-02-21. </remarks>
    /// <param name="linkId">       Identifier for the link. </param>
    /// <param name="lockTimeout">  (Optional) The lock timeout. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool TrySelectActiveClient( int linkId, int? lockTimeout = null );

    /// <summary>   Attempts to select client. </summary>
    /// <remarks>
    /// 2023-02-09. <para>
    /// 
    /// If the active client has the lock, examine the <see cref="DeviceOperationFlags.WaitLock"/>
    /// flag in <paramref name="operationFlags"/>. If the flag is set, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// blocks until the lock is released. Otherwise, return <see langword="false"/>, that is
    /// terminate that calling call and set error to <see cref="DeviceErrorCode.DeviceLockedByAnotherLink"/>
    /// (11).
    /// </para>
    /// </remarks>
    /// <param name="linkId">           Identifier for the link. </param>
    /// <param name="operationFlags">   The operation flags. </param>
    /// <param name="lockTimeout">      (Optional) The lock timeout. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool TrySelectClient( int linkId, DeviceOperationFlags operationFlags, int? lockTimeout = null );

    /// <summary>   Attempts to select client. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">       Identifier for the link. </param>
    /// <param name="waitLock">     Set <see langword="true"/> to wait for an existing lock;
    ///                             otherwise, return <see langword="false"/> if the active client is
    ///                             locked. </param>
    /// <param name="lockTimeout">  (Optional) The lock timeout. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool TrySelectClient( int linkId, bool waitLock, int? lockTimeout = null );

    /// <summary>   Gets or sets the <see cref="ServerClientInfo"/> of the active client. </summary>
    /// <value> Information describing the server client. </value>
    ServerClientInfo? ActiveServerClient { get; set; }

    /// <summary>   Query if 'linkId' is active client link identifier. </summary>
    /// <param name="linkId">   Identifier for the link. </param>
    /// <returns>   True if active client link identifier, false if not. </returns>
    bool IsActiveLinkId( int linkId );

    /// <summary>   Query if 'clientId' is active client identifier. </summary>
    /// <param name="clientId"> Identifier for the client. </param>
    /// <returns>   True if active client identifier, false if not. </returns>
    bool IsActiveClientId( int clientId );

    /// <summary>   Query if 'clinetId' is client linked. </summary>
    /// <remarks>   2023-02-21. </remarks>
    /// <param name="clientId"> Identifier for the client. </param>
    /// <returns>   True if client linked, false if not. </returns>
    bool IsClientLinked( int clientId );

    /// <summary>
    /// Gets a value indicating whether a valid link exists between the VXI-11 client
    /// and the <see cref="Vxi11Server"/>.
    /// </summary>
    /// <param name="clientId"> Identifier for the client. </param>
    /// <returns>
    /// True if a valid device link exists between the VXI-11 client
    /// and <see cref="Vxi11Server"/>.
    /// </returns>
    bool DeviceLinked( int clientId );

    /// <summary>   Determines if we can device locked. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool DeviceLocked();

    /// <summary>   Await lock release asynchronously. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="timeout">  The timeout to wait for the release of the lock. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool AwaitLockReleaseAsync( int timeout );

    /// <summary>   Await lock release. </summary>
    /// <remarks>   2023-02-21. </remarks>
    /// <param name="waitLock"> Set <see langword="true"/> to wait for an existing lock;
    ///                         otherwise, return <see langword="false"/> if the active client is
    ///                         locked. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    bool AwaitLockRelease( bool waitLock );

    #endregion

    #region " instrument operations "

    /// <summary>   Gets or sets the service request status. </summary>
    /// <value> The service request status. </value>
    ServiceRequests ServiceRequestStatus { get; set; }

    /// <summary>   Clears status: *CLS. </summary>
    /// <remarks>
    /// Clear Status Command. Clears the event registers in all register groups. Also clears the
    /// error queue.
    /// </remarks>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool CLS();

    /// <summary>   Enables Standard Event Status: *ESE. </summary>
    /// <remarks>
    /// Enables bits in the enable register for the Standard Event Register group. The selected bits
    /// are then reported to bit 5 of the Status Byte Register. Accepts the decimal sum of the bits
    /// in the register; default 0. For example, to enable bit 2 (value 4), bit 3 (value 8), and bit
    /// 7 (value 128), the decimal sum would be 140 (4 + 8 + 128). For example, *ESE 48 enables bit 4
    /// (value 16) and bit 5 (value 32) in the enable register.
    /// </remarks>
    /// <param name="standardEventStatusMask">  The standard event status mask. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool ESE( byte standardEventStatusMask );

    /// <summary>   Reads Standard Event Status: *ESE? </summary>
    /// <returns>   A string. </returns>
    string ESERead();

    /// <summary>   Standard Event Status Register Query: *ESR. </summary>
    /// <remarks>
    /// Queries the event register for the Standard Event Register group. Register is read-only; bits
    /// not cleared when read. Any or all conditions can be reported to the Standard Event summary
    /// bit through the enable register.To set the enable register mask, write a decimal value to the
    /// register using *ESE. Once a bit is set, it remains set until cleared by this query or *CLS.
    /// </remarks>
    /// <returns>   A string. </returns>
    string ESRRead();

    /// <summary>   Reads the instrument identity string: *IDN? </summary>
    /// <returns>   A string. </returns>
    string IDNRead();

    /// <summary>   Operation completion instruction: *OPC. </summary>
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
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool OPC();

    /// <summary>   Reads the operation completion status: *OPC? </summary>
    /// <remarks>
    /// Returns 1 to the output buffer after all pending commands complete. :
    /// The purpose of this command is to synchronize your application with the instrument. Other
    /// commands cannot be executed until this command completes. The difference between *OPC and
    /// *OPC? is that *OPC? returns "1" to the output buffer when the current operation
    /// completes. This means that no further commands can be sent after an *OPC? until it has
    /// responded. In this way an explicit polling loop can be avoided.That is, the IO driver will
    /// wait for the response.
    /// </remarks>
    /// <returns>   Returns 1 when all previous commands complete. </returns>
    string OPCRead();

    /// <summary>   Resets the instrument: *RST. </summary>
    /// <remarks>
    /// Resets instrument to factory default state, independent of MEMory:STATe:RECall:AUTO setting.
    /// Does not affect stored instrument states, stored arbitrary waveforms, or I/O settings; these
    /// are stored in non-volatile memory. Aborts a sweep or burst in progress.
    /// </remarks>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool RST();

    /// <summary>   Enables the service request events: *SER. </summary>
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
    /// cycles.
    /// Status Byte enable register is not cleared by *RST.
    /// </remarks>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool SRE( int serviceRequestEventMask );

    /// <summary>   Reads the service request enabled status: *SER? </summary>
    /// <returns>   A string. </returns>
    string SRERead();

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="RequestingService"/> event was raised 
    /// awaiting for the client to read the <see cref="ServiceRequestStatus"/> byte at which point
    /// the <see cref="ServiceRequests.RequestingService"/> bit is turned on and this value
    /// is set to <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="RequestingServiceEventRaised"/> is set <see langword="true"/> upon
    /// initiating the <see cref="RequestingService"/> event. The value is reset when the status byte
    /// is read by the client thus acknowledging that the service request was handled.
    /// </remarks>
    /// <value> True if status byte signaling is active, false if not. </value>
    bool RequestingServiceEventRaised { get; set; }

    /// <summary>   Reads status byte. </summary>
    /// <remarks>   2023-02-10. </remarks>
    /// <returns>   The status byte. </returns>
    byte ReadStatusByte();

    /// <summary>   Reads the status byte: *STB? </summary>
    /// <remarks>
    /// This command queries the condition register for the Status Byte Register group. For example
    /// *STB?: +40 indicates that condition register bits 3 and 5 are set. Similar to a Serial Poll,
    /// but processed like any other instrument command .Register is read-only; bits not cleared when
    /// read. Returns same result as a Serial Poll, but "Master Summary" bit (bit 6) is not cleared
    /// by *STB?. Power cycle or *RST clears all bits in condition register.
    /// </remarks>
    /// <returns>
    /// Returns a decimal value that corresponds to the binary-weighted sum of all bits set in the
    /// register. For example, with bit 3 (value 8) and bit 5 (value 32) set( and corresponding bits
    /// enabled ), the query returns +40.
    /// </returns>
    string STBRead();

    /// <summary>   Trigger command: *TRG. </summary>
    /// <remarks>
    /// Triggers a sweep, burst, arbitrary waveform advance, or LIST advance from
    /// the remote interface if the bus (software) trigger source is currently selected
    /// (TRIGger[1|2]:SOURce BUS).
    /// </remarks>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool TRG();

    /// <summary>   Runs a self test and reads its status: *TST?. </summary>
    /// <remarks>
    /// Self-Test Query. Performs a complete instrument self-test. If test fails, one or more error
    /// messages will provide additional information. Use SYSTem:ERRor? to read error queue.
    /// </remarks>
    /// <returns>   Returns +0 (pass) or +1 (one or more tests failed). </returns>
    string TSTRead();

    /// <summary>   Wait until all pending operations complete. *WAI. </summary>
    /// <remarks>
    /// Configures the instrument to wait for all pending operations to complete before executing any
    /// additional commands over the interface. For example, you can use this with the *TRG command
    /// to ensure that the instrument is ready for a trigger:
    /// *TRG;*WAI;*TRG.
    /// </remarks>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool WAI();

    #endregion

    #region " thread exception handler "

    /// <summary>
    /// Event queue for all listeners interested in ThreadExceptionOccurred events.
    /// </summary>
    public event ThreadExceptionEventHandler? ThreadExceptionOccurred;

    #endregion

    #region " run long operation in the background "

    /// <summary>   Gets or sets a value indicating whether the long operation running. </summary>
    /// <value> True if long operation running, false if not. </value>
    bool LongOperationRunning { get; set; }

    /// <summary>   Starts long operation asynchronously. </summary>
    /// <remarks> The task return <see cref="DeviceErrorCode.Abort"/> if the task is aborted using 
    /// the <paramref name="cancelSource"/> </remarks>
    /// <param name="cancelSource"> The cancellation source that allows processing to be canceled. </param>
    /// <returns>   A Task{DeviceErrorCode}. </returns>
    Task<DeviceErrorCode> StartLongOperationAsync( CancellationTokenSource cancelSource );

    /// <summary>   Stops the long operation. </summary>
    /// <param name="cancelSource"> The cancel source. </param>
    void StopLongOperation( CancellationTokenSource cancelSource );

    /// <summary>   Attempts to stop long operation an int from the given int. </summary>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <exception cref="AggregateException">           Thrown when an Aggregate error condition
    ///                                                 occurs. </exception>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool TryStopLongOperation( int timeout = 100, int loopDelay = 5 );

    /// <summary>   Try stop long operation asynchronously. </summary>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    /// <returns>   A Task. </returns>
    Task<bool> TryStopLongOperationAsync( int timeout = 100, int loopDelay = 5 );

    #endregion

    #region " instrument operation members "

    /// <summary>   Gets the identity. </summary>
    /// <value> The identity. </value>
    string Identity { get; set; }

    /// <summary>   Gets information describing the identity. </summary>
    /// <value> The identity parser. </value>
    public IdentityParser IdentityParser { get; }

    #endregion

    #region " sending interrupts (service requests) to the clients "

    /// <summary>   Gets or sets a value indicating whether the interrupt is enabled. </summary>
    /// <value> True if interrupt enabled, false if not. </value>
    bool InterruptEnabled { get; }

    /// <summary>   Enables or disables the interrupt. </summary>
    /// <param name="enable">   True to enable, false to disable. </param>
    /// <param name="handle">   The handle. </param>
    void EnableInterrupt( bool enable, byte[] handle );

    /// <summary>   Enables or disables the interrupt for the client referenced by the <paramref name="linkId"/>. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="linkId">   The link identifier. </param>
    /// <param name="enable">   True to enable, false to disable. </param>
    /// <param name="handle">   The handle. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    bool EnableInterrupt( int linkId, bool enable, byte[] handle );

    /// <summary>   Event queue for all listeners interested in <see cref="RequestingService"/> events. </summary>
    public event EventHandler<cc.isr.VXI11.Vxi11EventArgs>? RequestingService;

    #endregion

    #region " instrument state "

    /// <summary>   Initializes the instrument. </summary>
    /// <remarks>
    /// override this method when sub-classing this class to initialize values such as the identity.
    /// </remarks>
    public void Initialize();

    /// <summary>   Gets or sets a value indicating whether lock is requested on the device. </summary>
    /// <value> True if lock enabled, false if not. </value>
    public bool LockEnabled { get; set; }

    /// <summary>   Gets or sets a value indicating whether the remote is enabled. </summary>
    /// <value> True if remote enabled, false if not. </value>
    bool RemoteEnabled { get; set; }

    #endregion

    #region " rpc implementations "

    /// <summary>   Aborts and returns the <see cref="DeviceErrorCode"/>. </summary>
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
    DeviceErrorCode Abort();

    #endregion

    #region " i/o operations "

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
    DeviceReadResp DeviceRead( DeviceReadParms deviceReadParameters );

    /// <summary>   Process the device write procedure. </summary>
    /// <remarks>
    /// To a successfully complete a <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  RPC, the network instrument server SHALL: <para>
    /// 1. Transfer the contents of data to the device. </para><para>
    /// 2. Return in size parameter the number of bytes accepted by the device. </para><para>
    /// 3. Return with error set to 0, no error. </para><para>
    /// 
    /// If the end flag in <c>flags</c>  is set, then an END indicator SHALL be associated with the
    /// last byte in data. </para><para>
    /// 
    /// If a controller needs to send greater than maxRecvSize bytes to the device at one time, then
    /// the network instrument client makes multiple calls to <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  to accomplish the
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
    /// <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate without transferring any bytes to the device and SHALL
    /// set error to 5. </para><para>
    /// 
    /// If some other link has the lock, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL examine the <see cref="DeviceOperationFlags.WaitLock"/> flag
    /// in <c>flags</c> . If the flag is set, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL block until the lock is
    /// free. If the flag is not set,
    /// <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate and set error to 11, device already locked by another
    /// link. </para>
    /// <para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// SHALL terminate with error set to <see cref="DeviceErrorCode.DeviceLockedByAnotherLink"/>(11) . </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds not all of data has been transferred to the
    /// device,
    /// <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate with error set to 15, I/O timeout. This timeout is based
    /// on the entire transaction and not the time required to transfer single bytes. </para><para>
    /// 
    /// The <c>io_timeout</c> value set by the application may need to change based on the size of
    /// data. </para>
    /// <para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// SHALL terminate with error set to 23, abort. </para><para>
    /// 
    /// The number of bytes transferred to the device SHALL be returned in size, even when the call
    /// terminates due to a timeout or device_abort. </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// write the data, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate with error set to 17, I/O error. </para>
    ///  <list type="bullet">Abort shall cause the following errors: <item>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// terminate with error set to 23, abort. </item><item>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// write the data, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate with error set to 17, I/O error. </item><item>
    /// 
    /// </item></list>
    /// </remarks>
    /// <param name="compoundScpiCommand">  The compound SCPI command, which might consist of
    ///                                     commands separated with ';' or new line. </param>
    /// <returns>   A DeviceErrorCode. </returns>
    DeviceErrorCode DeviceWrite( string compoundScpiCommand );

    /// <summary>   Process the device write procedure. </summary>
    /// <remarks>
    /// To a successfully complete a <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  RPC,
    /// the network instrument server SHALL: <para>
    /// 1. Transfer the contents of data to the device. </para><para>
    /// 2. Return in size parameter the number of bytes accepted by the device. </para><para>
    /// 3. Return with error set to 0, no error. </para><para>
    /// 
    /// If the end flag in <c>flags</c>  is set, then an END indicator SHALL be associated with the
    /// last byte in data. </para><para>
    /// 
    /// If a controller needs to send greater than maxRecvSize bytes to the device at one time, then
    /// the network instrument client makes multiple calls to <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// to accomplish the complete transaction.A network instrument server accepts at least 1,024
    /// bytes in a single <c>
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
    /// <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate without transferring
    /// any bytes to the device and SHALL
    /// set error to 5. </para><para>
    /// 
    /// If some other link has the lock, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// SHALL examine the <see cref="DeviceOperationFlags.WaitLock"/> flag in <c>flags</c> . If the
    /// flag is set, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL block until the
    /// lock is free. If the flag is not set,
    /// <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate and set error to 11,
    /// device already locked by another
    /// link. </para>
    /// <para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// SHALL terminate with error set to <see cref="DeviceErrorCode.DeviceLockedByAnotherLink"/>(11)
    /// . </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds not all of data has been transferred to the
    /// device,
    /// <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate with error set to 15,
    /// I/O timeout. This timeout is based
    /// on the entire transaction and not the time required to transfer single bytes. </para><para>
    /// 
    /// The <c>io_timeout</c> value set by the application may need to change based on the size of
    /// data. </para>
    /// <para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// SHALL terminate with error set to 23, abort. </para><para>
    /// 
    /// The number of bytes transferred to the device SHALL be returned in size, even when the call
    /// terminates due to a timeout or device_abort. </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// write the data, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate with
    /// error set to 17, I/O error. </para>
    ///  <list type="bullet">Abort shall cause the following errors: <item>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// terminate with error set to 23, abort. </item><item>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// write the data, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate with
    /// error set to 17, I/O error. </item><item>
    /// 
    /// </item></list>
    /// </remarks>
    /// <param name="data"> The data. </param>
    /// <returns>   A DeviceErrorCode. </returns>
    DeviceErrorCode DeviceWrite( byte[] data );

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
    /// If some other link has the lock, <c>device_trigger</c> SHALL examine the <see cref="DeviceOperationFlags.WaitLock"/> flag
    /// in <c>flags</c> .If the flag is set, <c>device_trigger</c> SHALL block until the lock is free
    /// before sending the trigger. If the flag is not set, <c>device_trigger</c> SHALL terminate and
    /// set error to 11, device locked by another link. </para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_trigger</c>
    /// SHALL terminate with error set to 11, device locked by another link. </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, <c>
    /// device_trigger</c>
    /// SHALL terminate with error set to 15, I/O timeout. </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while sending to
    /// trigger , <c>device_trigger</c> SHALL terminate with error set to 17, I/O error. </para><para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_trigger</c>
    /// SHALL terminate with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="flags">        The flags. </param>
    /// <param name="ioTimeout">    The i/o timeout. </param>
    /// <returns>
    /// A response of type <see cref="DeviceErrorCode"/> to send to the remote procedure call.
    /// </returns>
    DeviceErrorCode DeviceTrigger( DeviceOperationFlags flags, int ioTimeout );


    /// <summary>   Device clear. </summary>
    /// <remarks>
    /// Since not all devices directly support a clear operation, how this operation is executed
    /// depends upon the interface between the network instrument server and the device. <para>
    /// If the device does not support a clear operation and the network instrument server is able to
    /// detect this, device_clear SHALL terminate and set error to 8, operation not supported. </para>
    /// <para>
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match,
    /// device_clear SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// If some other link has the lock, device_clear SHALL examine the <see cref="DeviceOperationFlags.WaitLock"/> flag in <c>
    /// flags</c> . If the flag is set, device_clear SHALL block until the lock is free. If the flag
    /// is not set, device_clear SHALL terminate with error set to 11, device locked by another link.
    /// </para><para>
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, device_clear SHALL
    /// terminate with error set to 11, device locked by another device. </para><para>
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, device_clear
    /// SHALL terminate with error set to 15, I/O timeout. </para><para>
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// clear the device, device_clear SHALL terminate with error set to 17, I/O error. </para><para>
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, device_clear SHALL
    /// terminate with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="flags">        The flags. </param>
    /// <param name="ioTimeout">    The i/o timeout. </param>
    /// <returns>
    /// A response of type <see cref="DeviceErrorCode"/> to send to the remote procedure call.
    /// </returns>
    DeviceErrorCode DeviceClear( DeviceOperationFlags flags, int ioTimeout );

    /// <summary>
    /// The device_remote RPC is used to place a device in a remote state wherein all programmable
    /// local controls are disabled.
    /// </summary>
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
    /// If some other link has the lock, <c>device_remote</c> SHALL examine the <see cref="DeviceOperationFlags.WaitLock"/> flag
    /// in <c>flags</c> . If the flag is set, <c>device_remote</c> SHALL block until the lock is
    /// free. If the flag is not set, <c>device_remote</c> SHALL terminate with error set to 11,
    /// device locked by another link.  </para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_remote</c>
    /// SHALL terminate with error set to 11, device locked by another link.  </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, <c>
    /// device_remote</c>
    /// SHALL terminate with error set to 15, I/O timeout.  </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// place the device in the remote state, <c>device_remote</c> SHALL terminate with error set to
    /// 17, I/O error. </para><para>
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_remote</c>
    /// SHALL terminate with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="flags">        The flags. </param>
    /// <param name="ioTimeout">    The i/o timeout. </param>
    /// <returns>
    /// A response of type <see cref="DeviceErrorCode"/> to send to the remote procedure call.
    /// </returns>
    DeviceErrorCode DeviceRemote( DeviceOperationFlags flags, int ioTimeout );

    /// <summary>
    /// The device_local RPC is used to place a device in a local state wherein all programmable
    /// local controls are enabled.
    /// </summary>
    /// <remarks>
    /// To successfully complete a <c>device_local</c> RPC, a network instrument server SHALL: <para>
    /// 1. Place the associated device in a local state. </para><para>
    /// 2. Return with error set to zero, no error, to indicate successful completion. </para><para>
    /// 
    /// Since not all devices directly support a local state, how this operation is executed depends
    /// upon the interface between the network instrument server and the device. </para><para>
    /// If the device does not support a local state and the network instrument server is able to
    /// detect this, <c>device_local</c> SHALL terminate and set error to 8, operation not supported.
    /// </para>
    /// <para>
    /// 
    /// The <c>link id</c> parameter is compared against the active link identifiers. If none match,
    /// <c>device_local</c> SHALL terminate with error set to 4, invalid link identifier. </para><para>
    /// 
    /// If some other link has the lock, <c>device_local</c> SHALL examine the <see cref="DeviceOperationFlags.WaitLock"/> flag in
    /// <c>flags</c>. If the flag is set, <c>device_local</c> SHALL block until the lock is free. If
    /// the flag is not set, <c>device_local</c> SHALL terminate with error set to 11, device locked
    /// by another link. </para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <c>device_local</c>
    /// SHALL terminate with error set to 11, device locked by another link. </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds the operation is not complete, <c>
    /// device_local</c> SHALL terminate with error set to 15, I/O timeout. </para><para>
    /// 
    /// If the network instrument server encounters a device specific I/O error while attempting to
    /// place the device in the local state, <c>device_local</c> SHALL terminate with error set to 17,
    /// I/O error. </para><para>
    /// 
    /// If the asynchronous <c>device_abort</c> RPC is called during execution, <c>device_local</c>
    /// SHALL terminate with error set to 23, abort. </para>
    /// </remarks>
    /// <param name="flags">        The flags. </param>
    /// <param name="ioTimeout">    The i/o timeout. </param>
    /// <returns>
    /// A response of type <see cref="DeviceErrorCode"/> to send to the remote procedure call.
    /// </returns>
    DeviceErrorCode DeviceLocal( DeviceOperationFlags flags, int ioTimeout );

    #endregion

    #region " rpc operation members "

    /// <summary>   Gets or sets the last device error. </summary>
    /// <value> The las <see cref="DeviceErrorCode"/> . </value>
    DeviceErrorCode LastDeviceError { get; set; }

    /// <summary>   Gets a <see cref="CircularList{T}"/> of (<see cref="DateTime"/> Timestamp, <see cref="String"/> Value)
    /// of the last messages that were sent to and received from the instrument. </summary>
    /// <value> The list of message tuples consisting of the client id, IO (R for read and W for write), 
    /// a timestamp and a value that were sent to or received from the instrument. </value>
    List<(int ClientId, char IO, DateTimeOffset Timestamp, String Value)> MessageLog { get; }

    /// <summary>   Gets or sets the number of I/O messages. </summary>
    /// <value> The number of I/O messages, which, in fact, flags the property change flag that can be used to 
    /// indicate the availability of new messages. </value>
    int MessageLogCount { get; set; }

    #endregion
}
