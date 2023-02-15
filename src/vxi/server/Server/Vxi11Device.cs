using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Logging;

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;

namespace cc.isr.VXI11.Server;

/// <summary>   A basic implementation of a <see cref="IVxi11Device"/> interface. </summary>
/// <remarks>
/// This class serves to interface between a VXI-11 server, such as <see cref="Vxi11Server"/>
/// and a VXI-11 'physical' instrument, such as <see cref="Vxi11Instrument"/>.
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
public partial class Vxi11Device : IVxi11Device
{

    #region " construction and cleanup "

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="instrumentFactory">    The instrument factory. </param>
    /// <param name="interfaceFactory">     The interface factory. </param>
    public Vxi11Device( Vxi11InstrumentFactory instrumentFactory, Vxi11InterfaceFactory interfaceFactory )
    {
        this.InstrumentFactory = instrumentFactory;
        this.InterfaceFactory = interfaceFactory;

        this.CharacterEncoding = CoreChannelClient.EncodingDefault;
        this._characterEncoding = CoreChannelClient.EncodingDefault;
        this.MaxReceiveLength = CoreChannelClient.MaxReceiveLengthDefault;
        this.AbortPortNumber = AbortChannelServer.AbortPortDefault;
        this.ReadTermination = CoreChannelClient.ReadTerminationDefault;
        this.WriteTermination = CoreChannelClient.WriteTerminationDefault;
        this._writeTermination = CoreChannelClient.WriteTerminationDefault;
        this.ServerClientsRegistry = new();
    }

    #endregion

    #region " thread exception handler "

    /// <summary>
    /// Event queue for all listeners interested in ThreadExceptionOccurred events.
    /// </summary>
    public event ThreadExceptionEventHandler? ThreadExceptionOccurred;

