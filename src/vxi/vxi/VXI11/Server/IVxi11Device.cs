using System.Net;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Server;

/// <summary>   An interface that is required for the implementation of a VXI-11 Device
/// on a <see cref="Vxi11SingleClientServer"/>. </summary>
public interface IVxi11Device
{

    #region " Abort server "

    /// <summary>   Gets or sets the abort port number. </summary>
    /// <value> The abortPort number. </value>
    int AbortPortNumber { get; set; }

    /// <summary>   Handles abort request. </summary>
    /// <param name="e">        Device error event information. </param>
    void HandleAbortRequest( DeviceErrorEventArgs e );

    #endregion

    #region " Interrupt port and client "

    /// <summary>   Gets or sets a value indicating whether the interrupt is enabled. </summary>
    /// <value> True if interrupt enabled, false if not. </value>
    bool InterruptEnabled { get; set; }

    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    int ClientId { get; set; }

    /// <summary>   Event queue for all listeners interested in <see cref="RequestingService"/> events. </summary>
    event EventHandler<Vxi11EventArgs> RequestingService;

    #endregion

    #region " I/O messages "

    /// <summary>   Gets or sets a message that was sent to the device. </summary>
    /// <value> The message that was sent to the device. </value>
    string WriteMessage { get; set; }

    /// <summary>   Gets or sets a message that was received from the device. </summary>
    /// <value> A message that was received from the device. </value>
    string ReadMessage { get; set; }

    /// <summary>   Gets or sets the last device error. </summary>
    /// <value> The las <see cref="DeviceErrorCode"/> . </value>
    DeviceErrorCode LastDeviceError { get; set; }

    #endregion

    #region " members "

    /// <summary>   Gets or sets the host IPv4 Address. </summary>
    /// <value> The host. </value>
    public string Host { get; set; }

    /// <summary>   Gets the IP address. </summary>
    /// <value> The IP address. </value>
    public IPAddress IPAddress { get; }

    /// <summary>
    /// Gets or sets the interface device string, .e.g, inst0, gpib0,5, or usb0[...].
    /// </summary>
    /// <value> The interface device string. </value>
    public string InterfaceDeviceString { get; set; }

    /// <summary>   Query if this device has valid interface device string. </summary>
    /// <remarks> This is required for validating the interface device string when creating the link. </remarks>
    /// <returns>   True if valid interface device string, false if not. </returns>
    public bool IsValidInterfaceDeviceString();

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
    public bool Eoi { get; set; }

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

    /// <summary>   Gets a value indicating whether the VXI Core Client is connected. </summary>
    /// <value> True if connected, false if not. </value>
    public bool Connected { get; }

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

    #region " Device state "

    /// <summary>   Gets or sets a value indicating whether lock is requested on the device. </summary>
    /// <value> True if lock enabled, false if not. </value>
    public bool LockEnabled { get; set; }

    /// <summary>   Gets or sets a value indicating whether the remote is enabled. </summary>
    /// <value> True if remote enabled, false if not. </value>
    bool RemoteEnabled { get; set; }

    #endregion

    #region " LXI-11 ONC/RPC Calls "

    /// <summary>   Gets or sets the device link to the actual single device. </summary>
    /// <value> The device link. </value>
    DeviceLink? DeviceLink { get; set; }

    /// <summary>
    /// Query if the server can create a new device link given that this is a
    /// single device server.
    /// </summary>
    /// <remarks>   2023-01-28. </remarks>
    /// <returns>   True if device free, false if not. </returns>
    bool CanCreateNewDeviceLink();

    /// <summary>   Create a device connection; Opens a link to a device. </summary>
    /// <remarks>
    /// To successfully complete a create_link RPC, a network instrument server SHALL: <para>
    /// 1. If lockDevice is set to true, acquire the lock for the device. </para><para>
    /// 2. Return in <c>link id</c> a link identifier to be used with future calls. The value of <c>link id</c> SHALL be
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
    /// If lockDevice is true and the lock is not freed after at least <c>lock_timeout</c> milliseconds,
    /// create_link SHALL terminate without creating a link and return with error set to 11, device
    /// locked by another link.Page 26 Section B: Network Instrument Protocol October 4, 2000
    /// Printing VXIbus Specification: VXI-11 Revision 1.0 </para><para>
    /// 
    /// The execution of create_link SHALL have no effect on the state of any device associated with
    /// the network instrument server. </para><para>
    /// 
    /// A create_link RPC cannot be aborted since a valid link identifier is not yet available.A
    /// network instrument client should set <c>lock_timeout</c> to a reasonable value to avoid locking up
    /// the server. </para>
    /// </remarks>
    /// <param name="request">  The request of type <see cref="CreateLinkParms"/> to use with
    ///                         the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceError"/>.
    /// </returns>
    CreateLinkResp CreateLink( CreateLinkParms request );

    /// <summary>   Destroy a connection. </summary>
    /// <remarks>
    /// To successfully complete a destroy_link RPC, a network instrument server SHALL: <para>
    /// 1. Deactivate the link identifier and recover any resources associated with the link. </para><para>
    /// 2. If this link has the lock, free the lock (see <c>device_lock</c> and create_link). </para><para>
    /// 3. Disable this link from using the interrupt mechanism( see <c>device_enable_srq</c> ). </para><para>
    /// 4. Return with error set to 0, no error, to indicate successful completion. </para><para>
    /// The Device_Link( link identifier ) parameter is compared against the active link
    /// identifiers. If none match, destroy_link SHALL terminate and set error to 4. </para><para>
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
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceError"/>.
    /// </returns>
    DeviceError DestroyLink( DeviceLink request );

    /// <summary>   Create an interrupt channel. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceRemoteFunc"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>   The new interrupt channel 1. </returns>
    DeviceError CreateIntrChan( DeviceRemoteFunc request );

    /// <summary>   Destroy an interrupt channel. </summary>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceError"/>.
    /// </returns>
    DeviceError DestroyIntrChan();

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
    DeviceError DeviceClear( DeviceGenericParms request );

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceDoCmdParms"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="DeviceDoCmdResp"/>.
    /// </returns>
    DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request );

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
    DeviceError DeviceEnableSrq( DeviceEnableSrqParms request );

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
    DeviceError DeviceLocal( DeviceGenericParms request );

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
    DeviceError DeviceRemote( DeviceGenericParms request );

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
    DeviceReadStbResp DeviceReadStb( DeviceGenericParms request );

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
    DeviceError DeviceTrigger( DeviceGenericParms request );

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
    DeviceError DeviceLock( DeviceLockParms request );

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
    DeviceError DeviceUnlock( DeviceLink deviceLink );

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
    DeviceWriteResp DeviceWrite( DeviceWriteParms deviceWriteParameters );

    #endregion

    #region " thread exception handler "

    /// <summary>
    /// Event queue for all listeners interested in ThreadExceptionOccurred events.
    /// </summary>
    public event ThreadExceptionEventHandler? ThreadExceptionOccurred;

    #endregion

}
