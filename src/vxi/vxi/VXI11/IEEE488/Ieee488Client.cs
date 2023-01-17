using System.Net;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.IEEE488;

/// <summary>   An IEEE 488 VXI-11 client. </summary>
public class Ieee488Client : IDisposable
{

    #region " construction, connection and cleanup "

    /// <summary>   Default constructor. </summary>
    public Ieee488Client()
    {
        // Initialize the client identifier with some more-or-less random value.
        this.ClientId = Support.GenerateClientIdentifier();

        // create a client id for this instance;
        // this.ClientId = ( int ) ( DateTime.Now.Subtract( DateTime.Parse( "2023-01-01" ) ).TotalMilliseconds % 0x7FFFFFFF);

        // initialize some values 
        this.MaxReadRawLength = Ieee488Client.MaxReadRawLengthDefault;
        this.MaxReceiveSize = 0;
        this.LastDeviceError = new DeviceErrorCode();
        this.Host = string.Empty;
        this.InterfaceDeviceString = string.Empty;
        this.Eoi = Ieee488Client.EoiEnabledDefault;
        this.LockTimeout = Ieee488Client.LockTimeoutDefault;
        this.LockEnabled = Ieee488Client.LockEnabledDefault;
        this.ReadTermination = Ieee488Client.ReadTerminationDefault;
        this.ReceiveTimeout = Ieee488Client.ReceiveTimeoutDefault;
        this.WriteTermination = Ieee488Client.WriteTerminationDefault;
        this.SendTimeout = Ieee488Client.SendTimeoutDefault;
        this.AbortPort = 0;
    }

    /// <summary>   An internal method to process connecting the device by calling the <see cref="Vxi11Message.CreateLinkProcedure"/> 
    /// RPC and returning the <see cref="DeviceErrorCode"/> codec. </summary>
    /// <param name="hostAddress">              The host device IPv4 address. </param>
    /// <param name="interfaceDeviceString">    The interface device string, e.g., inst0 or gpib0,8. </param>
    /// <returns>   A DeviceErrorCode. </returns>
    private DeviceErrorCode ConnectDevice( string hostAddress, string interfaceDeviceString )
    {
        // First destroy the link if not destroyed. 
        if ( this.Connected ) { _ = this.Close(); }

        this.DeviceLink = null;
        this.Host = string.Empty;
        this.InterfaceDeviceString = string.Empty;

        // instantiate the core client.
        this.CoreClient = new DeviceCoreClient( IPAddress.Parse( hostAddress ), OncRpcProtocols.OncRpcTcp );
        this.ReceiveTimeout = this.ReceiveTimeout;
        this.SendTimeout = this.SendTimeout;

        CreateLinkParms createLinkParam = new() {
            Device = interfaceDeviceString,
            LockDevice = this.LockEnabled,
            LockTimeout = this.LockTimeout,
        };
        CreateLinkResp linkResp = this.CoreClient.CreateLink( createLinkParam );
        if ( linkResp.ErrorCode.Value == DeviceErrorCodeValue.NoError )
        {
            this.DeviceLink = linkResp.DeviceLink;
            this.MaxReceiveSize = linkResp.MaxReceiveSize;
            this.LastDeviceError = linkResp.ErrorCode;
            this.AbortPort = linkResp.AbortPort;

            this.Host = hostAddress;
            this.InterfaceDeviceString = interfaceDeviceString;
        }
        return linkResp.ErrorCode;
    }

    /// <summary>   Connects the device by calling the <see cref="Vxi11Message.CreateLinkProcedure"/> 
    /// RPC and sets the <see cref="LastDeviceError"/> or throws an exception on failure. </summary>
    /// <param name="hostAddress">              The host device IPv4 address. </param>
    /// <param name="interfaceDeviceString">    The interface device string, e.g., inst0 or gpib0,8. </param>
    public void Connect( string hostAddress, string interfaceDeviceString )
    {
        try
        {
            this.LastDeviceError = this.ConnectDevice( hostAddress, interfaceDeviceString );

            if ( this.LastDeviceError.Value != DeviceErrorCodeValue.NoError )
            {
                throw new VXI11.DeviceException( this.LastDeviceError.Value );
            }
        }
        catch
        {
            try
            {
                _ = this.Close();
            }
            catch ( Exception )
            {
                throw;
            }
            throw;
        }
        finally
        {
        }
    }