    /// <summary>   Executes the <see cref="ThreadExceptionOccurred"/> event. </summary>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    public virtual void OnThreadException( ThreadExceptionEventArgs e )
    {
        var handler = this.ThreadExceptionOccurred;
        handler?.Invoke( this, e );
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

    /// <summary>   Handles abort request. </summary>
    /// <param name="e">        Device error event information. </param>
    public void HandleAbortRequest( DeviceErrorEventArgs e )
    {
        e.ErrorCodeValue = this.ActiveInstrument is null
                                ? DeviceErrorCode.DeviceNotAccessible
                                : this.ActiveInstrument.Abort();
    }

    #endregion

    #region " interrupt port and client "

    private bool _interruptEnabled;
    /// <summary>   Gets or sets a value indicating whether the interrupt is enabled. </summary>
    /// <value> True if interrupt enabled, false if not. </value>
    public bool InterruptEnabled => this._interruptEnabled;

    /// <summary>   Enables or disables the interrupt. </summary>
    /// <param name="enable">   True to enable, false to disable. </param>
    /// <param name="handle">   The handle. </param>
    public void EnableInterrupt( bool enable, byte[] handle )
    {
        this.ActiveInstrument?.EnableInterrupt( enable, handle );
        _ = this.OnPropertyChanged( ref this._interruptEnabled, enable, nameof( this.InterruptEnabled ) );
    }

    /// <summary>
    /// Event queue for all listeners interested in <see cref="RequestingService"/> events.
    /// </summary>
    public event EventHandler<cc.isr.VXI11.Vxi11EventArgs>? RequestingService;

    /// <summary>   Raises the <see cref="RequestingService"/> event. </summary>
    /// <remarks>   2023-02-06. </remarks>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    protected virtual void OnRequestingService( Vxi11EventArgs e )
    {
        // TODO: add device code here.

        if ( e is not null ) RequestingService?.Invoke( this, e );

    }

    /// <summary>   Filters and passes on the service request. </summary>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        VXI-11 event information. </param>
    private void OnRequestingService( object sender, Vxi11EventArgs e )
    {
        // the servicing goes to all clients where the client filtering goes.
        // if ( e.ServiceRequestCodec.ClientId == this.ClientId ) { this.OnRequestingService( e ); }
        this.OnRequestingService( e );
    }

    #endregion

    #region " members "

    private DeviceErrorCode _lastDeviceError;
    /// <summary>   Gets or sets the last device error. </summary>
    /// <value> The las <see cref="DeviceErrorCode"/> . </value>
    public DeviceErrorCode LastDeviceError
    {
        get => this._lastDeviceError;
        set => _ = this.SetProperty( ref this._lastDeviceError, value );
    }

    private byte _readTermination;
    /// <summary>   Gets or sets the read termination. </summary>
    /// <value> The read termination. </value>
    public byte ReadTermination
    {
        get => this._readTermination;
        set => _ = this.SetProperty( ref this._readTermination, value );
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
        set => _ = this.SetProperty( ref this._connectTimeout, value );
    }

    private int _ioTimeout;
    /// <summary>   Gets or sets the I/O timeout. </summary>
    /// <value> The I/O timeout. </value>
    public int IOTimeout
    {
        get => this._ioTimeout;
        set => _ = this.SetProperty( ref this._ioTimeout, value );
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
        set => _ = this.SetProperty( ref this._transmitTimeout, value );
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
        set => _ = this.SetProperty( ref this._lockTimeout, value );
    }

    private byte[] _writeTermination;
    /// <summary>   Gets or sets the write termination. </summary>
    /// <value> The write termination. </value>
    public byte[] WriteTermination
    {
        get => this._writeTermination;
        set => _ = this.SetProperty( ref this._writeTermination, value );
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
        set {
            if ( this.SetProperty( ref this._characterEncoding, value ) )
                if ( this.ActiveInstrument is not null ) this.ActiveInstrument.CharacterEncoding = value;
        }
    }

    private int _maxReceiveLength;
    /// <summary>   Gets or sets the maximum length of the receive. </summary>
    /// <value> The maximum length of the receive. </value>
    public int MaxReceiveLength
    {
        get => this._maxReceiveLength;
        set => _ = this.SetProperty( ref this._maxReceiveLength, value );
    }

    #endregion

    #region " device state "

    private bool _lockEnabled;
    /// <summary>   Gets or sets a value indicating whether lock is requested on the device. </summary>
    /// <value> True if lock enabled, false if not. </value>
    public bool LockEnabled
    {
        get => this._lockEnabled;
        set
        {
            if ( this.SetProperty( ref this._lockEnabled, value ) && this.ActiveInstrument is not null )
                this.ActiveInstrument.LockEnabled = value;
        }
    }

    private bool _remoteEnabled;
    /// <summary>   Gets or sets a value indicating whether Remote is enabled on the device. </summary>
    /// <value> True if remote is enabled, false if not. </value>
    public bool RemoteEnabled
    {
        get => this._remoteEnabled;
        set {
            if ( this.SetProperty( ref this._remoteEnabled, value ) && this.ActiveInstrument is not null )
                this.ActiveInstrument.RemoteEnabled = value;
        }
    }

    #endregion

    #region " instrument management "

    private Vxi11InstrumentFactory InstrumentFactory { get; }

    private ServerClientsRegistry ServerClientsRegistry { get; }

    /// <summary>   Gets the number of linked clients. </summary>
    /// <value> The number of linked clients. </value>
    public int LinkedClientsCount => this.ServerClientsRegistry.Count;

    private ConcurrentDictionary<string, IVxi11Instrument> Instruments { get; } = new();

    /// <summary>   Gets or sets the active instrument. </summary>
    /// <value> The active instrument. </value>
    public IVxi11Instrument? ActiveInstrument { get; set; }

    /// <summary>   Gets the active server client. </summary>
    /// <value> The active server client. </value>
    public ServerClientInfo ActiveServerClient => this.ActiveInstrument?.ActiveServerClient ?? new ServerClientInfo( new CreateLinkParms(), 0 );

    /// <summary>   Await lock release asynchronously. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="timeout">  The timeout to wait for the release of the lock. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool AwaitLockReleaseAsync( int timeout )
    {
        return this.ActiveInstrument?.AwaitLockReleaseAsync( timeout ) ?? true;
    }

    /// <summary>   Raises the instrument property changed event. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    private void OnInstrumentPropertyChanged( object sender, PropertyChangedEventArgs e )
    {
        if ( sender is not IVxi11Instrument ) return;
        this.OnInstrumentPropertyChanged( ( IVxi11Instrument ) sender, e.PropertyName );
    }

    /// <summary>   Raises the instrument property changed event. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="sender">       Source of the event. </param>
    /// <param name="propertyName"> Name of the property. </param>
    private void OnInstrumentPropertyChanged( IVxi11Instrument sender, string propertyName )
    {
        if ( sender is not IVxi11Instrument || string.IsNullOrWhiteSpace( propertyName ) ) return;
        {
            switch ( propertyName )
            {
                case nameof( IVxi11Instrument.CharacterEncoding ):
                    this.CharacterEncoding = sender.CharacterEncoding;
                    break;

                case nameof( IVxi11Instrument.LastDeviceError ):
                    this.LastDeviceError = sender.LastDeviceError;
                    break;

            }
        }
    }

    /// <summary>   Attempts to select active client instrument. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">       Identifier for the link. </param>
    /// <param name="flags">        The flags. </param>
    /// <param name="lockTimeout">  (Optional) The lock timeout. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    private bool TrySelectActiveClientInstrument( int linkId, DeviceOperationFlags flags, int? lockTimeout = null )
    {
        return this.TrySelectActiveClientInstrument( linkId, DeviceOperationFlags.Waitlock == ( flags & DeviceOperationFlags.Waitlock ), lockTimeout );
    }

    /// <summary>   Attempts to select active client instrument. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">       Identifier for the link. </param>
    /// <param name="waitLock">     True to lock, false to unlock the wait. </param>
    /// <param name="lockTimeout">  (Optional) The lock timeout. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    private bool TrySelectActiveClientInstrument( int linkId, bool waitLock, int? lockTimeout = null )
    {
        if ( this.ServerClientsRegistry.TrySelectClient( linkId, out ServerClientInfo client ) )
        {
            IVxi11Instrument instrument = this.Instruments[client.DeviceName];

            if ( this.ActiveInstrument is null
                || !string.Equals( this.ActiveInstrument.DeviceName, instrument.DeviceName, StringComparison.OrdinalIgnoreCase ) )
            {
                this.ActiveInstrument = instrument;
                _ = this.ActiveInstrument.TrySelectClient( linkId, waitLock, lockTimeout );
            }
        }
        return false;
    }


    #endregion

    #region " Interface management "

    private Vxi11InterfaceFactory InterfaceFactory { get; }

    private ConcurrentDictionary<string, IVxi11Interface> Interfaces { get; } = new();

    /// <summary>   Select active interface. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">   Identifier for the link. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    private bool TrySelectActiveInterface( int linkId )
    {
        if ( this.ActiveInterface is not null ) this.ActiveInterface.PropertyChanged -= this.OnInterfacePropertyChanged;
        if ( this.ServerClientsRegistry.TrySelectClient( linkId, out ServerClientInfo client ) )
        {
            DeviceNameParser parser = new( client.DeviceName );
            string interfaceName = parser.InterfaceName;
            this.ActiveInterface = this.Interfaces.GetOrAdd( interfaceName, this.InterfaceFactory.Interface( interfaceName ) );
            this.ActiveInterface.PropertyChanged += this.OnInterfacePropertyChanged;
            return true;
        }

        return false;
    }

    /// <summary>   Gets or sets the active Interface. </summary>
    /// <value> The active Interface. </value>
    private IVxi11Interface? ActiveInterface { get; set; }

    /// <summary>   Executes the interface property changed action. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    private void OnInterfacePropertyChanged( object sender, PropertyChangedEventArgs e )
    {
        if ( sender is not IVxi11Interface) return;
        this.OnInterfacePropertyChanged( ( IVxi11Interface ) sender, e.PropertyName );
    }

    /// <summary>   Executes the interface property changed action. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="sender">       Source of the event. </param>
    /// <param name="propertyName"> Name of the property. </param>
    private void OnInterfacePropertyChanged( IVxi11Interface sender, string propertyName )
    {
        if ( sender is not IVxi11Interface || string.IsNullOrWhiteSpace( propertyName ) ) return;
        {
            switch ( propertyName )
            {
                case nameof( IVxi11Interface.LastDeviceError ):
                    this.LastDeviceError = sender.LastDeviceError;
                    break;

            }
        }
    }

    #endregion

    #region " link id generator "

    private static int _lastLinkId = 0;

    /// <summary>   Gets the next link identifier. </summary>
    /// <remarks>   The link id is zeroed upon reaching <see cref="int.MaxValue"/> </remarks>
    /// <returns>   The next link identifier. </returns>
    public static int GetNextLinkId()
    {
        _lastLinkId = GetCandidateLinkId( _lastLinkId );
        return _lastLinkId;
    }

    public static int GetCandidateLinkId( int currentId )
    {
        return ++currentId == int.MaxValue ? 0 : currentId;
    }

    #endregion

    #region " LXI-11 ONC/RPC Calls "

    /// <summary>   Create a device connection; Opens a link to a device. </summary>
    /// <remarks>
    /// To successfully complete a create_link RPC, a network instrument server SHALL: <para>
    /// 1. If lockDevice is set to true, acquire the lock for the device. </para><para>
    /// 2. Return in <c>link id</c> a link identifier to be used with future calls. The value of <c>link id</c> SHALL be
    /// unique for all currently active links within a network instrument server.  </para><para>
    /// 3. Return in maxRecvSize the size of the largest data parameter the network instrument server
    /// can accept in a <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  RPC.This value SHALL be at least 1024.  </para><para>
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
    /// locked by another link. </para><para>
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
    /// A response of type <see cref="CreateLinkResp"/> to send to the remote procedure call.
    /// </returns>
    public CreateLinkResp CreateLink( CreateLinkParms request )
    {
        CreateLinkResp reply;
        if ( this.ServerClientsRegistry.IsClientLinked( request.ClientId ) )
            reply = new CreateLinkResp() { ErrorCode = DeviceErrorCode.ChannelAlreadyEstablished };
        else
        {
            reply = new() {
                MaxReceiveSize = this.MaxReceiveLength,
                AbortPort = this.AbortPortNumber
            };

            DeviceNameParser parser = new( request.DeviceName );
            reply.ErrorCode = parser.IsValid()
                ? DeviceErrorCode.NoError
                : DeviceErrorCode.InvalidLinkIdentifier;

            if ( reply.ErrorCode != DeviceErrorCode.NoError)
            {
                return reply;
            }

            int linkId = Vxi11Device.GetCandidateLinkId( _lastLinkId );

            // see if we need to add an instrument.

            if ( !this.Instruments.ContainsKey( request.DeviceName ) )
            {
                _ = this.Instruments.TryAdd( request.DeviceName, this.InstrumentFactory.Instrument( request.DeviceName ) );

                // select the existing instrument

                this.ActiveInstrument = this.Instruments[request.DeviceName];

                // set this instrument properties.

                _ = this.ActiveInstrument.AddClient( request, linkId );

                this.ActiveInstrument.RemoteEnabled = true;
            }

            // select the existing instrument
            this.ActiveInstrument = this.Instruments[request.DeviceName];
            this.ActiveInstrument.ActiveServerClient?.ActivateLockTimeout( request.LockTimeout );

            Logger.Writer.LogVerbose( $"creating link to {request.DeviceName} for client {request.ClientId}" );

            _lastLinkId = linkId;
            reply.DeviceLink = new DeviceLink( linkId );

        }
        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

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
    /// <param name="request">  The request of type of type <see cref="DeviceLink"/> to use
    ///                         with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public DeviceError DestroyLink( DeviceLink request )
    {
        DeviceError reply = new ();
        try
        {
            if ( this.ServerClientsRegistry.TrySelectClient( request.LinkId, out ServerClientInfo client ) )
            {
                IVxi11Instrument instrument = this.Instruments[client.DeviceName];
                _ = instrument.RemoveClient( request.LinkId );

                // remove the instrument if it no longer has clients.

                if ( instrument.LinkedClientsCount == 0 )
                {

                    _ = this.Instruments.TryRemove( client.DeviceName, out _ );

                    // remove the active instrument is it was removed not having clients.

                    if ( this.ActiveInstrument is not null && string.Equals( this.ActiveInstrument.DeviceName, instrument.DeviceName, StringComparison.OrdinalIgnoreCase ) )
                        this.ActiveInstrument = null;

                }
            }
            reply.ErrorCode = this.ServerClientsRegistry.RemoveClient( request.LinkId )
                ? DeviceErrorCode.NoError
                : DeviceErrorCode.InvalidLinkIdentifier;
        }
        catch ( Exception )
        {
            reply = new DeviceError( DeviceErrorCode.IOError );
        }
       
        this.LastDeviceError = reply.ErrorCode;
        return reply;
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
    /// If some other link has the lock, device_clear SHALL examine the <see cref="DeviceOperationFlags.Waitlock"/> flag in <c>flags</c> . If the
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
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public DeviceError DeviceClear( DeviceGenericParms request )
    {
        DeviceError reply = new ();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceError( DeviceErrorCode.ChannelNotEstablished );
        else
        {
            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags ) )
            {
                // now this is the active client.

                // TODO: Implement the specifications as defined above.

                reply.ErrorCode = this.ActiveInstrument!.DeviceClear( request.Flags, request.IOTimeout );
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }

        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceDoCmdParms"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceDoCmdResp"/> to send to the remote procedure call.
    /// </returns>
    public DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request )
    {
        DeviceDoCmdResp reply = new ();

        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceDoCmdResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };
        else
        {

            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags ) )
            {
                // now this is the active client.

                if ( this.TrySelectActiveInterface( request.Link.LinkId ) )
                {
                    // run the commands on the interface

                    reply = this.ActiveInterface?.DeviceDoCmd( request ) ?? reply;
                }
                else
                {
                    reply.ErrorCode = DeviceErrorCode.InvalidLinkIdentifier;
                }
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }

        }
        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

