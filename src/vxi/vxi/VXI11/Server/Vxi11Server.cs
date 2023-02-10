using System.ComponentModel;
using System.Net;

using cc.isr.ONC.RPC.Client;
using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.Server;

/// <summary>  A abstract VXI-11 server. </summary>
/// <remarks> Implements the minimum requirements for a VXI-11 server including an <see cref="AbortChannelServer"/>
/// and <see cref="InterruptChannelClient"/> without handling locks. Intended to be inherited by either a <see cref="Vxi11SingleClientServer"/> 
/// or a <see cref="Vxi11MultiClientServer"/> server. </remarks>
public abstract class Vxi11Server : CoreChannelServerBase
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public Vxi11Server() : this( new Vxi11Device ( new Vxi11Instrument(), new Vxi11Interface() ), IPAddress.Any, 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Vxi11Server( IVxi11Device device, IPAddress bindAddr, int port = 0 ) : base( bindAddr ?? IPAddress.Any, port )
    {
        this.Device = device;
        this.AbortPortNumber = AbortChannelServer.AbortPortDefault;
        this.InterruptAddress = IPAddress.Any;

        this.Device.PropertyChanged += this.OnDevicePropertyChanged;
        this.OnDevicePropertiesChanges( device );
        this.Device.RequestingService += this.OnRequestingService;
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

            if ( this.Device is not null )
            {
                this.Device.Instrument = null;
                this.Device = null;
            }

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
        set
        { if ( this.SetProperty( ref this._abortPortNumber, value ) && this.Device is not null )
                this.Device.AbortPortNumber = value;
        }
    }

    /// <summary>   Handles abort request. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Device error event information. </param>
    private void HandleAbortRequest( object sender, DeviceErrorEventArgs e )
    {
        if ( this.Device is null ) return;
        this.Device.HandleAbortRequest( e );
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
    public static int AbortServerDisableLoopDelayDefault = 5;

    /// <summary>   Stops abort server. </summary>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    The loop delay in milliseconds. </param>
    protected virtual void StopAbortServer( int timeout = 500, int loopDelay = 5 )
    {
        if ( this.AbortServer?.Running ?? false )
            try
            {
                this.AbortServer.AbortRequested -= this.HandleAbortRequest;
                this.AbortServer.StopRpcProcessing();
                DateTime endT = DateTime.Now.AddMilliseconds( timeout );
                while ( endT > DateTime.Now && this.AbortServer.Running )
                    // allow the thread time to address the request
                    Task.Delay( loopDelay ).Wait();
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

    /// <summary>   Disables (stops) the abort server asynchronously. </summary>
    /// <remarks>   2023-01-28. </remarks>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    public virtual async Task DisableAbortServerAsync( int timeout = 500, int loopDelay = 5 )
    {
        await Task.Factory.StartNew( () => { this.StopAbortServer( timeout, loopDelay ); } )
                .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ), TaskContinuationOptions.OnlyOnFaulted );
    }

    /// <summary>   Disables the abort server synchronously. </summary>
    /// <remarks>   2023-01-30. </remarks>
    /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    public virtual void DisableAbortServerSync( int timeout = 500, int loopDelay = 5 )
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

    #region " interrupt port and client "

    /// <summary>   Query if an interrupt channel was created. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <returns>   True if interrupt created, false if not. </returns>
    private bool IsInterruptChannelRequested()
    {
        return !( ( this.InterruptAddress.Equals( IPAddress.Any ) || this.InterruptAddress.Equals( IPAddress.None ) )
                 && this.InterruptPort > 0 );
    }

    /// <summary>   The interrupt address as set when getting the <see cref="CreateIntrChan(DeviceRemoteFunc)"/> RPC. </summary>
    private IPAddress InterruptAddress { get; set; }

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

    /// <summary>   Creates interrupt channel. </summary>
    /// <remarks>   2023-02-09. </remarks>
    protected virtual void CreateInterruptChannel()
    {
        if ( this.InterruptClient is null && this.IsInterruptChannelRequested() )
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
    }

    /// <summary>   Asynchronous Interrupt an in-progress call. </summary>
    /// <exception cref="DeviceException">  Thrown when a VXI-11 error condition occurs. </exception>
    protected virtual void OnRequestingService( Vxi11EventArgs e )
    {
        if ( this.InterruptClient is not null &&  this.IsInterruptChannelRequested() )
            this.InterruptClient?.DeviceIntrSrq( e.ServiceRequestCodec.GetHandle() );
    }

    /// <summary>   Asynchronous Interrupt an in-progress call. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    private void OnRequestingService( object sender, Vxi11EventArgs e )
    {
        this.OnRequestingService ( e );
    }

#endregion

#region " members "

    /// <summary>
    /// Gets or sets the encoding to use when serializing strings. If <see langcref="null" />, the
    /// system's default encoding is to be used.
    /// </summary>
    /// <value> The character encoding. </value>
    public override Encoding CharacterEncoding
    {
        get => base.CharacterEncoding;
        set
        {
            if ( this.SetProperty( base.CharacterEncoding!, value, () => base.CharacterEncoding = value ) )
                if ( this.Device is not null ) { this.Device.CharacterEncoding = value; }
        } 
    }

#endregion

#region " device "

    /// <summary>
    /// Gets a reference to the implementation of the <see cref="IVxi11Device"/> interface.
    /// </summary>
    /// <remarks>   The setter is provided for detaching the reference. </remarks>
    /// <value> A reference to the implemented <see cref="IVxi11Device"/> VXI-11 device. </value>
    public IVxi11Device? Device { get; private set; }

    private void OnDevicePropertyChanged( object sender, PropertyChangedEventArgs e )
    {
        if ( sender is not IVxi11Device ) return;
        this.OnDevicePropertyChanged( ( IVxi11Device ) sender, e.PropertyName );
    }

    private void OnDevicePropertyChanged( IVxi11Device sender, string propertyName )
    {
        if ( sender is not IVxi11Device || string.IsNullOrWhiteSpace( propertyName ) ) return;
        {
            switch ( propertyName )
            {

                case nameof( IVxi11Device.AbortPortNumber ):
                    this.AbortPortNumber = sender.AbortPortNumber;
                    break;

                case nameof( IVxi11Device.CharacterEncoding ):
                    this.CharacterEncoding = sender.CharacterEncoding;
                    break;

            }
        }
    }

    private void OnDevicePropertiesChanges( IVxi11Device sender )
    {
        if ( sender is not IVxi11Device ) return;
        this.OnDevicePropertyChanged( sender, nameof( IVxi11Device.AbortPortNumber ) );
        this.OnDevicePropertyChanged( sender, nameof( IVxi11Device.CharacterEncoding ) );
        this.OnDevicePropertyChanged( sender, nameof( IVxi11Device.DeviceName ) );
    }

#endregion

#region " remote procedure call handlers "

    /// <summary>   Gets the active device link between the <see cref="Client.Vxi11Client"/>
    /// and this <see cref="Vxi11Server"/>. </summary>
    /// <value> The device link. </value>
    public DeviceLink DeviceLink => new ( this.Device?.ActiveServerClient?.LinkId ?? 0 );

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
    /// The network instrument client sends an identifying number in the clientId parameter. While
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
    /// A response of type <see cref="CreateLinkResp"/> to send to the remote procedure call.
    /// </returns>
    public override CreateLinkResp CreateLink( CreateLinkParms request )
    {
        return this.Device is null
            ? new CreateLinkResp() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.CreateLink( request );
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
    public override DeviceError DestroyLink( DeviceLink request )
    {
        if ( this.Device is null )
            return new DeviceError( DeviceErrorCode.DeviceNotAccessible );

        DeviceError reply;
        try
        {
            // get the device to destroy the current link

            reply = this.Device.DestroyLink( request );

            // remove all servers if no more clients.

            if ( this.Device.LinkedClientsCount == 0 )
            {
                InterruptChannelClient? interruptClient = this.InterruptClient;
                interruptClient?.Close();
                this.InterruptClient = null;
            }
        }
        catch ( Exception )
        {
            reply = new ( DeviceErrorCode.IOError );
        }
        finally
        {
        }
        return reply;
    }

    /// <summary>   Create an interrupt channel. </summary>
    /// <remarks> notices that the interrupt channel is created only after the first 
    /// service request is sent to the client. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceRemoteFunc"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError CreateIntrChan( DeviceRemoteFunc request )
    {
        // These constructs only exists in the server class. 
        // no calls are required on the device and instrument classes.

        if ( this.InterruptClient is not null )
            return new DeviceError( DeviceErrorCode.ChannelAlreadyEstablished );

        if ( request.ProgNum != Vxi11ProgramConstants.InterruptProgram
            || request.ProgVers != Vxi11ProgramConstants.InterruptVersion
            || request.TransportProtocol != TransportProtocol.Tcp )
            return new DeviceError( DeviceErrorCode.OperationNotSupported );

        this.InterruptAddress = request.HostAddr;
        this.InterruptPort = request.HostPort;
        this.InterruptProtocol = request.TransportProtocol;

        // if the interrupt requested check is in error it means that either 
        // the port or address are invalid

        DeviceError result = this.IsInterruptChannelRequested()
            ? (new())
            : (new( DeviceErrorCode.ParameterError));

        if ( result.ErrorCode == DeviceErrorCode.NoError )
        {
            try
            {
                this.CreateInterruptChannel();
            }
            catch ( Exception ex )
            {
                Logger.Writer.LogError( "Failed creating interrupt channel", ex );
                result = new DeviceError( DeviceErrorCode.ChannelNotEstablished );
            }
        }
        return result;
    }

    /// <summary>   Destroy an interrupt channel. </summary>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DestroyIntrChan()
    {
        DeviceError reply = new ();
        try
        {
            // no calls are required on the device. 
            // just make sure to tell it that interrupts are disabled.

            this.Device?.EnableInterrupt( false, Array.Empty<byte>() );

            if ( this.InterruptClient is not null )
                this.InterruptClient?.Dispose();
            else
                reply = new DeviceError( DeviceErrorCode.ChannelNotEstablished );
        }
        catch ( Exception )
        {
            reply = new DeviceError( DeviceErrorCode.IOError );
        }
        finally
        {
            this.InterruptClient = null;
        }
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
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DeviceClear( DeviceGenericParms request )
    {
        return this.Device is null
            ? new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceClear( request );
    }

    /// <summary>   The device executes a command. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="request">  The request of type of type <see cref="DeviceDoCmdParms"/> to
    ///                         use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceDoCmdResp"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request )
    {
        return this.Device is null
            ? new DeviceDoCmdResp() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceDoCmd( request );
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
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DeviceEnableSrq( DeviceEnableSrqParms request )
    {
        return this.Device is null
            ? new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceEnableSrq( request );
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
    /// <param name="request">  The request of type of type <see cref="DeviceGenericParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DeviceLocal( DeviceGenericParms request )
    {
        return this.Device is null
            ? new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceLocal( request );
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
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DeviceRemote( DeviceGenericParms request )
    {
        return this.Device is null
            ? new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceRemote( request );
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
    /// <returns>
    /// A response of type <see cref="DeviceReadStbResp"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceReadStbResp DeviceReadStb( DeviceGenericParms request )
    {
        return this.Device is null
            ? new DeviceReadStbResp() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceReadStb( request );
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
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DeviceTrigger( DeviceGenericParms request )
    {
        return this.Device is null
            ? new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceTrigger( request );
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
    /// <param name="request">  The request of type of type <see cref="DeviceLockParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceError"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceError DeviceLock( DeviceLockParms request )
    {
        return this.Device is null
            ? new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceLock( request );
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
    public override DeviceError DeviceUnlock( DeviceLink deviceLink )
    {
        return this.Device is null
            ? new DeviceError() { ErrorCode = DeviceErrorCode.DeviceNotAccessible }
            : this.Device.DeviceUnlock( deviceLink );
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
    /// <param name="request">  The request of type of type <see cref="DeviceReadParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceReadResp"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceReadResp DeviceRead( DeviceReadParms request )
    {
        DeviceReadResp readRes = new();
        if ( this.Device is null )
            readRes.ErrorCode = DeviceErrorCode.DeviceNotAccessible;
        else
            readRes = this.Device.DeviceRead( request );
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
    /// <param name="request">  The request of type of type <see cref="DeviceReadParms"/>
    ///                         to use with the remote procedure call. </param>
    /// <returns>
    /// A response of type <see cref="DeviceWriteResp"/> to send to the remote procedure call.
    /// </returns>
    public override DeviceWriteResp DeviceWrite( DeviceWriteParms request )
    {
        DeviceWriteResp writeRes = new();
        if ( this.Device is null )
            writeRes.ErrorCode = DeviceErrorCode.DeviceNotAccessible;
        else
            writeRes = this.Device.DeviceWrite( request );
        return writeRes;
    }

#endregion

}
