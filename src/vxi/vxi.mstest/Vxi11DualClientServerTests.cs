using System.ComponentModel;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Server;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a vxi 11 dual client server tests. </summary>
/// <remarks>   2023-06-01. </remarks>
[TestClass]
public class Vxi11DualClientServerTests
{

    #region " construction and cleanup "

    /// <summary>   Gets or sets the server start time typical. </summary>
    /// <value> The server start time typical. </value>
    public static int ServerStartTimeTypical { get; set; } = 3500;

    /// <summary>   Gets or sets the server start loop delay. </summary>
    /// <value> The server start loop delay. </value>
    public static int ServerStartLoopDelay { get; set; } = 100;


    /// <summary> Initializes the test class before running the first test. </summary>
    /// <param name="testContext"> Gets or sets the test context which provides information about
    /// and functionality for the current test run. </param>
    /// <remarks>Use ClassInitialize to run code before running the first test in the class</remarks>
    [ClassInitialize()]
    public static void InitializeTestClass( TestContext testContext )
    {
        try
        {
            string methodFullName = $"{testContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}";
            if ( Logger is null )
                Console.WriteLine( methodFullName );
            else
                Logger?.LogMemberInfo( methodFullName );

            _server = new();

            _server.PropertyChanged += OnServerPropertyChanged;
            _server.ThreadExceptionOccurred += OnThreadException;

            _ = Task.Factory.StartNew( () => {

                Logger?.LogInformation( "starting the embedded port map service; this takes ~3.5 seconds." );
                using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
                epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

                Logger?.LogInformation( "starting the server task; this takes ~2.5 seconds." );
                _server.Run();
            } ).ContinueWith( failedTask => Vxi11DualClientServerTests.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception! ) ),
                                                                                 TaskContinuationOptions.OnlyOnFaulted );

            Logger?.LogInformation( $"{nameof( Vxi11Server )} waiting running {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * Vxi11DualClientServerTests.ServerStartTimeTypical, Vxi11DualClientServerTests.ServerStartLoopDelay ) )
                throw new InvalidOperationException( "failed starting the ONC/RPC server." );

            Logger?.LogInformation( $"{nameof( Vxi11Server )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
        }
        catch ( Exception ex )
        {
            if ( Logger is null )
                Console.WriteLine( $"Failed initializing the test class: {ex}" );
            else
                Logger.LogMemberError( "Failed initializing the test class:", ex );

            // cleanup to meet strong guarantees

            try
            {
                CleanupTestClass();
            }
            finally
            {
            }
        }
    }

