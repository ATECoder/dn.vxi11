using System.Net;
using System.Net.Sockets;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device explorer tests. </summary>
[TestClass]
public class Vxi11DiscovererLoopbackTests
{

    #region " construction and cleanup "

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

            Logger?.LogInformation( $"starting the embedded Portmap service" );
            Stopwatch sw = Stopwatch.StartNew();
            _embeddedPortMapService = VXI11.Vxi11Discoverer.StartEmbeddedPortmapService();
            _embeddedPortMapService.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

            Logger?.LogInformation( $"{nameof( OncRpcEmbeddedPortmapServiceStub )} started in {sw.Elapsed.TotalMilliseconds:0.0} ms" );
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
        OncRpcEmbeddedPortmapServiceStub? service = _embeddedPortMapService;
        if ( service is not null )
        {
            try
            {
                service.Dispose();
                // service.ThreadExceptionOccurred -= OnThreadExceptionOccurred;
            }
            catch ( Exception ex )
            {
                Logger?.LogError( "Exception cleaning up fixture", ex );
            }
            finally
            {
                _embeddedPortMapService = null;
            }
        }
    }

    private IDisposable? _loggerScope;

    private LoggerTraceListener<Vxi11DiscovererLoopbackTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<Vxi11DiscovererLoopbackTests>( Logger );
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
    public static ILogger<Vxi11DiscovererLoopbackTests>? Logger { get; } = LoggerProvider.InitLogger<Vxi11DiscovererLoopbackTests>();

    #endregion

    #region " ONC RPC Embedded port map and server stub base event handlers "

    private static OncRpcEmbeddedPortmapServiceStub? _embeddedPortMapService;

    /// <summary>   handles the thread exception event. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    internal static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger?.LogError( $"{name}  encountered an exception during an asynchronous operation", e.Exception );
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

    #region " helpers "

    /// <summary>   Gets the host. </summary>
    /// <returns>   The host. </returns>
    internal static IPAddress? GetHost()
    {
        IPHostEntry host = Dns.GetHostEntry( Dns.GetHostName() );
        var ipAddress = host
            .AddressList
            .FirstOrDefault( ip => ip.AddressFamily == AddressFamily.InterNetwork );
        return ipAddress;
    }
    #endregion

    #region " port map tests "

    /// <summary>   (Unit Test Method) Portmap service should ping. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///   2023-02-04 19:24:55.307,cc.isr.VXI11.MSTest.LoopbackDeviceExplorerTests.LoopbackDeviceExplorerTests
    ///   2023-02-04 19:24:55.310,starting the embedded Portmap service
    ///   2023-02-04 19:24:55.311,Checking for Portmap service
    ///   2023-02-04 19:24:55.339, No Portmap service available.
    ///   2023-02-04 19:24:55.339,Creating embedded Portmap instance
    ///   2023-02-04 19:24:55.561, Portmap service started; checked 27.4 ms.
    ///   2023-02-04 19:24:55.562,OncRpcEmbeddedPortmapServiceStub started in 251.5 ms
    ///   2023-02-04 19:24:55.567,2023:02:04:07:24:55.566 pinging Portmap service:
    ///   2023-02-04 19:24:55.571,127.0.0.1 portmap pinged in 4.2 ms.
    /// </code>
    /// </remarks>
    [TestMethod]
    public void PortmapServiceShouldPing()
    {
        Logger?.LogInformation( $"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} pinging Portmap service: " );
        Stopwatch sw = Stopwatch.StartNew();
        IPAddress host = IPAddress.Loopback;
        Assert.IsTrue( VXI11.Vxi11Discoverer.PortmapPingHost( host ), $"port map at {IPAddress.Loopback} should reply to a ping" );
        Logger?.LogInformation( $"{host} portmap pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms." );
    }

    #endregion

}
