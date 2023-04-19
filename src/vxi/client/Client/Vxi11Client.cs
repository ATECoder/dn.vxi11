using System.Net;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Client;

/// <summary>   A VXI-11 client. </summary>
public partial class Vxi11Client : ICloseable
{

    #region " construction, connection and cleanup "

    /// <summary>   Default constructor. </summary>
    public Vxi11Client()
    {

        // get the next client identifier
        this.ClientId = Vxi11Client.GetNextClientId();

        // initialize some values 
        this.MaxReadRawLength = CoreChannelClient.MaxReadRawLengthDefault;
        this.MaxReceiveSize = 0;
        this.LastDeviceError = DeviceErrorCode.NoError;
        this._host = string.Empty;
        this._deviceName = string.Empty;
        this.Eoi = CoreChannelClient.EoiEnabledDefault;
        this.IOTimeout = CoreChannelClient.IOTimeoutDefault;
        this.TransmitTimeout = CoreChannelClient.TransmitTimeoutDefault;
        this.LockTimeout = CoreChannelClient.LockTimeoutDefault;
        this.LockEnabled = CoreChannelClient.LockEnabledDefault;
        this.ReadTermination = CoreChannelClient.ReadTerminationDefault;
        this.WriteTermination = CoreChannelClient.WriteTerminationDefault;
        this._writeTermination = CoreChannelClient.WriteTerminationDefault;
        this.AbortPort = 0;
    }

    /// <summary>
    /// An internal method to process connecting the device by calling the <see cref="Vxi11Message.CreateLinkProcedure"/>
    /// RPC and returning the <see cref="DeviceErrorCode"/> codec.
    /// </summary>
    /// <param name="hostAddress">      The host device IPv4 address. </param>
    /// <param name="deviceName">       The device name, e.g., inst0 or gpib0,8. </param>
    /// <param name="connectTimeout">   The connect timeout. This timeout overrides the
    ///                                 <see cref="ONC.RPC.Client.OncRpcClientBase.TransmitTimeout"/> </param>
    /// <returns>   A DeviceErrorCode. </returns>
    protected virtual DeviceErrorCode ConnectDevice( string hostAddress, string deviceName, int connectTimeout )
    {
        // First destroy the link if not destroyed. 
        if ( this.Connected ) { this.Close(); }

        // clear previous values.
        this.DeviceLink = null;
        this.Host = string.Empty;
        this.DeviceName = string.Empty;

        // save for reconnecting.
        this.ConnectTimeout = connectTimeout;

        // instantiate the core client.
        this.CoreClient = new CoreChannelClient( IPAddress.Parse( hostAddress ), OncRpcProtocol.OncRpcTcp, connectTimeout );

        // set the client timeouts.
        this.IOTimeout = this.IOTimeout;
        this.TransmitTimeout = this.TransmitTimeout;

        // override the client transmit timeout during the connection to allow longer timeout periods.
        this.CoreClient.Client!.TransmitTimeout = connectTimeout;

        CreateLinkResp linkResp = this.CreateLink( this.CoreClient, deviceName );
        return linkResp.ErrorCode;
    }

    /// <summary>
    /// Connects the device by calling the <see cref="Vxi11Message.CreateLinkProcedure"/>
    /// RPC and sets the <see cref="LastDeviceError"/> or throws an exception on failure.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <param name="hostAddress">      The host device IPv4 address. </param>
    /// <param name="deviceName">       The device name, e.g., inst0 or gpib0,8. </param>
    /// <param name="connectTimeout">   (Optional) The connect timeout [3000]. This timeouts
    ///                                 overrides the
    ///                                 <see cref="ONC.RPC.Client.OncRpcClientBase.TransmitTimeout"/> </param>
    public virtual void Connect( string hostAddress, string deviceName, int connectTimeout = 3000 )
    {
        try
        {
            this.LastDeviceError = this.ConnectDevice( hostAddress, deviceName, connectTimeout );

            if ( this.LastDeviceError != DeviceErrorCode.NoError )
            {
                throw new VXI11.DeviceException( this.LastDeviceError );
            }
        }
        catch
        {
            try
            {
                this.Close();
            }
            catch ( Exception )
            {
                throw;
            }
            throw;
        }
        finally
        {
            // restore the client transmit timeout following the connection.
            this.TransmitTimeout = this.TransmitTimeout;
        }
    }

    /// <summary>   Reconnects this object. </summary>
    public void Reconnect()
    {
        if ( this.IsDisposed )
        {
            throw new InvalidOperationException( $"{nameof( Vxi11Client )} @ {this.IPAddress} cannot reconnected because it is disposed." );
        }
        this.Connect( this.Host, this.DeviceName, this.ConnectTimeout );
    }