    /// <summary>   Reconnects this object. </summary>
    /// <remarks>   2023-01-17. </remarks>
    public void Reconnect()
    {
        this.Connect( this.Host, this.InterfaceDeviceString );
    }

    /// <summary>   Closes this object. </summary>
    public DeviceError Close()
    {
        DeviceError? deviceError = new();
        List<Exception> exceptions = new ();
        if ( this.Connected && this.DeviceLink is not null )
        {
            try
            {
                deviceError = this.CoreClient?.DestroyLink( this.DeviceLink );
            }
            catch ( Exception ex )
            {

                exceptions.Add( ex );
            }
            finally
            {
                this.DeviceLink = null;

            }
        }

        try
        {
            this.CoreClient?.Close();
        }
        catch ( Exception ex )
        {
            exceptions.Add( ex );
        }
        finally
        {
            this.CoreClient = null;
        }

        try
        {
            this.AbortClient?.Close();
        }
        catch ( Exception ex )
        {
            exceptions.Add( ex );
        }
        finally
        {
            this.AbortClient = null;
        }

        if ( exceptions.Any() )
        {
            AggregateException aggregateException = new(exceptions);
            throw aggregateException;
        }
        return deviceError ?? new DeviceError();
    }

    #region " disposable implementation "

    /// <summary>   Query if this object is disposed. </summary>
    /// <returns>   True if disposed, false if not. </returns>
    public bool IsDisposed => this.DeviceLink is null || this.CoreClient is null;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    void IDisposable.Dispose()
    {
        if ( this.IsDisposed ) { return; }
        try
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            this.Dispose( true );

            // uncomment the following line if Finalize() is overridden above.
            GC.SuppressFinalize( this );
        }
        catch ( Exception ex ) { Console.WriteLine( ex.ToString() ); }
        finally
        {
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <param name="disposing">    True to release both managed and unmanaged resources; false to
    ///                             release only unmanaged resources. </param>
    private void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // dispose managed state (managed objects)
        }

        // free unmanaged resources and override finalizer
        // I am assuming that the socket used in the derived classes include unmanaged resources.
        _ = this.Close();

        this.CoreClient?.Dispose();
        this.AbortClient?.Dispose();