    /// <summary> Cleans up the test class after all tests in the class have run. </summary>
    /// <remarks> Use <see cref="CleanupTestClass"/> to run code after all tests in the class have run. </remarks>
    [ClassCleanup()]
    public static void CleanupTestClass()
    {
        Vxi11Server? server = _server;
        if ( server is not null )
        {
            try
            {
                server.Dispose();
                server.PropertyChanged -= OnServerPropertyChanged;
                server.ThreadExceptionOccurred -= OnThreadException;
            }
            catch ( Exception ex )
            {
                Logger?.LogError( "Failed cleaning up the fixture", ex );
            }
            finally
            {
                _server = null;
            }
        }
    }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<Vxi11DualClientServerTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<Vxi11DualClientServerTests>( Logger );
            _ = Trace.Listeners.Add( this._traceListener );
        }
    }

    /// <summary> Cleans up the test class instance after each test has run. </summary>
    [TestCleanup()]
    public void CleanupAfterEachTest()
    {
        Assert.IsFalse( this._traceListener?.Any( TraceEventType.Error ),
            $"{nameof( this._traceListener )} should have no {TraceEventType.Error} messages" );
        this._loggerScope?.Dispose();
        this._traceListener?.Dispose();
        Trace.Listeners.Clear();
    }

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the
    /// current test run.
    /// </summary>
    /// <value> The test context. </value>
    public TestContext? TestContext { get; set; }

    /// <summary>   Gets a logger instance for this category. </summary>
    /// <value> The logger. </value>
    public static ILogger<Vxi11DualClientServerTests>? Logger { get; } = LoggerProvider.InitLogger<Vxi11DualClientServerTests>();

    #endregion

    #region " VXI-11 Server information and event handlers "

    private static readonly string? _ipv4Address = "127.0.0.1";

    private static Vxi11Server? _server;

    /// <summary>   handles the thread exception event. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    internal static void OnThreadException( ThreadExceptionEventArgs e )
    {
        Logger?.LogError( $"An exception occurred during an asynchronous operation", e.Exception );
    }

    /// <summary>   handles the thread exception event. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    internal static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is Vxi11Server ) name = nameof( Vxi11Server );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger?.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }

    /// <summary>   Handles the property changed event. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is not Vxi11Server ) { return; }
        switch ( e.PropertyName )
        {
            case nameof( Vxi11Server.PortNumber ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( Vxi11Server ) sender).PortNumber}" );
                break;
            case nameof( Vxi11Server.IPv4Address ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( Vxi11Server ) sender).IPv4Address}" );
                break;
            case nameof( Vxi11Server.Running ):
                Logger?.LogInformation( $"{e.PropertyName} set to {(( Vxi11Server ) sender).Running}" );
                break;
        }
    }

    #endregion

    #region " initialization tests "

    /// <summary>   (Unit Test Method) 00 logger should be enabled. </summary>
    /// <remarks>   2023-05-31. </remarks>
    [TestMethod]
    public void A00LoggerShouldBeEnabled()
    {
        Assert.IsNotNull( Logger, $"{nameof( Logger )} should initialize" );
        Assert.IsTrue( Logger.IsEnabled( LogLevel.Information ),
            $"{nameof( Logger )} should be enabled for the {LogLevel.Information} {nameof( LogLevel )}" );
    }

    /// <summary>   (Unit Test Method) 01 logger trace listener should have messages. </summary>
    /// <remarks>   2023-06-01. </remarks>
    [TestMethod]
    public void A01LoggerTraceListenerShouldHaveMessages()
    {
        Assert.IsNotNull( this._traceListener, $"{nameof( this._traceListener )} should initialize" );
        Assert.IsTrue( Trace.Listeners.Count > 0, $"{nameof( Trace )} should have non-zero {nameof( Trace.Listeners )}" );
        Trace.TraceError( "Testing tracing an error" ); Trace.Flush();
        Assert.IsTrue( this._traceListener?.Any( TraceEventType.Error ), $"{nameof( this._traceListener )} should have {TraceEventType.Error} messages" );

        // no need to report errors for this test.

        this._traceListener?.Clear();
    }

    #endregion

    /// <summary>   (Unit Test Method) server should listen. </summary>
    [TestMethod]
    public void ServerShouldListen()
    {
        Assert.IsTrue( _server?.Running );
    }

    /// <summary>   Assert open client. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="ipv4Address">      The IPv4 address. </param>
    /// <param name="lockTimeout">      The lock timeout. </param>
    /// <param name="interfaceNumber">  The interface number. </param>
    /// <returns>   A VXI11.Client.Vxi11Client. </returns>
    private static VXI11.Client.Vxi11Client AssertOpenClient( string ipv4Address, int lockTimeout, int interfaceNumber )
    {
        VXI11.Client.Vxi11Client client = new() {
            LockEnabled = true,
            LockTimeout = lockTimeout
        };
        client.ThreadExceptionOccurred += OnThreadException;
        client.Connect( ipv4Address, DeviceNameParser.BuildDeviceName( DeviceNameParser.GenericInterfaceFamily, interfaceNumber ) );
        return client;
    }

    /// <summary>   Assert identity should query. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="ipv4Address">      The IPv4 address. </param>
    /// <param name="repeatCount">      Number of repeats. </param>
    /// <param name="interfaceNumber">  (Optional) The interface number. </param>
    private static void AssertSecondClientShouldOpenAfterTimeout( string ipv4Address )
    {
        using VXI11.Client.Vxi11Client INSTR0A = AssertOpenClient( ipv4Address, 100, 0 );

        using VXI11.Client.Vxi11Client INSTR0B = AssertOpenClient( ipv4Address, 100, 0 );
    }

    /// <summary>   (Unit Test Method) client should open after timeout. </summary>
    /// <remarks>   2023-06-01. </remarks>
    [TestMethod]
    public void ClientShouldOpenAfterTimeout()
    {
        AssertSecondClientShouldOpenAfterTimeout( _ipv4Address! );
    }

}