    /// <summary>
    /// Closes the connection to an VXI-11 server and free all network-related resources.
    /// </summary>
    /// <remarks> This implementation of close and dispose follows the implementation of
    /// the <see cref="System.Net.Sockets.TcpClient"/> at
    /// <see href="https://GitHub.com/microsoft/referencesource/blob/master/System/net/System/Net/Sockets/TCPClient.cs"/>
    /// with the following modifications:
    /// <list type="bullet"> <item>
    /// <see cref="Close()"/> is not <see langword="virtual"/> </item><item>
    /// <see cref="Close()"/> calls <see cref="Dispose()"/> </item><item>
    /// Consequently, <see cref="Close()"/> need not be overridden. </item><item>
    /// <see cref="Close()"/> does not hide any exception that might be thrown by <see cref="Dispose()"/> </item></list>
    /// <list type="bullet"> <item>
    /// The <see cref="Dispose()"/> method skips if <see cref="IsDisposed"/> is <see langword="true"/>; </item><item>
    /// The <see cref="Dispose(bool)"/> accumulates and throws an aggregate exception </item><item>
    /// The <see cref="Dispose()"/> method throws the aggregate exception from <see cref="Dispose(bool)"/>. </item></list>
    /// </remarks>
    public void Close()
    {
        (( IDisposable ) this).Dispose();
    }

    #region " disposable implementation "

    /// <summary>   Query if this object is disposed. </summary>
    /// <returns>   True if disposed, false if not. </returns>
    public bool IsDisposed => this.DeviceLink is null || this.CoreClient is null;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <remarks>
    /// Takes account of and updates <see cref="IsDisposed"/>. Encloses <see cref="Dispose(bool)"/>
    /// within a try...finaly block. <para>
    ///
    /// Because this class is implementing <see cref="IDisposable"/> and is not sealed, then it
    /// should include the call to <see cref="GC.SuppressFinalize(object)"/> even if it does not
    /// include a user-defined finalizer. This is necessary to ensure proper semantics for derived
    /// types that add a user-defined finalizer but only override the protected <see cref="Dispose(bool)"/>
    /// method. </para> <para>
    /// 
    /// To this end, call <see cref="GC.SuppressFinalize(object)"/>, where <see langword="Object"/> = <see langword="this"/> in the <see langword="Finally"/> segment of
    /// the <see langword="try"/>...<see langword="catch"/> clause. </para><para>
    ///
    /// If releasing unmanaged code or freeing large objects then override <see cref="Object.Finalize()"/>. </para>
    /// </remarks>
    public void Dispose()
    {
        if ( this.IsDisposed ) { return; }
        try
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.

            this.Dispose( true );

        }
        catch { throw; }
        finally
        {
            // this is included because this class is not sealed.

            GC.SuppressFinalize( this );

        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <param name="disposing">    True to release large objects and managed and unmanaged resources;
    ///                             false to release only unmanaged resources and large objects. </param>
    protected virtual void Dispose( bool disposing )
    {
        List<Exception> exceptions = new();
        if ( disposing )
        {
            // dispose managed state (managed objects)
            try
            {
                DeviceError? deviceError = this.DestroyLink();
                if ( deviceError is null )
                    throw new DeviceException( $"; failed destroying the link to the {this.DeviceName} device at {this.IPAddress}.",
                        DeviceErrorCode.IOError );
                if ( deviceError.ErrorCode != DeviceErrorCode.NoError )
                    throw new DeviceException( $"; failed destroying the link to the {this.DeviceName} device at {this.IPAddress}.",
                        deviceError.ErrorCode );
            }
            catch ( Exception ex )
            {
                exceptions.Add( ex );
                throw;
            }

            CoreChannelClient? coreClient = this.CoreClient;
            try
            {
                coreClient?.Close();
            }
            catch ( Exception ex )
            {
                exceptions.Add( ex );
            }
            finally
            {
                this.CoreClient = null;
            }

            AbortChannelClient? abortClient = this.AbortClient;
            try
            {
                abortClient?.Close();
            }
            catch ( Exception ex )
            {
                exceptions.Add( ex );
            }
            finally
            {
                this.AbortClient = null;
            }

        }

        // free unmanaged resources and override finalizer

        // set large fields to null

        if ( exceptions.Any() )
        {
            AggregateException aggregateException = new( exceptions );
            throw aggregateException;
        }

    }

    /// <summary>   Finalizer. </summary>
    ~Vxi11Client()
    {
        if ( this.IsDisposed ) { return; }
        this.Dispose( false );
    }

    #endregion

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

    #region " client identifiers "

    private static int _lastClientId = 0;

    /// <summary>   Gets the next client identifier. </summary>
    /// <remarks>   The client id is zeroed upon reaching <see cref="int.MaxValue"/> </remarks>
    /// <returns>   The next client identifier. </returns>
    public static int GetNextClientId()
    {
        return ++_lastClientId == int.MaxValue ? 0 : _lastClientId;
    }

    #endregion

    #region " abort port and client "

    /// <summary>   Gets or sets the abort port. </summary>
    /// <value> The abort port. </value>
    private int AbortPort { get; set; }

    /// <summary>   Gets or sets the <see cref="AbortChannelClient"/> abort client. </summary>
    /// <value> The abort client. </value>
    protected AbortChannelClient? AbortClient { get; set; }

    /// <summary>   Asynchronous abort an in-progress call. </summary>
    /// <exception cref="DeviceException">  Thrown when a VXI-11 error condition occurs. </exception>
    public virtual void Abort()
    {
        if ( !this.Connected )
        {
            this.Connect( this.Host, this.DeviceName );
        }

        if ( this.AbortClient is null )
        {
            this.AbortClient = new AbortChannelClient( IPAddress.Parse( this.Host ), this.AbortPort, OncRpcProtocol.OncRpcTcp, this.ConnectTimeout );
            // set the timeouts of the client.
            this.TransmitTimeout = this.TransmitTimeout;
            this.IOTimeout = this.IOTimeout;
        }
        DeviceError reply = this.AbortClient.DeviceAbort( this.DeviceLink! );
        if ( reply.ErrorCode != DeviceErrorCode.NoError )
        {
            throw new DeviceException( $"; failed sending the {nameof( Vxi11Client.Abort )} command.", reply.ErrorCode );
        }
    }

    #endregion

    #region " VXI-11 members "

    /// <summary>   Gets or sets the Core client. </summary>
    protected CoreChannelClient? CoreClient { get; set; }

    private int _clientId;
    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    public int ClientId
    {
        get => this._clientId;
        set => _ = this.OnPropertyChanged( ref this._clientId, value );
    }

    /// <summary>   The  <see cref="DeviceLink"/> that is established upon connection. </summary>
    /// <value> The device link. </value>
    protected DeviceLink? DeviceLink { get; set; }

    /// <summary>   Gets or sets the max data size in bytes device will accept on a write. </summary>
    /// <remarks>
    /// This is the size of the largest data set the network instrument server can accept in a <see cref="Vxi11Message.DeviceWriteProcedure"/>
    /// RPC. This value is at least 1024. <para>
    /// 
    /// The value is returned from the network instrument is used by the <see cref="CoreChannelClient"/>
    /// for implementing the <see cref="Vxi11Message.DeviceWriteProcedure">Device Write</see>/&gt;
    /// RPC. </para><para>
    /// 
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned short because XDR encodes <see cref="short"/>s as <see cref="int"/>s. </para>
    /// </remarks>
    /// <value> The maximum <see cref="Vxi11Message.DeviceWriteProcedure"/> data size. </value>
    public int MaxReceiveSize { get; private set; }

    /// <summary>   Gets or sets the last device error. </summary>
    /// <value> The las <see cref="DeviceErrorCode"/> . </value>
    public DeviceErrorCode LastDeviceError { get; private set; }

    /// <summary>
    /// Gets the encoding to use when serializing strings. If <see langcref="null" />, the
    /// system's default encoding is to be used.
    /// </summary>
    /// <value> The character encoding. </value>
    public Encoding CharacterEncoding => this.CoreClient?.Client is null ? Encoding.Default : this.CoreClient.Client.CharacterEncoding;

    #endregion

    #region " members "

    private string _host;
    /// <summary>   Gets or sets the host IPv4 Address. </summary>
    /// <value> The host. </value>
    public string Host
    {
        get => this._host;
        private set => _ = this.SetProperty( ref this._host, value );
    }

    /// <summary>   Gets the IP address. </summary>
    /// <value> The IP address. </value>
    public IPAddress IPAddress => IPAddress.Parse( this.Host );

    private string _deviceName;
    /// <summary>
    /// Gets or sets the device name, .e.g, inst0, gpib0,5, or usb0[...].
    /// </summary>
    /// <value> The device name. </value>
    public string DeviceName
    {
        get => this._deviceName;
        set => _ = this.SetProperty( ref this._deviceName, value );
    }

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
        set {
            if ( this.SetProperty( ref this._ioTimeout, value ) )
            {
                if ( this.CoreClient?.Client is not null )
                    this.CoreClient.Client.IOTimeout = value;
                if ( this.AbortClient?.Client is not null )
                    this.AbortClient.Client.IOTimeout = value;
            }
        }
    }