        // set large fields to null
    }

    /// <summary>   Finalizer. </summary>
    ~Ieee488Client()
    {
        if ( this.IsDisposed ) { return; }
        this.Dispose( false );
    }

    #endregion

    #region " default values "

    /// <summary>   Gets or sets the default maximum length for the <see cref="ReadRaw"/> method. </summary>
    /// <value> The default maximum read raw length. </value>
    public static int MaxReadRawLengthDefault { get; set; } = 128 * 1024 * 1024;

    /// <summary>   Gets or sets the lock timeout default. </summary>
    /// <value> The lock timeout default. </value>
    public static int LockTimeoutDefault { get; set; } = 3000;

    /// <summary>   Gets or sets the receive timeout default. </summary>
    /// <value> The receive timeout default. </value>
    public static int ReceiveTimeoutDefault { get; set; } = 3000;

    /// <summary>   Gets or sets the send timeout default. </summary>
    /// <value> The send timeout default. </value>
    public static int SendTimeoutDefault { get; set; } = 3000;

    /// <summary>   Gets or sets the read termination default. </summary>
    /// <value> The read termination default. </value>
    public static byte ReadTerminationDefault { get; set; } = ( byte ) '\n';

    /// <summary>   Gets or sets the write termination default. </summary>
    /// <value> The write termination default. </value>
    public static byte[] WriteTerminationDefault { get; set; } = new byte[] { ( byte ) '\n' };

    /// <summary>
    /// Gets or sets a value indicating whether the end-or-identify (EOI) terminator is enabled by
    /// default.
    /// </summary>
    /// <value> True if end-or-identify (EOI) terminator is enabled by default, false if not. </value>
    public static bool EoiEnabledDefault { get; set; } = true;

    /// <summary>   Gets or sets a value indicating whether the enabled default is locked. </summary>
    /// <value> True if lock enabled default, false if not. </value>
    public static bool LockEnabledDefault { get; set; } = false;

    #endregion


    #endregion

    #region " VXI-11 members "

    /// <summary>   Gets or sets the Core client. </summary>
    protected DeviceCoreClient? CoreClient { get; set; }

    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    protected int ClientId { get; set; }

    /// <summary>   The  <see cref="DeviceLink"/> that is established upon connection. </summary>
    /// <value> The device link. </value>
    protected DeviceLink? DeviceLink { get; set; }

    /// <summary>   Gets or sets the max data size in bytes device will accept on a write. </summary>
    /// <remarks> This is the size of the largest data set the network instrument server can
    /// accept in a <see cref="Vxi11Message.DeviceWriteProcedure"/> RPC. This value is at least 1024. </remarks>
    /// <value> The maximum <see cref="Vxi11Message.DeviceWriteProcedure"/> data size. </value>
    public int MaxReceiveSize { get; private set; }

    /// <summary>   Gets or sets the last device error. </summary>
    /// <value> The las <see cref="DeviceErrorCode"/> . </value>
    public DeviceErrorCode LastDeviceError { get; private set; } 

    #endregion

    #region " abort port and client "

    /// <summary>   Gets or sets the abort port. </summary>
    /// <value> The abort port. </value>
    private int AbortPort { get; set; }

    /// <summary>   Gets or sets the <see cref="DeviceAsyncClient"/> abort client. </summary>
    /// <value> The abort client. </value>
    protected DeviceAsyncClient? AbortClient { get; set; }

    /// <summary>   Asynchronous abort an in-progress call. </summary>
    /// <exception cref="DeviceException">  Thrown when a VXI-11 error condition occurs. </exception>
    public virtual void Abort()
    {
        if ( !this.Connected )
        {
            this.Connect( this.Host, this.InterfaceDeviceString );
        }

        if ( this.AbortClient is null )
        {
            this.AbortClient = new DeviceAsyncClient( IPAddress.Parse( this.Host ), this.AbortPort, OncRpcProtocols.OncRpcTcp );
            this.AbortClient.Client!.SendTimeout = this.SendTimeout;
            this.AbortClient.Client!.ReceiveTimeout = this.ReceiveTimeout;
        }
        DeviceError error = this.AbortClient.DeviceAbort( this.DeviceLink! );
        if ( error.ErrorCode.Value != DeviceErrorCodeValue.NoError  )
        {
            throw new DeviceException( $"; Abort failed.", error.ErrorCode.Value );
        }
    }

    #endregion

    #region " members "

    /// <summary>   Gets or sets the host IPv4 Address. </summary>
    /// <value> The host. </value>
    public string Host { get; private set; }

    /// <summary>
    /// Gets or sets the interface device string, .e.g, inst0, gpib0,5, or usb0[...].
    /// </summary>
    /// <value> The interface device string. </value>
    public string InterfaceDeviceString { get; private set; }

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

    private int _receiveTimeout;
    /// <summary>   Gets or sets the Receive timeout. </summary>
    /// <value> The Receive timeout. </value>
    public int ReceiveTimeout
    {
        get => this._receiveTimeout;
        set {
            this._receiveTimeout = value;
            if ( this.CoreClient?.Client is not null )
                this.CoreClient.Client.ReceiveTimeout = value;
            if ( this.AbortClient?.Client is not null )
                this.AbortClient.Client.ReceiveTimeout = value;
        }
    }

    private int _sendTimeout;
    /// <summary>   Gets or sets the send timeout in milliseconds. </summary>
    /// <value> The send timeout. </value>
    public int SendTimeout
    {
        get => this._sendTimeout;
        set {
            this._sendTimeout = value;
            if ( this.CoreClient?.Client is not null )
                this.CoreClient.Client.SendTimeout = value;
            if ( this.AbortClient?.Client is not null )
                this.AbortClient.Client.SendTimeout = value;
        }
    }

    /// <summary>   Gets or sets the lock timeout in milliseconds. </summary>
    /// <remarks> </remarks>
    /// <remarks>
    /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a lock
    /// to be released. If the device is locked by another link and the <see cref="LockTimeout"/> is non-zero,
    /// the network instrument server allows at least <see cref="LockTimeout"/> milliseconds for a lock to be 
    /// released.
    /// </remarks>
    public int LockTimeout { get; set; } 

    /// <summary>   Gets or sets a value indicating whether lock is requested on the device. </summary>
    /// <value> True if lock enabled, false if not. </value>
    public bool LockEnabled { get; set; }

    /// <summary>   Gets or sets the write termination. </summary>
    /// <value> The write termination. </value>
    public byte[] WriteTermination { get; set; } 

    /// <summary>   Gets a value indicating whether the VXI Core Client is connected. </summary>
    /// <value> True if connected, false if not. </value>
    public bool Connected => this.DeviceLink is not null;

    #endregion

    #region " send and receive "

    /// <summary>   Query if 'data' is write terminated. </summary>
    /// <param name="data">         . </param>
    /// <param name="termination">  The termination. </param>
    /// <returns>   True if write terminated, false if not. </returns>
    private static bool IsWriteTerminated( byte[] data, byte[] termination )
    {
        bool terminated = data is not null && termination is not null && termination.Length > 0 && data.Length > termination.Length;
        if ( !terminated ) return terminated;
        for ( int i = 0; i < termination!.Length; i++ )
            terminated &= data![^(i + 1)] == termination[i];
        return terminated;
    }

    /// <summary>   Query if 'data' is write terminated. </summary>
    /// <param name="data"> . </param>
    /// <returns>   True if write terminated, false if not. </returns>
    private bool IsWriteTerminated( byte[] data )
    {
        return IsWriteTerminated( data, this.WriteTermination );
    }

    /// <summary>   Query if 'data' is query. </summary>
    /// <param name="data"> . </param>
    /// <returns>   True if query, false if not. </returns>
    private bool IsQuery( byte[] data )
    {
        return data is not null && data.Length > 1 && this.IsWriteTerminated( data ) && Array.IndexOf( data, ( byte ) '?' ) > -1;
    }

    /// <summary>   Send this message. </summary>
    /// <param name="data"> . </param>
    /// <returns>   A <see cref="DeviceWriteResp">device write response</see> . </returns>
    public DeviceWriteResp Send( byte[] data )
    {
        DeviceWriteResp resp = new();
        if ( this.DeviceLink is not null && this.CoreClient is not null )
        {
            DeviceWriteParms writeParam = new() {
                Link = this.DeviceLink,
                IOTimeout = this.SendTimeout, // in ms
                LockTimeout = this.LockTimeout, // in ms
                Flags = new DeviceFlags( this.Eoi ? DeviceOperationFlags.EndIndicator : DeviceOperationFlags.None ),
            };
            writeParam.SetData( data );
            resp = this.CoreClient.DeviceWrite( writeParam );
        }
        return resp;
    }

    /// <summary>   Send this message to the VXI-11 server. </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   A <see cref="DeviceWriteResp">device write response</see> . </returns>
    public DeviceWriteResp Send( string message )
    {
        return this.Send( Encoding.Default.GetBytes( message ) );
    }

    /// <summary>   Receives a reply from the VXI-11 server. </summary>
    /// <returns>   A <see cref="DeviceReadResp">device read response</see> . </returns>
    public DeviceReadResp Receive()
    {
        DeviceReadResp resp = new();
        if ( this.DeviceLink is not null && this.CoreClient is not null )
        {
            DeviceReadParms readParam = new() {
                Link = DeviceLink,
                RequestSize = this.MaxReceiveSize, // response.Length,
                IOTimeout = this.ReceiveTimeout,
                LockTimeout = this.LockTimeout,
                Flags = new DeviceFlags(),
                TermChar = this.ReadTermination
            };
            resp = this.CoreClient.DeviceRead( readParam );
        }
        return resp;
    }

    /// <summary>   Receives a reply from the VXI-11 server. </summary>
    /// <param name="byteCount">    Number of bytes. </param>
    /// <returns>   A <see cref="DeviceReadResp">device read response</see> . </returns>
    public DeviceReadResp Receive( int byteCount )
    {
        DeviceReadResp resp = new();
        if ( this.DeviceLink is not null && this.CoreClient is not null )
        {
            DeviceReadParms readParam = new() {
                Link = DeviceLink,
                RequestSize = byteCount,
                IOTimeout = this.ReceiveTimeout,
                LockTimeout = this.LockTimeout,
                Flags = new DeviceFlags(),
                TermChar = this.ReadTermination
            };
            resp = this.CoreClient.DeviceRead( readParam );
        }
        return resp;
    }

    /// <summary>   Send and receive if query. </summary>
    /// <param name="data">                     . </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay. </param>
    /// <returns>   A Tuple. </returns>
    public (DeviceWriteResp writeResponse, DeviceReadResp readResponse) SendReceive( byte[] data, int millisecondsReadDelay = 3 )
    {
        DeviceWriteResp writeResponse = this.Send( data );
        if ( writeResponse.ErrorCode.Value == DeviceErrorCodeValue.NoError )
            if ( this.IsQuery( data ) )
            {
                Thread.Sleep( millisecondsReadDelay );
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
        return this.SendReceive( Encoding.Default.GetBytes( message ), millisecondsReadDelay );
    }

    #endregion

    #region " raw read and write "

    public int WriteRaw( string data )
    {
        throw new NotImplementedException();
    }

    public int MaxReadRawLength { get; private set; }

    /// <summary>   Reads until all data is read from the instrument. </summary>
    /// <returns>   The data. </returns>
    public string ReadRaw()
    {
        throw new NotImplementedException();
    }


    #endregion

    #region " Write "

    /// <summary>   Sends a message to the VXI-11 server. </summary>
    /// <exception cref="DeviceException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="message">  The message. </param>
    /// <returns>   An int. </returns>
    public int Write( string message )
    {
        if ( string.IsNullOrEmpty( message ) ) return 0;

        (DeviceWriteResp writeResponse, _) = this.SendReceive( Encoding.Default.GetBytes( message ) );

        return writeResponse is null
            ? throw new DeviceException( $"; {nameof( Write )}({nameof( message )}: {message}) write failed; {nameof( DeviceWriteResp )} is null.",
                                       DeviceErrorCodeValue.IOError )
            : writeResponse.ErrorCode is null
                ? throw new DeviceException( $"; {nameof( Write )}({nameof( message )}: {message}) write failed; {nameof( DeviceWriteResp )}.{nameof( DeviceWriteResp.ErrorCode )} is null.",
                                       DeviceErrorCodeValue.IOError )
                : writeResponse.ErrorCode.Value != DeviceErrorCodeValue.NoError
                    ? throw new DeviceException( $"; {nameof( Write )}({nameof( message )}: {message}) write failed.", writeResponse.ErrorCode.Value )
                    : writeResponse.Size;
    }

    /// <summary>   Sends a message with termination to the VXI-11 server. </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   An int. </returns>
    public int WriteLine( string message )
    {
        return this.Write( $"{message}{this.WriteTermination}" );
    }

    /// <summary>
    /// Sends a message to the VXI-11 server and returns an exception message or the message length.
    /// </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, int length, string response) TryWrite( string message )
    {
        try
        {
            return (true, this.Write( message ), string.Empty);
        }
        catch ( Exception ex )
        {
            return (false, 0, ex.Message);
        }
    }

    /// <summary>
    /// Sends a message with termination to the VXI-11 server and returns an exception message or the
    /// message length.
    /// </summary>
    /// <param name="message">  The message. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, int length, string response) TryWriteLine( string message )
    {
        return this.TryWrite( $"{message}{this.WriteTermination}" );
    }

    #endregion

    #region " Read "

    /// <summary>   Receives a message from the VXI-11 server. </summary>
    /// <exception cref="DeviceException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="trimEnd">  (Optional) True to trim end. </param>
    /// <returns>   A string. </returns>
    public string Read( bool trimEnd = false )
    {
        DeviceReadResp readResponse = this.Receive();

        if ( readResponse is null )
            throw new DeviceException( $"; {nameof( Read )}({nameof( System.Boolean )}) failed; {nameof( DeviceReadResp )} is null.",
                                       DeviceErrorCodeValue.IOError );
        else if ( readResponse.ErrorCode is null )
            throw new DeviceException( $"; {nameof( Read )}({nameof( System.Boolean )}) failed; {nameof( DeviceReadResp )}.{nameof( DeviceReadResp.ErrorCode )} is null.",
                                       DeviceErrorCodeValue.IOError );
        else if ( readResponse.ErrorCode.Value != DeviceErrorCodeValue.NoError )
            throw new DeviceException( $"; {nameof( Read )}({nameof( System.Boolean )}) failed.", readResponse.ErrorCode.Value );
        else
        {
            int length = readResponse.GetData().Length - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
            return length > 0
                ? Encoding.Default.GetString( readResponse.GetData(), 0, length )
                : string.Empty;
        }
    }

    /// <summary>   Tries to receive a message from the VXI-11 server. </summary>
    /// <param name="trimEnd">  (Optional) True to trim end. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) TryRead( bool trimEnd = false )
    {
        try
        {
            return (true, this.Read( trimEnd ));
        }
        catch ( Exception ex )
        {
            return (false, ex.Message);
        }
    }

    /// <summary>   Receives single-precision values from the VXI-11 server. </summary>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>   The number of received bytes. </returns>
    public int Read( int offset, int count, ref float[] values )
    {
        DeviceReadResp readResponse = this.Receive( count * 4 + offset + 1 );
        // Need to convert to the byte array into single
        Buffer.BlockCopy( readResponse.GetData(), offset, values, 0, values.Length * 4 );
        return readResponse.GetData().Length;
    }


    #endregion

    #region " Query "

    /// <summary>   Sends a query message to and receives a message from the VXI-11 server. </summary>
    /// <exception cref="DeviceException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) Query( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        if ( string.IsNullOrEmpty( message ) ) return (false, $"{nameof( message )} is empty");

        (DeviceWriteResp writeResponse, DeviceReadResp readResponse) = this.SendReceive( DeviceCoreClient.EncodingDefault.GetBytes( message ), millisecondsReadDelay );
        if ( writeResponse is null )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) write failed; {nameof( DeviceWriteResp )} is null.",
                                       DeviceErrorCodeValue.IOError );
        else if ( writeResponse.ErrorCode is null )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) write failed; {nameof( DeviceWriteResp )}.{nameof( DeviceWriteResp.ErrorCode )} is null.",
                                       DeviceErrorCodeValue.IOError );
        else if ( writeResponse.ErrorCode.Value != DeviceErrorCodeValue.NoError )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) write failed.", writeResponse.ErrorCode.Value );
        else if ( readResponse is null )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) read failed; {nameof( DeviceReadResp )} is null.",
                                       DeviceErrorCodeValue.IOError );
        else if ( readResponse.ErrorCode is null )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) read failed; {nameof( DeviceReadResp )}.{nameof( DeviceReadResp.ErrorCode )} is null.",
                                       DeviceErrorCodeValue.IOError );
        else if ( readResponse.ErrorCode.Value != DeviceErrorCodeValue.NoError )
            throw new DeviceException( $"; {nameof( Query )}({nameof( message )}: {message}) read failed.", readResponse.ErrorCode.Value );
        else
        {
            int length = readResponse.GetData().Length - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
            return length > 0
                ? (true, DeviceCoreClient.EncodingDefault.GetString( readResponse.GetData(), 0, length ))
                : (true, string.Empty);
        }
    }

    /// <summary>   Sends a query message with termination to the VXI-11 server and returns the reply message. </summary>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>   The line. </returns>
    public (bool success, string response) QueryLine( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        return this.Query( $"{message}{this.WriteTermination}", millisecondsReadDelay, trimEnd );
    }


    /// <summary>
    /// Sends a query message to the VXI-11 server and returns the replay message or an exception
    /// message.
    /// </summary>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) TryQuery( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        try
        {
            return this.Query( message, millisecondsReadDelay, trimEnd );
        }
        catch ( Exception ex )
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Sends a query message with termination to the VXI-11 server and returns the replay message or
    /// an exception message.
    /// </summary>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) TryQueryLine( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        return this.TryQuery( $"{message}{this.WriteTermination}", millisecondsReadDelay, trimEnd );
    }

    /// <summary>   Sends a query message and reads the reply as a single-precision values. </summary>
    /// <param name="message">  The message. </param>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>   The number of received bytes. </returns>
    public int Query( string message, int offset, int count, ref float[] values )
    {
        if ( string.IsNullOrEmpty( message ) ) return 0;
        _ = this.Write( message );
        return this.Read( offset, count, ref values );
    }

    /// <summary>   Sends a query message with termination and reads the reply as a single-precision values. </summary>
    /// <param name="message">  The message. </param>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>   The number of received bytes. </returns>
    public int QueryLine( string message, int offset, int count, ref float[] values )
    {
        if ( string.IsNullOrEmpty( message ) ) return 0;
        _ = this.WriteLine( message );
        return this.Read( offset, count, ref values );
    }

    #endregion
}
