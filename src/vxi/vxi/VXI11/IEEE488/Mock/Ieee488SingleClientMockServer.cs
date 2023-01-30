using System.Net;
using System.Reflection;

using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Visa;
using cc.isr.VXI11.Logging;
using cc.isr.ONC.RPC.Client;
using System;

namespace cc.isr.VXI11.IEEE488.Mock;

/// <summary>   An IEEE488 Mock server capable of serving a single client. </summary>
/// <remarks>   
/// Closing a client connected to the Mock local server no longer throws an exception when destroying the link.
/// </remarks>
public partial class Ieee488SingleClientMockServer : CoreChannelServerBase
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public Ieee488SingleClientMockServer() : this( null, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port number where the server will wait for incoming calls. </param>
    public Ieee488SingleClientMockServer( int port ) : this( null, port )
    {
    }

    public Ieee488SingleClientMockServer( IPAddress? bindAddr, int port ) : this( new Ieee488Device(), bindAddr, port )
    {
    }

    public Ieee488SingleClientMockServer( Ieee488Device device ) : this( device, null, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Ieee488SingleClientMockServer( Ieee488Device device, IPAddress? bindAddr, int port ) : base( bindAddr ?? IPAddress.Any, port )
    {
        this._device = device;
        this._interfaceDeviceString = string.Empty;
        this._readMessage = string.Empty;
        this._writeMessage = string.Empty;
        this.AbortPortNumber = AbortChannelServer.AbortPortDefault;
        this.MaxReceiveLength = Ieee488Client.MaxReceiveLengthDefault;
        this.InterruptAddress = IPAddress.None;
        this.DeviceLink = new DeviceLink();
    }

    /// <summary>
    /// Terminates listening if active and closes the  all transports listed in a set of server
    /// transports. Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// Allows a timeout of <see cref="P:cc.isr.ONC.RPC.Server.OncRpcServerStubBase.ShutdownTimeout" />
    /// milliseconds for the server to stop listening before raising an exception to that effect.
    /// </remarks>
    /// <exception cref="AggregateException">   Thrown when an Aggregate error condition occurs. </exception>
    /// <param name="disposing">    True to release both managed and unmanaged resources; false to
    ///                             release only unmanaged resources. </param>
    protected override void Dispose( bool disposing )
    {
        List<Exception> exceptions = new();
        if ( disposing )
        {
            // dispose managed state (managed objects)

            this._device = null;
            this.DeviceLink = new DeviceLink();

            AbortChannelServer? abortServer = this.AbortServer;
            try
            {
                if ( abortServer is not null )
                {
                    using Task? task = this.DisableAbortServerAsync();
                    task.Wait();
                    if ( task.IsFaulted ) exceptions.Add( task.Exception );
                }
            }
            catch ( Exception ex )
            {
                { exceptions.Add( ex ); }
            }
            finally
            {
                this.AbortServer = null;
            }

            InterruptChannelClient? interruptClient = this.InterruptClient;
            try
            {
                interruptClient?.Close();
            }
            catch ( Exception ex )
            {
                exceptions.Add( ex );
            }
            finally
            {
                this.InterruptClient = null;
            }

        }

        // free unmanaged resources and override finalizer

        // set large fields to null

        // call base dispose( bool ).

        try
        {
            base.Dispose( disposing );
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
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Device error event information. </param>
    private void HandleAbortRequest( object sender, DeviceErrorEventArgs e )
    {
        if ( this._device is null ) return;
        e.ErrorCodeValue = this._device.Abort().ErrorCode.ErrorCodeValue;
    }

    protected AbortChannelServer? AbortServer { get; set; }

    /// <summary>   Starts the abort server. </summary>
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
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <returns>   A DeviceErrorCode. </returns>
    protected virtual void StartAbortServer()
    {
        if ( this.AbortServer is null )
        {
            this.AbortServer = new AbortChannelServer( this.IPv4Address, this.AbortPortNumber );
            this.AbortServer.AbortRequested += this.HandleAbortRequest;
            this.AbortServer.Run();
        }
    }

    /// <summary>   Enables (start) the abort server asynchronously. </summary>
    /// <remarks>   2023-01-30. </remarks>
    /// <returns>   A Task. </returns>
    public virtual async Task EnableAbortServerAsync()
    {
        await Task.Factory.StartNew( () => { this.StartAbortServer(); } )
                .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ), TaskContinuationOptions.OnlyOnFaulted );
    }

    /// <summary>   The default time for waiting the abort server to stop listening. </summary>
    public static int AbortServerDisableTimeoutDefault = 500;

    /// <summary>   The abort server disable loop delay default. </summary>
    public static int AbortServerDisableLoopDelayDefault = 50;

    /// <summary>   Stops abort server. </summary>
    /// <param name="timeout">      (Optional) The timeout. </param>
    /// <param name="loopDelay">    The loop delay. </param>
    protected virtual void StopAbortServer( int timeout = 500, int loopDelay = 50 )
    {
        if ( this.AbortServer is not null && this.AbortServer.Running )
        {
            try
            {
                this.AbortServer.AbortRequested -= this.HandleAbortRequest;
                this.AbortServer.StopRpcProcessing();
                DateTime endT = DateTime.Now.AddMilliseconds( timeout );
                while ( endT > DateTime.Now && this.AbortServer.Running )
                {
                    // allow the thread time to address the request
                    Task.Delay( 50 ).Wait();
                }
            }
            catch ( Exception )
            {
                throw;
            }
            finally
            {
                this.AbortServer.Close();
                this.AbortServer = null;
            }
        }
    }

    /// <summary>   Disables (stops) the abort server asynchronously. </summary>
    /// <remarks>   2023-01-28. </remarks>
    /// <param name="timeout">      (Optional) The timeout. </param>
    /// <param name="loopDelay">    (Optional) The loop delay. </param>
    public virtual async Task DisableAbortServerAsync( int timeout = 500, int loopDelay = 50 )
    {
        await Task.Factory.StartNew( () => { this.StopAbortServer( timeout, loopDelay ); } )
                .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ), TaskContinuationOptions.OnlyOnFaulted );
    }

    /// <summary>   Disables the abort server synchronously. </summary>
    /// <remarks>   2023-01-30. </remarks>
    /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
    /// <param name="timeout">      (Optional) The timeout. </param>
    /// <param name="loopDelay">    (Optional) The loop delay. </param>
    public virtual void DisableAbortServerSync( int timeout = 500, int loopDelay = 50 )
    {
        AbortChannelServer? abortServer = this.AbortServer;
        try
        {
            if ( abortServer is not null )
            {
                using Task? task = this.DisableAbortServerAsync( timeout, loopDelay );
                task.Wait();
                if ( task.IsFaulted ) throw task.Exception;
            }
        }
        catch ( Exception )
        {
            throw;
        }
        finally
        {
            this.AbortServer = null;
        }
    }

    #endregion

    #region " Interrupt port and client "

    /// <summary>   The interrupt address as set when getting the <see cref="CreateIntrChan(DeviceRemoteFunc)"/> RPC. </summary>
    private IPAddress InterruptAddress { get; set; }

    private bool InterruptEnabled { get; set; }

    /// <summary>   the Handle of the interrupt as received when getting the <see cref="DeviceEnableSrq(DeviceEnableSrqParms)"/> RPC. </summary>
    private byte[] _interruptHandle = new byte[40];

    private int InterruptTransmitTimeout { get; set; }

    /// <summary>   Gets or sets the interrupt connect timeout. </summary>
    /// <value> The interrupt connect timeout. </value>
    public int InterruptConnectTimeout { get; set; }

    /// <summary>   Gets or sets the interrupt i/o timeout. </summary>
    /// <value> The interrupt i/o timeout. </value>
    public int InterruptIOTimeout { get; set; }

    /// <summary>   Gets or sets the Interrupt port. </summary>
    /// <value> The Interrupt port. </value>
    private int InterruptPort { get; set; }

    /// <summary>   Gets or sets the interrupt protocol. </summary>
    /// <value> The interrupt protocol. </value>
    private TransportProtocol InterruptProtocol { get; set; }

    /// <summary>   Gets or sets the <see cref="InterruptChannelClient"/> Interrupt client. </summary>
    /// <value> The Interrupt client. </value>
    protected InterruptChannelClient? InterruptClient { get; set; }

    /// <summary>   Asynchronous Interrupt an in-progress call. </summary>
    /// <exception cref="DeviceException">  Thrown when a VXI-11 error condition occurs. </exception>
    public virtual void Interrupt()
    {
        if ( this.InterruptClient is null && this.InterruptEnabled && this.InterruptAddress is not null && this.InterruptAddress != IPAddress.None )
        {
            if ( this.InterruptConnectTimeout == 0 ) this.InterruptConnectTimeout = OncRpcTcpClient.ConnectTimeoutDefault;
            if ( this.InterruptTransmitTimeout == 0 ) this.InterruptTransmitTimeout = OncRpcTcpClient.TransmitTimeoutDefault;
            if ( this.InterruptIOTimeout == 0 ) this.InterruptIOTimeout = OncRpcTcpClient.IOTimeoutDefault;

            this.InterruptClient = new InterruptChannelClient( this.InterruptAddress, this.InterruptPort,
                                                               this.InterruptProtocol == TransportProtocol.Udp ? OncRpcProtocol.OncRpcUdp : OncRpcProtocol.OncRpcTcp,
                                                               this.InterruptConnectTimeout );

            // set the timeouts of the client.
            this.InterruptClient.Client!.TransmitTimeout = this.InterruptTransmitTimeout;
            this.InterruptClient.Client!.IOTimeout = this.InterruptIOTimeout;
        }
        if ( this.InterruptEnabled )
        {
            this.InterruptClient?.DeviceIntrSrq( this._interruptHandle );
        }
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

    #region " mock single device "

    /// <summary>
    /// current device
    /// </summary>
    private IIeee488Device? _device = null;

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

    /// <summary>   Gets or sets the device link to the actual single device. </summary>
    /// <value> The device link. </value>
    private DeviceLink DeviceLink { get; set; }

    /// <summary>
    /// Query if the server can create a new device link given that this is a
    /// single device server.
    /// </summary>
    /// <remarks>   2023-01-28. </remarks>
    /// <returns>   True if device free, false if not. </returns>
    public bool CanCreateNewDeviceLink()
    {
        return this.DeviceLink is null || this.DeviceLink.LinkId == 0;
    }

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
    /// <param name="request">  The request of type <see cref="Codecs.CreateLinkParms"/> to use with
    ///                         the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public override CreateLinkResp CreateLink( CreateLinkParms request )
    {
        if ( this.CanCreateNewDeviceLink() )
        {
            this.DeviceLink = new DeviceLink() { LinkId = 1 };

            CreateLinkResp reply = new() {
                DeviceLink = this.DeviceLink,
                MaxReceiveSize = this.MaxReceiveLength,
                AbortPort = this.AbortPortNumber
            };

            Logger.Writer.LogVerbose( $"creating link to {request.Device}" );

            this.InterfaceDevice = new DeviceAddress( request.Device );
            reply.ErrorCode = this.InterfaceDevice.IsValid()
                ? new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.NoError }
                : new DeviceErrorCode() { ErrorCodeValue = DeviceErrorCodeValue.InvalidLinkIdentifier };
            return reply;
        }
        else
            return new CreateLinkResp() { ErrorCode = new DeviceErrorCode( DeviceErrorCodeValue.DeviceNotAccessible ) };
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
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceLink"/> to use
    ///                         with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public override DeviceError DestroyLink( DeviceLink request )
    {
        try
        {
            this.DeviceLink = new DeviceLink();

            InterruptChannelClient? interruptClient = this.InterruptClient;
            interruptClient?.Close();
            this.InterruptClient = null;
            this.DisableAbortServerSync();
            return new DeviceError();
        }
        catch ( Exception )
        {
            return new DeviceError( new DeviceErrorCode( DeviceErrorCodeValue.IOError ) );
        }
    }

    /// <summary>   Create an interrupt channel. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceRemoteFunc"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>   The new interrupt channel 1. </returns>
    public override DeviceError CreateIntrChan( DeviceRemoteFunc request )
    {
        this.InterruptAddress = request.HostAddr;
        this.InterruptPort = request.HostPort;
        this.InterruptProtocol = request.TransportProtocol;
        DeviceError result = new() { ErrorCode = new DeviceErrorCode( ( int ) OncRpcExceptionReason.OncRpcSuccess ) };
        return result;
    }

    /// <summary>   Destroy an interrupt channel. </summary>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public override DeviceError DestroyIntrChan()
    {
        try
        {
            this.InterruptClient?.Dispose();
            return new DeviceError();
        }
        catch ( Exception )
        {
            return new DeviceError( new DeviceErrorCode( DeviceErrorCodeValue.IOError )  );
        }
        finally
        {
            this.InterruptClient = null;
        }
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
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public override DeviceError DeviceClear( DeviceGenericParms request )
    {
        throw new NotImplementedException();
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceDoCmdParms"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceDoCmdResp"/>.
    /// </returns>
    public override DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request )
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
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceEnableSrqParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public override DeviceError DeviceEnableSrq( DeviceEnableSrqParms request )
    {
        this.InterruptEnabled = request.Enable;
        this._interruptHandle = request.GetHandle();
        return new DeviceError();
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
    public override DeviceError DeviceLocal( DeviceGenericParms request )
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
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceRemote( DeviceGenericParms request )
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
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>   A Device_ReadStbResp. </returns>
    public override DeviceReadStbResp DeviceReadStb( DeviceGenericParms request )
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
    /// <param name="request">  The request of type of type <see cref="Codecs.DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>   A Device_Error. </returns>
    public override DeviceError DeviceTrigger( DeviceGenericParms request )
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
    public override DeviceError DeviceLock( DeviceLockParms request )
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
    public override DeviceError DeviceUnlock( DeviceLink deviceLink )
    {
        throw new NotImplementedException();
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