    private int _transmitTimeout;
    /// <summary>   
    /// Gets or sets the timeout during the phase where data is sent within RPC calls, or data is
    /// received within RPC replies. The <see cref="TransmitTimeout"/> timeout must be greater than 0.
    /// </summary>
    /// <remarks> This timeout is set to the <see cref="ConnectTimeout"/> during <see cref="Connect(string, string, int)"/>
    /// actions and restored thereafter. </remarks>
    /// <value> The Transmit timeout. </value>
    public int TransmitTimeout
    {
        get => this._transmitTimeout;
        set {
            if ( this.SetProperty( ref this._transmitTimeout, value ) )
            {
                if ( this.CoreClient?.Client is not null )
                    this.CoreClient.Client.TransmitTimeout = value;
                if ( this.AbortClient?.Client is not null )
                    this.AbortClient.Client.TransmitTimeout = value;
            }
        }
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

    private bool _lockEnabled;
    /// <summary>   Gets or sets a value indicating whether lock is requested on the device. </summary>
    /// <value> True if lock enabled, false if not. </value>
    public bool LockEnabled
    {
        get => this._lockEnabled;
        set => _ = this.SetProperty( ref this._lockEnabled, value );
    }

    private byte[] _writeTermination;
    /// <summary>   Gets or sets the write termination. </summary>
    /// <value> The write termination. </value>
    public byte[] WriteTermination
    {
        get => this._writeTermination;
        set => _ = this.SetProperty( ref this._writeTermination, value );
    }

    /// <summary>   Gets a value indicating whether the VXI Core Client is connected. </summary>
    /// <value> True if connected, false if not. </value>
    public bool Connected => this.DeviceLink is not null && this.CoreClient is not null;

    #endregion

    #region " termination "

    /// <summary>   Gets the termination bytes of a query command. </summary>
    /// <returns>   An array of byte. </returns>
    public byte[] QueryTermination()
    {
        byte[] queryTermination = new byte[this.WriteTermination.Length + 1];
        queryTermination[0] = ( byte ) '?';
        Array.Copy( this.WriteTermination, 0, queryTermination, 1, this.WriteTermination.Length );
        return queryTermination;
    }

    /// <summary>
    /// Query if <paramref name="data"/> ends with the specified <paramref name="termination"/>.
    /// </summary>
    /// <remarks>   2023-02-01. </remarks>
    /// <param name="data">         The data. </param>
    /// <param name="termination">  The termination. </param>
    /// <returns>   True if ends with the termination, false if not. </returns>
    private static bool EndsWith( byte[] data, byte[] termination )
    {
        bool terminated = (termination?.Length > 0) && (data?.Length > termination?.Length);
        if ( !terminated ) return terminated;
        for ( int i = 0; i < termination!.Length; i++ )
            terminated &= data![^(i + 1)] == termination[i];
        return terminated;
    }

    /// <summary>   Query if <paramref name="data"/> is write terminated. </summary>
    /// <param name="data"> . </param>
    /// <returns>   True if write terminated, false if not. </returns>
    private bool IsWriteTerminated( byte[] data )
    {
        return EndsWith( data, this.WriteTermination );
    }

    /// <summary>
    /// Query if <paramref name="data"/> is as query which ends with a '?' and is write terminated.
    /// </summary>
    /// <param name="data"> The data. </param>
    /// <returns>   True if query, false if not. </returns>
    public bool IsWriteTerminatedQuery( byte[] data )
    {
        return EndsWith( data, this.QueryTermination() );
    }

    /// <summary>   Query if <paramref name="data"/> ends with a '?'. </summary>
    /// <param name="data"> The data. </param>
    /// <returns>   True if query, false if not. </returns>
    public bool IsQuery( byte[] data )
    {
        return data[^1] == ( byte ) '?';
    }

    /// <summary>   Query if <paramref name="message"/> ends with the specific <paramref name="termination"/>. </summary>
    /// <param name="message">      The message. </param>
    /// <param name="termination">  The termination. </param>
    /// <returns>   True if ends with the termination, false if not. </returns>
    private static bool EndsWith( string message, byte[] termination )
    {
        bool terminated = (termination?.Length > 0) && (message?.Length > termination?.Length);
        if ( !terminated ) return terminated;
        for ( int i = 0; i < termination!.Length; i++ )
            terminated &= message![^(i + 1)] == termination[i];
        return terminated;
    }

    /// <summary>   Query if the <paramref name="message"/> is write terminated. </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   True if write terminated, false if not. </returns>
    private bool IsWriteTerminated( string message )
    {
        return EndsWith( message, this.WriteTermination );
    }

    /// <summary>
    /// Query if <paramref name="message"/> is as query which ends with a '?' and is write terminated.
    /// </summary>
    /// <remarks>   2023-02-01. </remarks>
    /// <param name="message">  The message. </param>
    /// <returns>   True if query, false if not. </returns>
    public bool IsWriteTerminatedQuery( string message )
    {
        return EndsWith( message, this.QueryTermination() );
    }

    /// <summary>   Query if 'message' ends with a '?'. </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   True if query, false if not. </returns>
    public bool IsQuery( string message )
    {
        return message[^1] == '?';
    }

    #endregion

    #region " send and receive "

    /// <summary>   Send this message. </summary>
    /// <param name="data"> . </param>
    /// <returns>   A <see cref="DeviceWriteResp">device write response</see> . </returns>
    public DeviceWriteResp Send( byte[] data )
    {
        if ( this.DeviceLink is null || this.CoreClient is null ) return new DeviceWriteResp();
        if ( data is null || data.Length == 0 ) return new DeviceWriteResp();
        if ( data.Length > this.MaxReceiveSize )
            throw new DeviceException( $"Data size {data.Length} exceed {nameof( this.MaxReceiveSize )}({this.MaxReceiveSize})", DeviceErrorCode.IOError );

        DeviceWriteParms writeParam = new() {
            Link = this.DeviceLink,
            IOTimeout = this.IOTimeout, // in ms
            LockTimeout = this.LockTimeout, // in ms
            Flags = this.Eoi ? DeviceOperationFlags.EndIndicator : DeviceOperationFlags.None,
        };
        writeParam.SetData( data );
        return this.CoreClient.DeviceWrite( writeParam );
    }

    /// <summary>   Send this message to the VXI-11 server. </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   A <see cref="DeviceWriteResp">device write response</see> . </returns>
    public DeviceWriteResp Send( string message )
    {
        this.Eoi = message.Length <= this.MaxReceiveSize;
        return this.Send( this.CharacterEncoding.GetBytes( message ) );
    }

    /// <summary>   Receives a reply from the VXI-11 server. </summary>
    /// <remarks>   Requests the <see cref="MaxReceiveSize"/> maximum byte length that
    ///             can be received from the server on each read. </remarks>
    /// <returns>   A <see cref="DeviceReadResp">device read response</see> . </returns>
    public DeviceReadResp Receive()
    {
        return this.Receive( this.MaxReceiveSize );
    }

    /// <summary>   Receives a reply from the VXI-11 server. </summary>
    /// <param name="byteCount">    The number of bytes requested from the server. </param>
    /// <returns>   A <see cref="DeviceReadResp">device read response</see> . </returns>
    public DeviceReadResp Receive( int byteCount )
    {
        if ( this.DeviceLink is null || this.CoreClient is null ) return new DeviceReadResp();

        DeviceReadParms readParam = new() {
            Link = DeviceLink,
            RequestSize = byteCount,
            IOTimeout = this.IOTimeout,
            LockTimeout = this.LockTimeout,
            Flags = this.ReadTermination > 0 ? DeviceOperationFlags.TerminationCharacterSet : DeviceOperationFlags.None,
            TermChar = this.ReadTermination
        };
        return this.CoreClient.DeviceRead( readParam );
    }

    /// <summary>   Send and receive if query. </summary>
    /// <param name="data">                     . </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay. </param>
    /// <returns>   A Tuple. </returns>
    public (DeviceWriteResp writeResponse, DeviceReadResp readResponse) SendReceive( byte[] data, int millisecondsReadDelay = 3 )
    {
        DeviceWriteResp writeResponse = this.Send( data );
        if ( writeResponse.ErrorCode == DeviceErrorCode.NoError )
            if ( this.IsWriteTerminatedQuery( data ) )
            {
                if ( millisecondsReadDelay > 0 ) Task.Delay( millisecondsReadDelay ).Wait();
                DeviceReadResp readResponse = this.Receive();
                return (writeResponse, readResponse);
            }
            else
                return (writeResponse, new DeviceReadResp());
        else
            // RPC error
            return (writeResponse, new DeviceReadResp());
    }

    /// <summary>
    /// Send a message to and receives a replay from the VXI-11 server if sending a query message.
    /// </summary>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay. </param>
    /// <returns>   A Tuple. </returns>
    public (DeviceWriteResp writeResponse, DeviceReadResp readResponse) SendReceive( string message, int millisecondsReadDelay = 3 )
    {
        return this.SendReceive( this.CharacterEncoding.GetBytes( message ), millisecondsReadDelay );
    }

    #endregion

    #region " raw read and write "

    /// <summary>
    /// Writes data in raw mode allowing to write a block of data larger than the <see cref="MaxReceiveSize"/>
    /// data that the device accepts in each block.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <param name="data">                 The data to write to the instrument. </param>
    /// <param name="appendTermination">    (Optional) True to append the <see cref="WriteTermination"/>. </param>
    /// <returns>   A Tuple: number of bytes sent, error code, error details. </returns>
    public virtual (int BytesSent, DeviceErrorCode ErrorCode, string ErrorDetails) WriteRaw( string data, bool appendTermination = true )
    {
        if ( this.DeviceLink is null )
            return (0, DeviceErrorCode.ChannelNotEstablished, $"A link was not established to the device");
        if ( this.CoreClient is null )
            return (0, DeviceErrorCode.ChannelNotEstablished, $"The core client was not created");
        if ( string.IsNullOrEmpty( data ) )
            return (0, DeviceErrorCode.IOError, $"the data to write is empty");

        if ( appendTermination )
            data += this.CharacterEncoding.GetString( this.WriteTermination );

        int total = 0;
        int remaining = data.Length;
        int offset = 0;
        while ( remaining > 0 )
        {
            this.Eoi = remaining <= this.MaxReceiveSize;
            var block = data.Substring( offset, Math.Min( remaining, this.MaxReceiveSize ) );

            DeviceWriteResp reply = this.Send( this.CharacterEncoding.GetBytes( block ) );

            if ( reply.ErrorCode != DeviceErrorCode.NoError )
            {
                return (total, reply.ErrorCode, $"{nameof( WriteRaw )}({nameof( data )}: {data}, {nameof( appendTermination )}: {appendTermination} ) block '{block}' failed.");
            }
            else if ( reply.Size < block.Length )
            {
                return (total, DeviceErrorCode.IOError, $"; {nameof( WriteRaw )}({nameof( data )}: {data}, {nameof( appendTermination )}: {appendTermination} ) incomplete block '{block}' {reply.Size}/{block.Length} was written");
            }
            offset += reply.Size;
            remaining -= reply.Size;
            total += reply.Size;
        }
        return (total, DeviceErrorCode.NoError, string.Empty);
    }

    public int MaxReadRawLength { get; private set; }

    /// <summary>
    /// Reads until the <paramref name="byteCount"/> or all data, if <paramref name="byteCount"/> is -
    /// 1, are read from the instrument.
    /// </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <param name="byteCount">    (Optional) Number of bytes to read from the device; [-1] defaults
    ///                             to reading all available data from the device. </param>
    /// <returns>   A Tuple: received data, error code, error details. </returns>
    public (byte[] Data, DeviceErrorCode ErrorCode, string ErrorDetails) ReadRaw( int byteCount = -1 )
    {

        int requestByteCount = (byteCount > 0) ? Math.Min( byteCount, this.MaxReadRawLength ) : this.MaxReadRawLength;

        DeviceReadResp reply = new();
        DeviceReadReasons endOfStream = DeviceReadReasons.EndIndicator | DeviceReadReasons.TermCharIndicator;

        var values = Array.Empty<byte>();

        // Read while read reason does not match the end of stream 

        while ( requestByteCount > 0 && ((reply.Reason & endOfStream) == 0) )
        {
            // request readings from the device.

            reply = this.Receive( requestByteCount );

            // on error, throw and exception 

            if ( reply.ErrorCode != DeviceErrorCode.NoError )
            {
                return (values, reply.ErrorCode, $"; {nameof( ReadRaw )}({nameof( byteCount )}: {byteCount}) failed reading");
            }

            // extend the data by the amount of data received

            values = values.Concat( reply.GetData() ).ToArray();

            // update the remaining count;
            if ( byteCount > 0 )
            {
                byteCount -= reply.GetData().Length;
                if ( byteCount < requestByteCount )
                {
                    requestByteCount = byteCount;
                }
            }
        }
        return (values, DeviceErrorCode.NoError, string.Empty);
    }

    /// <summary>   Queries the device in raw mode. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <param name="data">                     The query message. </param>
    /// <param name="byteCount">                (Optional) Number of bytes to read from the device; [-
    ///                                         1] defaults to reading all available data from the
    ///                                         device. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay. </param>
    /// <returns>   A Tuple: number of bytes sent, received data, error code, error details. </returns>
    public virtual (int SentCount, byte[] Received, DeviceErrorCode ErrorCode, string ErrorDetails) QueryRaw( string data, int byteCount = -1, int millisecondsReadDelay = 3 )
    {
        (int sentCount, DeviceErrorCode errorCode, string details) = this.WriteRaw( data );
        if ( errorCode != DeviceErrorCode.NoError )
            return (sentCount, Array.Empty<byte>(), errorCode, details);
        if ( millisecondsReadDelay > 0 ) Task.Delay( millisecondsReadDelay ).Wait();
        (byte[] values, errorCode, details) = this.ReadRaw( byteCount );
        return (sentCount, values, errorCode, details);
    }

    #endregion

    #region " Write "

    /// <summary>   Sends a message to the VXI-11 server using <see cref="WriteRaw(string, bool)"/>. </summary>
    /// <exception cref="DeviceException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="message">  The message. </param>
    /// <returns>   A Tuple: number of bytes sent, error code, error details. </returns>
    public (int BytesSent, DeviceErrorCode ErrorCode, string ErrorDetails) Write( string message )
    {
        return this.WriteRaw( message, false );
#if false
        if ( string.IsNullOrEmpty( message ) ) return 0;

        (DeviceWriteResp writeResponse, _) = this.SendReceive( this.CharacterEncoding.GetBytes( message ) );

        return writeResponse is null
            ? throw new DeviceException( $"; {nameof( Write )}({nameof( message )}: {message}) write failed; {nameof( DeviceWriteResp )} is null.",
                                       DeviceErrorCode.IOError )
            : writeResponse.ErrorCode != DeviceErrorCode.NoError
                ? throw new DeviceException( $"; {nameof( Write )}({nameof( message )}: {message}) write failed.", writeResponse.ErrorCode )
                : writeResponse.Size;
#endif
    }

    /// <summary>   Sends a message with termination to the VXI-11 server using <see cref="WriteRaw(string, bool)"/>. </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   A Tuple: number of bytes sent, error code, error details. </returns>
    public (int BytesSent, DeviceErrorCode ErrorCode, string ErrorDetails) WriteLine( string message )
    {
        return this.WriteRaw( message, true );
    }

    /// <summary>
    /// Sends a message to the VXI-11 server and returns an exception message or the message length.
    /// </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   A Tuple. </returns>
    public (int BytesSent, DeviceErrorCode ErrorCode, string ErrorDetails) TryWrite( string message )
    {
        try
        {
            return this.WriteRaw( message, false );
        }
        catch ( Exception ex )
        {
            return (0, DeviceErrorCode.IOError, ex.Message);
        }
    }

    /// <summary>   Sends a message with termination to the VXI-11 server using <see cref="WriteRaw(string, bool)"/>
    /// trapping any exception and returning the exception message. </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   A Tuple: number of bytes sent, error code, error details. </returns>
    public (int BytesSent, DeviceErrorCode ErrorCode, string ErrorDetails) TryWriteLine( string message )
    {
        try
        {
            return this.WriteRaw( message, true );
        }
        catch ( Exception ex )
        {
            return (0, DeviceErrorCode.IOError, ex.Message);
        }
    }

    #endregion

    #region " Read "

    /// <summary>   Receives a message from the VXI-11 server using <see cref="ReadRaw(int)"/>. </summary>
    /// <exception cref="DeviceException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="trimEnd">  (Optional) True to trim end. </param>
    /// <returns>   A Tuple: reply, error code, error details. </returns>
    public (string Reply, DeviceErrorCode ErrorCode, string Details) Read( bool trimEnd = false )
    {
        (byte[] data, DeviceErrorCode errorCode, string details) = this.ReadRaw();
        if ( errorCode != DeviceErrorCode.NoError )
            return (string.Empty, errorCode, details);
        int len = data is null ? 0 : data.Length - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
        return len > 0
            ? (this.CharacterEncoding.GetString( data, 0, len ), errorCode, details)
            : (string.Empty, errorCode, details);
#if false
        DeviceReadResp readResponse = this.Receive();

        if ( readResponse is null )
            throw new DeviceException( $"; {nameof( Read )}({nameof( System.Boolean )}) failed; {nameof( DeviceReadResp )} is null.",
                                       DeviceErrorCode.IOError );
        else if ( readResponse.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; {nameof( Read )}({nameof( System.Boolean )}) failed.", readResponse.ErrorCode );
        else
        {
            int length = readResponse.GetData().Length - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
            return length > 0
                ? this.CharacterEncoding.GetString( readResponse.GetData(), 0, length )
                : string.Empty;
        }
#endif
    }

    /// <summary>   Tries to receives a message from the VXI-11 server using <see cref="ReadRaw(int)"/>
    /// catching any exception and returning the exception message. </summary>
    /// <param name="trimEnd">  (Optional) True to trim end. </param>
    /// <returns>   A Tuple: reply, error code, error details. </returns>
    public (string Reply, DeviceErrorCode ErrorCode, string Details) TryRead( bool trimEnd = false )
    {
        try
        {
            return this.Read( trimEnd );
        }
        catch ( Exception ex )
        {
            return (string.Empty, DeviceErrorCode.IOError, ex.Message);
        }
    }

    /// <summary>   Receives single-precision values from the VXI-11 server. </summary>
    /// <remarks>   2023-02-01. </remarks>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>  A tuple: The number of output bytes, which will be the minimum of <paramref name="count"/>
    /// and length of <paramref name="values"/>, error code, error details).
    /// </returns>
    public (int BytesReceived, DeviceErrorCode ErrorCode, string Details) Read( int offset, int count, ref float[] values )
    {
        if ( count == 0 )
            return (0, DeviceErrorCode.IOError, $"requested count is zero");
        int length = count * 4 + offset + 1;
        (byte[] data, DeviceErrorCode errorCode, string details) = this.ReadRaw( length );
        if ( errorCode != DeviceErrorCode.NoError )
            return (0, errorCode, details);
        int receivedLen = data.Length;
        int outputLen = Math.Min( count, values.Length ) * 4;
        if ( receivedLen < outputLen )
            return ( receivedLen, DeviceErrorCode.IOError, $"{nameof( Read )}(int, int, ref float[]) received {receivedLen} of the requested byte output {outputLen}" );
        Buffer.BlockCopy( data, offset, values, 0, outputLen );
        return (outputLen, DeviceErrorCode.NoError, string.Empty);
#if false
        DeviceReadResp readResponse = this.Receive( count * 4 + offset + 1 );
        // Need to convert to the byte array into single
        Buffer.BlockCopy( readResponse.GetData(), offset, values, 0, values.Length * 4 );
        return readResponse.GetData().Length;
#endif
    }

    #endregion

    #region " Query "

    /// <summary>   Sends a query message to and receives a message from the VXI-11 server. </summary>
    /// <remarks>   2023-02-01. </remarks>
    /// <exception cref="DeviceException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>  A Tuple ( reply, error code, error details ).
    /// </returns>
    public (string Reply, DeviceErrorCode ErrorCode, string Details) Query( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        (int sentCount, DeviceErrorCode errorCode, string details) = this.WriteRaw(message, false);
        if ( errorCode != DeviceErrorCode.NoError )
            return ( string.Empty, errorCode, details);
        if ( millisecondsReadDelay > 0 ) Task.Delay( millisecondsReadDelay ).Wait();
        return this.Read( trimEnd );
        
#if false
        if ( string.IsNullOrEmpty( message ) ) return (false, $"{nameof( message )} is empty");

        (DeviceWriteResp writeResponse, DeviceReadResp readResponse) = this.SendReceive( this.CharacterEncoding.GetBytes( message ), millisecondsReadDelay );
        if ( writeResponse is null )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) write failed; {nameof( DeviceWriteResp )} is null.",
                                       DeviceErrorCode.IOError );
        else if ( writeResponse.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) write failed.", writeResponse.ErrorCode );
        else if ( readResponse is null )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) read failed; {nameof( DeviceReadResp )} is null.",
                                       DeviceErrorCode.IOError );
        else if ( readResponse.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) read failed.", readResponse.ErrorCode );
        else
        {
            int length = readResponse.GetData().Length - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
            return length > 0
                ? (true, this.CharacterEncoding.GetString( readResponse.GetData(), 0, length ))
                : (true, string.Empty);
        }
