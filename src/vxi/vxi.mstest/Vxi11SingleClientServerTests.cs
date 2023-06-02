using System.ComponentModel;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Server;

namespace cc.isr.VXI11.MSTest;

[TestClass]
public class Vxi11SingleClientServerTests
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
            } ).ContinueWith( failedTask => Vxi11SingleClientServerTests.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception! ) ),
                                                                                 TaskContinuationOptions.OnlyOnFaulted );

            Logger?.LogInformation( $"{nameof( Vxi11Server )} waiting running {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * Vxi11SingleClientServerTests.ServerStartTimeTypical, Vxi11SingleClientServerTests.ServerStartLoopDelay ) )
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

    private LoggerTraceListener<Vxi11SingleClientServerTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<Vxi11SingleClientServerTests>( Logger );
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
    public static ILogger<Vxi11SingleClientServerTests>? Logger { get; } = LoggerProvider.InitLogger<Vxi11SingleClientServerTests>();

    #endregion

    #region " VXI-11 Server information and event handlers "

    private static readonly string? _ipv4Address = "127.0.0.1";

    private static Vxi11Server? _server;

    /// <summary>   Handles the thread exception event. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    internal static void OnThreadException( ThreadExceptionEventArgs e )
    {
        Logger?.LogError( $"An exception occurred during an asynchronous operation", e.Exception );
    }

    /// <summary>   Handles the thread exception event. </summary>
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
        Trace.TraceInformation( "Testing tracing an info message" ); Trace.Flush();
        Assert.IsTrue( this._traceListener?.Any( TraceEventType.Information ), $"{nameof( this._traceListener )} should have {TraceEventType.Error} messages" );

        // no need to report errors for this test.

        this._traceListener?.Clear();
    }

    #endregion

    #region " single client server tests "

    /// <summary>   (Unit Test Method) server should listen. </summary>
    [TestMethod]
    public void ServerShouldListen()
    {
        Assert.IsTrue( _server?.Running );
    }

    /// <summary>   Assert identity should query. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="ipv4Address">      The IPv4 address. </param>
    /// <param name="repeatCount">      Number of repeats. </param>
    /// <param name="interfaceNumber">  (Optional) The interface number. </param>
    private static void AssertIdentityShouldQuery( string ipv4Address, int repeatCount, int interfaceNumber = 0 )
    {
        using VXI11.Client.Vxi11Client vxi11Client = new();
        vxi11Client.ThreadExceptionOccurred += OnThreadException;

        string command = Vxi11InstrumentCommands.IDNRead;
        vxi11Client.Connect( ipv4Address, DeviceNameParser.BuildDeviceName( DeviceNameParser.GenericInterfaceFamily, interfaceNumber ) );
        Console.WriteLine();
        Logger?.LogVerbose( $"Querying {vxi11Client.DeviceName} {repeatCount} times" );
        string identity = _server!.Device!.ActiveInstrument!.Identity;
        int count = repeatCount;
        while ( repeatCount > 0 )
        {
            repeatCount--;
            (string response, DeviceErrorCode errorCode, string errorDetails) = vxi11Client.TryQuery( $"{command}\n", 0 );
            Assert.AreEqual( DeviceErrorCode.NoError, errorCode, errorDetails );
            Assert.AreEqual( identity, response, $"@count = {count - repeatCount}" );
        }

    }

    /// <summary>   (Unit Test Method) identity should query. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///    2023-02-08 11:54:38.666,cc.isr.VXI11.MSTest.Vxi11ServerTests.Vxi11ServerTests
    ///    2023-02-08 11:54:38.677,Vxi11Server waiting running 38.677
    ///    2023-02-08 11:54:38.677,starting the embedded port map service; this takes ~3.5 seconds.
    ///    2023-02-08 11:54:38.678,Checking for Portmap service
    ///    2023-02-08 11:54:38.712, No Portmap service available.
    ///    2023-02-08 11:54:38.712,Creating embedded Portmap instance
    ///    2023-02-08 11:54:38.923, Portmap service started; checked 34.4 ms.
    ///    2023-02-08 11:54:38.924,starting the server task; this takes ~2.5 seconds.
    ///    2023-02-08 11:54:38.930,Running set to True
    ///    2023-02-08 11:54:45.770, Vxi11Server is running  45.770
    ///    2023-02-08 11:54:45.782,creating link to INST0
    ///    2023-02-08 11:54:45.786, link ID: 1 -> Received：*IDN?
    ///
    ///    2023-02-08 11:54:45.786,Processing '*IDN?'
    ///    2023-02-08 11:54:45.787,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    ///    2023-02-08 11:54:45.804, Running set to False
    /// </code>
    /// </remarks>
    [TestMethod]
    public void IdentityShouldQueryMultipleTimes()
    {
        int count = 5;
        AssertIdentityShouldQuery( _ipv4Address!, count );
    }


    /// <summary>   (Unit Test Method) identity should query multiple device names. </summary>
    /// <remarks>   2023-02-14.
    /// <code>
    /// Standard Output: 
    /// 2023-02-14 15:54:14.175,cc.isr.VXI11.MSTest.Vxi11SingleClientServerTests.Vxi11SingleClientServerTests
    /// 2023-02-14 15:54:14.187,Vxi11Server waiting running 14.187
    /// 2023-02-14 15:54:14.187,starting the embedded port map service; this takes ~3.5 seconds.
    /// 2023-02-14 15:54:14.188,Checking for Portmap service
    /// 2023-02-14 15:54:14.226, No Portmap service available.
    /// 2023-02-14 15:54:14.226,Creating embedded Portmap instance
    /// 2023-02-14 15:54:14.451, Portmap service started; checked 37.3 ms.
    /// 2023-02-14 15:54:14.452,starting the server task; this takes ~2.5 seconds.
    /// 2023-02-14 15:54:14.458,Running set to True
    /// 2023-02-14 15:54:21.219, Vxi11Server is running  21.219
    /// 2023-02-14 15:54:21.232,creating link to INST0
    /// 2023-02-14 15:54:21.236, Querying INST0 1 times
    /// 2023-02-14 15:54:21.238,link ID: 1 -> Received：*IDN?
    ///
    /// 2023-02-14 15:54:21.238,Processing '*IDN?'
    /// 2023-02-14 15:54:21.241,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    /// 2023-02-14 15:54:21.249, creating link to INST1
    /// 2023-02-14 15:54:21.249,Querying INST1 1 times
    /// 2023-02-14 15:54:21.249,link ID: 2 -> Received：*IDN?
    ///
    /// 2023-02-14 15:54:21.249,Processing '*IDN?'
    /// 2023-02-14 15:54:21.249,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    /// 2023-02-14 15:54:21.261, Running set to False
    /// </code>
    /// </remarks>
    [TestMethod]
    public void IdentityShouldQueryMultipleDeviceNames()
    {
        int count = 1;
        AssertIdentityShouldQuery( _ipv4Address!, count, 0 );
        AssertIdentityShouldQuery( _ipv4Address!, count, 1 );
    }

    #endregion

}