    /// <summary>   The device enables or disables the Send Request service. </summary>
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
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public DeviceError DeviceEnableSrq( DeviceEnableSrqParms request )
    {
        DeviceError reply = new();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceError( DeviceErrorCode.ChannelNotEstablished );
        else
        {
            // enable or disable interrupt of the specified server client

            _ = this.ServerClientsRegistry.EnableInterrupt( request.Link.LinkId, request.Enable, request.GetHandle() );

            // if the request comes for the current server client, apply to the instrument.

            if ( request.Link.LinkId == ( this.ActiveInstrument?.ActiveServerClient?.LinkId ?? 0 ) )
                this.ActiveInstrument?.EnableInterrupt( request.Enable, request.GetHandle() );

        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

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
    /// If some other link has the lock, <c>device_local</c> SHALL examine the <see cref="DeviceOperationFlags.Waitlock"/> flag in
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
    /// <param name="request">  The request of type <see cref="DeviceGenericParms"/> to use with the
    ///                         remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public DeviceError DeviceLocal( DeviceGenericParms request )
    {
        DeviceError reply = new ();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceError( DeviceErrorCode.ChannelNotEstablished );
        else
        {
            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags ) )
            {
                // now this is the active client.

                // enable/disable the interrupt for this client.

                reply.ErrorCode = this.ActiveInstrument!.DeviceLocal( request.Flags, request.IOTimeout );
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }
        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

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
    /// If some other link has the lock, <c>device_remote</c> SHALL examine the <see cref="DeviceOperationFlags.Waitlock"/> flag
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
    /// <param name="request">  The request of type of type <see cref="DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public DeviceError DeviceRemote( DeviceGenericParms request )
    {
        DeviceError reply = new ();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceError( DeviceErrorCode.ChannelNotEstablished );
        else
        {

            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags ) )
            {
                // now this is the active client.

                // TODO: Implement the specifications as defined above.

                reply.ErrorCode = this.ActiveInstrument!.DeviceRemote( request.Flags, request.IOTimeout );
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }
        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
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
    /// If some other link has the lock, the procedure examines the <see cref="DeviceOperationFlags.Waitlock"/> flag in <c>flags</c>
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
    /// <returns>
    /// A response of type <see cref="DeviceReadStbResp"/> to send to the remote procedure call.
    /// </returns>
    public DeviceReadStbResp DeviceReadStb( DeviceGenericParms request )
    {
        DeviceReadStbResp reply = new ();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceReadStbResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };
        else
        {
            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags ) )
            {
                // now this is the active client.

                reply.Stb = this.ActiveInstrument!.ReadStatusByte();
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }
        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
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
    /// If some other link has the lock, <c>device_trigger</c> SHALL examine the <see cref="DeviceOperationFlags.Waitlock"/> flag
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
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public DeviceError DeviceTrigger( DeviceGenericParms request )
    {
        DeviceError reply = new();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceError( DeviceErrorCode.ChannelNotEstablished );
        else
        {
            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags ) )
            {
                // now this is the active client.

                // TODO: Implement the specifications as defined above.

                reply.ErrorCode = this.ActiveInstrument!.DeviceTrigger( request.Flags, request.IOTimeout );
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }
        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
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
    /// If some other link has the lock, <c>device_lock</c> SHALL examine the <see cref="DeviceOperationFlags.Waitlock"/> flag in 
    /// <see cref="DeviceLockParms.Flags"/>. If the flag is set, <c>device_lock</c> SHALL block until the lock is free.
    /// If the flag is not set, <c>device_lock</c> SHALL terminate with error set to 11, device locked by another link. </para><para>
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
    /// <param name="request">  The request of type <see cref="DeviceLockParms"/> to use with the
    ///                         remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public DeviceError DeviceLock( DeviceLockParms request )
    {
        DeviceError reply = new();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceError( DeviceErrorCode.ChannelNotEstablished );
        else
        {
            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags, request.LockTimeout ) )
            {
                // selecting a  new active client sets its lock.
                
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }
        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
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
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public DeviceError DeviceUnlock( DeviceLink deviceLink )
    {
        DeviceError reply = new();
        if ( !this.ServerClientsRegistry.ContainsLink( deviceLink.LinkId ) )
            reply.ErrorCode = DeviceErrorCode.ChannelNotEstablished;
        else
        {
            if ( this.ServerClientsRegistry.ContainsLink( deviceLink.LinkId ))
            {
                if ( this.ServerClientsRegistry.IsLocked( deviceLink.LinkId ) )
                {
                    _ = this.ServerClientsRegistry.ReleaseLock( deviceLink.LinkId );

                    if ( ( this.ActiveInstrument?.ActiveServerClient?.LinkId ?? 0 ) == deviceLink.LinkId )
                        this.ActiveInstrument!.ActiveServerClient?.ReleaseLockTimeout();
                }
                else
                {
                    reply.ErrorCode = DeviceErrorCode.NoLockHeldByThisLink;
                }

            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.InvalidLinkIdentifier;
            }

        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

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
    /// <param name="request">  The request of type <see cref="DeviceReadParms"/> to use with the
    ///                         remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceReadResp"/> to send to the remote procedure call.
    /// </returns>
    public DeviceReadResp DeviceRead( DeviceReadParms request )
    {
        DeviceReadResp reply = new();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceReadResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };
        else
        {
            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags ) )
            {
                reply = this.ActiveInstrument!.DeviceRead( request );
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }
        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

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
    /// complete transaction.A network instrument server accepts at least 1,024 bytes in a single 
    /// <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/> call due to RULE B.6.3.  </para><para>
    /// 
    /// The value of data.data_len may be zero, in which case no device actions are performed.  </para>
    /// <para>
    /// 
    /// The <c>link id</c> parameter is compared to the active link identifiers. If none match, <c>
    /// device_write</c> SHALL terminate and set error to 4, invalid link identifier. </para><para>
    /// 
    /// If data.data_len is greater than the value of maxRecvSize returned in create_link,
    /// <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate without transferring any bytes to the device and SHALL
    /// set error to 5 </para><para>
    /// 
    /// If some other link has the lock, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL examine the <see cref="DeviceOperationFlags.Waitlock"/> flag
    /// in <c>flags</c> . If the flag is set, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/> SHALL block until the lock is
    /// free. If the flag is not set, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/> SHALL terminate and set error to 11, 
    /// device already locked by another link. </para><para>
    /// 
    /// If after at least <c>lock_timeout</c> milliseconds the lock is not freed, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>
    /// SHALL terminate with error set to <see cref="DeviceErrorCode.DeviceLockedByAnotherLink"/>(11) . </para><para>
    /// 
    /// If after at least <c>io_timeout</c> milliseconds not all of data has been transferred to the
    /// device, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL terminate with error set to 15, I/O timeout. This timeout 
    /// is based on the entire transaction and not the time required to transfer single bytes. </para><para>
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
    /// <param name="request">  The request of type <see cref="DeviceWriteParms"/> to use with the
    ///                         remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceWriteResp"/> to send to the remote procedure call.
    /// </returns>
    public DeviceWriteResp DeviceWrite( DeviceWriteParms request )
    {
        DeviceWriteResp reply = new();
        if ( !this.ServerClientsRegistry.ContainsLink( request.Link.LinkId ) )
            reply = new DeviceWriteResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };
        else
        {
            // Select the client for this link. This maybe the existing client or a 
            // new client. Either was this should return true because we checked that 
            // the link was created so a client exists for this link

            if ( this.TrySelectActiveClientInstrument( request.Link.LinkId, request.Flags ) )
            {
                // now this is the active client.

                // TODO: Implement the specifications as defined above.

                reply = this.DeviceWrite( request.GetData() );
            }
            else
            {
                reply.ErrorCode = DeviceErrorCode.DeviceLockedByAnotherLink;
            }
        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

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
    /// If some other link has the lock, <see cref="Vxi11Server.DeviceWrite(DeviceWriteParms)"/>  SHALL examine the <see cref="DeviceOperationFlags.Waitlock"/> flag
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
    /// <param name="data">     The data. </param>
    /// <returns>
    /// A response of type <see cref="DeviceWriteResp"/> to send to the remote procedure call.
    /// </returns>
    private DeviceWriteResp DeviceWrite( byte[] data )
    {
        DeviceWriteResp reply = new() {
            ErrorCode = this.ActiveInstrument is null
                            ? DeviceErrorCode.DeviceNotAccessible
                            : data is null
                                ? DeviceErrorCode.IOError
                                : DeviceErrorCode.NoError,
            Size = data is null ? 0 : data.Length
        };
        if ( reply.ErrorCode == DeviceErrorCode.NoError )
        {
            string cmd = this.CharacterEncoding.GetString( data );
            Logger.Writer.LogVerbose( $"{this.ActiveInstrument!.ActiveServerClient} -> Received：{cmd}" );
            reply.ErrorCode = this.ActiveInstrument!.DeviceWrite( cmd );
        }

        this.LastDeviceError = reply.ErrorCode;
        return reply;
    }

    #endregion

}