#endif
    }

    /// <summary>   Sends a query message with termination to the VXI-11 server and returns the reply message. </summary>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>  A Tuple ( reply, error code, error details ). </returns>
    public (string Reply, DeviceErrorCode ErrorCode, string Details) QueryLine( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        (_, DeviceErrorCode errorCode, string details) = this.WriteRaw( message, true );
        if ( errorCode != DeviceErrorCode.NoError )
            return (string.Empty, errorCode, details);
        if ( millisecondsReadDelay > 0 ) Task.Delay( millisecondsReadDelay ).Wait();
        return this.Read( trimEnd );
    }


    /// <summary>
    /// Sends a query message to the VXI-11 server and returns the replay message or an exception
    /// message.
    /// </summary>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>  A Tuple ( reply, error code, error details ). </returns>
    public (string Reply, DeviceErrorCode ErrorCode, string Details) TryQuery( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        try
        {
            return this.Query( message, millisecondsReadDelay, trimEnd );
        }
        catch ( Exception ex )
        {
            return (string.Empty, DeviceErrorCode.IOError, ex.Message);
        }
    }

    /// <summary>
    /// Sends a query message with termination to the VXI-11 server and returns the replay message or
    /// an exception message.
    /// </summary>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>  A Tuple ( reply, error code, error details ). </returns>
    public (string Reply, DeviceErrorCode ErrorCode, string Details) TryQueryLine( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        try
        {
            return this.QueryLine( message, millisecondsReadDelay, trimEnd );
        }
        catch ( Exception ex )
        {
            return (string.Empty, DeviceErrorCode.IOError, ex.Message);
        }
    }

    /// <summary>   Sends a query message and reads the reply as a single-precision values. </summary>
    /// <param name="message">  The message. </param>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>  A tuple: the number of bytes sent, The number of output bytes, which will be the minimum of <paramref name="count"/>
    /// and length of <paramref name="values"/>, error code, error details).
    /// </returns>
    public (int BytesSent,int BytesReceived, DeviceErrorCode ErrorCode, string Details) Query( string message, int offset, int count, ref float[] values )
    {
        if ( count == 0 )
            return (0, 0, DeviceErrorCode.IOError, $"requested count is zero");
        (int sentCount, DeviceErrorCode errorCode, string details) = this.WriteRaw( message, false );
        if ( errorCode != DeviceErrorCode.NoError )
            return (sentCount, 0, errorCode, details);
        (int receivedCount, errorCode, details) = this.Read( offset, count, ref values );
        return (sentCount, receivedCount, errorCode, details);
    }

    /// <summary>   Sends a query message with termination and reads the reply as a single-precision values. </summary>
    /// <param name="message">  The message. </param>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>  A tuple: the number of bytes sent, The number of output bytes, which will be the minimum of <paramref name="count"/>
    /// and length of <paramref name="values"/>, error code, error details).
    /// </returns>
    public (int BytesSent, int BytesReceived, DeviceErrorCode ErrorCode, string Details) QueryLine( string message, int offset, int count, ref float[] values )
    {
        if ( count == 0 )
            return (0, 0, DeviceErrorCode.IOError, $"requested count is zero");
        (int sentCount, DeviceErrorCode errorCode, string details) = this.WriteRaw( message, true );
        if ( errorCode != DeviceErrorCode.NoError )
            return (sentCount, 0, errorCode, details);
        (int receivedCount, errorCode, details) = this.Read( offset, count, ref values );
        return (sentCount, receivedCount, errorCode, details);
    }

    #endregion

    #region " VXI-11 call implementations: Client "

    /// <summary>   Creates a link. </summary>
    /// <remarks>   2023-02-07. </remarks>
    /// <param name="coreChannelClient">    The core channel client. </param>
    /// <param name="deviceName">           The device name, e.g., inst0 or gpib0,8. </param>
    /// <returns>   The new link. </returns>
    private CreateLinkResp CreateLink( CoreChannelClient coreChannelClient, string deviceName )
    {
        if ( coreChannelClient is null || coreChannelClient.Client is null )
            return new CreateLinkResp() { ErrorCode = DeviceErrorCode.ChannelNotEstablished };

        CreateLinkResp linkResp = coreChannelClient.CreateLink( this.ClientId, this.LockEnabled, this.LockTimeout, deviceName );
        if ( linkResp.ErrorCode == DeviceErrorCode.NoError )
        {
            this.DeviceLink = linkResp.DeviceLink;
            this.MaxReceiveSize = linkResp.MaxReceiveSize;
            this.LastDeviceError = linkResp.ErrorCode;
            this.AbortPort = linkResp.AbortPort;

            this.Host = coreChannelClient.Client.Host.ToString();
            this.DeviceName = deviceName;
        }
        return linkResp;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DestroyLinkProcedure"/>;
    /// Closes a link to a device.
    /// </summary>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public virtual DeviceError DestroyLink()
    {
        CoreChannelClient? coreClient = this.CoreClient;
        DeviceLink? link = this.DeviceLink;
        try
        {
            return coreClient is not null && link is not null
                ? coreClient.DestroyLink( link )
                : new DeviceError();
        }
        catch ( Exception )
        {
            throw;
        }
        finally
        {
            this.DeviceLink = null;
        }
    }

    /// <summary>   Sends the Clear command. </summary>
    public virtual void Clear()
    {
        if ( this.DeviceLink is null || this.CoreClient is null ) return;
        DeviceError reply = this.CoreClient.DeviceClear( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );

        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11Client.Clear )} command.", reply.ErrorCode );
    }

    /// <summary>   Sends the Lock command. </summary>
    public virtual void Lock()
    {
        if ( this.DeviceLink is null || this.CoreClient is null ) return;
        DeviceError reply = this.CoreClient.DeviceLock( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout );

        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11Client.Lock )} command.", reply.ErrorCode );
    }

    /// <summary>   Sends the Unlock command. </summary>
    public virtual void Unlock()
    {
        if ( this.DeviceLink is null || this.CoreClient is null ) return;
        DeviceError reply = this.CoreClient.DeviceUnlock( this.DeviceLink );

        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11Client.Unlock )} command.", reply.ErrorCode );
    }

    /// <summary>   Sends the Trigger command. </summary>
    public virtual void Trigger()
    {
        if ( this.DeviceLink is null || this.CoreClient is null ) return;
        DeviceError reply = this.CoreClient.DeviceTrigger( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );

        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11Client.Trigger )} command.", reply.ErrorCode );
    }

    #endregion

}
