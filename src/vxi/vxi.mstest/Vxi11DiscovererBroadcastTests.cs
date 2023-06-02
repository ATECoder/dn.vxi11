using System.Net;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a vxi 11 discoverer broadcast tests. </summary>
/// <remarks>   2023-06-01. </remarks>
[TestClass]
public class Vxi11DiscovererBroadcastTests
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
            Vxi11DiscovererTests.EnumerateHosts();

            Logger?.LogInformation( $"Starting the embedded Portmap service" );
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

    private LoggerTraceListener<Vxi11DiscovererBroadcastTests>? _traceListener;

    /// <summary> Initializes the test class instance before each test runs. </summary>
    [TestInitialize()]
    public void InitializeBeforeEachTest()
    {
        if ( Logger is not null )
        {
            this._loggerScope = Logger.BeginScope( this.TestContext?.TestName ?? string.Empty );
            this._traceListener = new LoggerTraceListener<Vxi11DiscovererBroadcastTests>( Logger );
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
    public static ILogger<Vxi11DiscovererBroadcastTests>? Logger { get; } = LoggerProvider.InitLogger<Vxi11DiscovererBroadcastTests>();

    #endregion

    #region " ONC RPC Embedded port map and server stub base event handlers "

    /// <summary>   Gets or sets the discover timeout. Overrides the default longer (100) timeout. </summary>
    /// <value> The discover timeout. </value>
    public static int DiscoverTimeout { get; set; } = 4;

    private static OncRpcEmbeddedPortmapServiceStub? _embeddedPortMapService;

    /// <summary>   Handles the thread exception event. </summary>
    /// <remarks>   2023-06-01. </remarks>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    internal static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger?.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
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

    #region " discover devices "

    /// <summary>   (Unit Test Method) device explorer should list devices endpoints. </summary>
    /// <remarks>   
    /// <code>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///   2023-02-04 18:52:33.633,cc.isr.VXI11.MSTest.DeviceExplorerDiscoverTests.DeviceExplorerDiscoverTests
    ///   2023-02-04 18:52:33.637,Enumerating hosts:
    ///   2023-02-04 18:52:37.980, Starting the embedded Portmap service
    ///   2023-02-04 18:52:37.981, Checking for Portmap service
    ///   2023-02-04 18:52:37.999, No Portmap service available.
    ///   2023-02-04 18:52:37.999,Creating embedded Portmap instance
    ///   2023-02-04 18:52:38.211, Portmap service started; checked 18.1 ms.
    ///   2023-02-04 18:52:38.212,OncRpcEmbeddedPortmapServiceStub started in 231.9 ms
    ///   2023-02-04 18:52:38.216,DeviceExplorerShouldListDevicesEndpoints using timeout of 4 ms.
    ///   2023-02-04 18:52:38.217,ListCoreDevicesEndpoints scanning 254 addresses at 192.168.4.255
    ///   2023-02-04 18:52:43.604,ListCoreDevicesEndpoints scanning 254 addresses at 192.168.0.255
    ///   2023-02-04 18:52:48.881,DeviceExplorer.ListCoreDevicesEndpoints found 3 Core VXI-11 device( s) in 10665.1 ms:
    ///   
    ///   2023-02-04 18:52:48.916,192.168.0.144:1024: KEITHLEY INSTRUMENTS, MODEL DMM7510,04051720,1.7.7b
    ///   
    ///   2023-02-04 18:52:48.928,192.168.0.152:1024: KEITHLEY INSTRUMENTS, MODEL 2450,01419966,1.6.4c
    ///   
    ///   2023-02-04 18:52:48.958,192.168.0.154:1024: KEITHLEY INSTRUMENTS, MODEL DAQ6510,04388991,0.0.03i
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "discover" )]
    public void DeviceExplorerShouldListDevicesEndpoints()
    {
        Logger?.LogInformation( @$"{nameof( DeviceExplorerShouldListDevicesEndpoints )} using timeout of {Vxi11DiscovererBroadcastTests.DiscoverTimeout} ms." );
        DateTime startTime = DateTime.Now;
        Stopwatch sw = Stopwatch.StartNew();
        List<IPEndPoint>? endpoints = VXI11.Vxi11Discoverer.ListCoreDevicesEndpoints( IPAddress.Any, Vxi11DiscovererBroadcastTests.DiscoverTimeout, false );
        Assert.IsNotNull( endpoints );

        Logger?.LogInformation(
            $"{nameof( VXI11.Vxi11Discoverer )}.{nameof( VXI11.Vxi11Discoverer.ListCoreDevicesEndpoints )} found {endpoints.Count} Core VXI-11 device(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms:\n" );

        //var s = string.Join( Environment.NewLine, endpoints.Select( x => $"{x}" ).ToArray() );
        //Console.WriteLine( s );

        foreach ( IPEndPoint endpoint in endpoints )
        {
            Logger?.LogInformation( $"{endpoint}: {Vxi11DiscovererTests.TryQueryIdentity( endpoint.Address.ToString() )}" );
        }

        Assert.AreEqual( Vxi11DiscovererTests.PingedHosts.Count, endpoints.Count, "Device count is expected to equal pinged hosts count." );

    }

    /// <summary>   (Unit Test Method) device explorer should list devices addresses. </summary>
    /// <remarks>   
    /// <code>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///   2023-02-04 18:51:20.986,cc.isr.VXI11.MSTest.DeviceExplorerDiscoverTests.DeviceExplorerDiscoverTests
    ///   2023-02-04 18:51:20.990,Enumerating hosts:
    ///   2023-02-04 18:51:25.497, Starting the embedded Portmap service
    ///   2023-02-04 18:51:25.499, Checking for Portmap service
    ///   2023-02-04 18:51:25.531, No Portmap service available.
    ///   2023-02-04 18:51:25.532,Creating embedded Portmap instance
    ///   2023-02-04 18:51:25.742, Portmap service started; checked 32.7 ms.
    ///   2023-02-04 18:51:25.742,OncRpcEmbeddedPortmapServiceStub started in 244.5 ms
    ///   2023-02-04 18:51:25.746,DeviceExplorerShouldListDevicesAddresses using timeout of 4 ms.
    ///   2023-02-04 18:51:25.748,ListCoreDevicesAddresses scanning 254 addresses at 192.168.4.255
    ///   2023-02-04 18:51:31.157,ListCoreDevicesAddresses scanning 254 addresses at 192.168.0.255
    ///   2023-02-04 18:51:36.587,DeviceExplorer.ListCoreDevicesEndpoints found 3 Core VXI-11 device( s) in 10840.1 ms:
    ///
    ///   2023-02-04 18:51:36.623,192.168.0.144: KEITHLEY INSTRUMENTS, MODEL DMM7510,04051720,1.7.7b
    ///
    ///   2023-02-04 18:51:36.633,192.168.0.152: KEITHLEY INSTRUMENTS, MODEL 2450,01419966,1.6.4c
    ///
    ///   2023-02-04 18:51:36.649,192.168.0.154: KEITHLEY INSTRUMENTS, MODEL DAQ6510,04388991,0.0.03i
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "discover" )]
    public void DeviceExplorerShouldListDevicesAddresses()
    {
        Logger?.LogInformation( @$"{nameof( DeviceExplorerShouldListDevicesAddresses )} using timeout of {Vxi11DiscovererBroadcastTests.DiscoverTimeout} ms." );
        DateTime startTime = DateTime.Now;
        Stopwatch sw = Stopwatch.StartNew();
        List<IPAddress>? addresses = VXI11.Vxi11Discoverer.ListCoreDevicesAddresses( IPAddress.Any, Vxi11DiscovererBroadcastTests.DiscoverTimeout, false );
        Assert.IsNotNull( addresses );

        Logger?.LogInformation( $"{nameof( VXI11.Vxi11Discoverer )}.{nameof( VXI11.Vxi11Discoverer.ListCoreDevicesEndpoints )} found {addresses.Count} Core VXI-11 device(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms:\n" );

        //var s = string.Join( Environment.NewLine, addresses.Select( x => $"{x}" ).ToArray() );
        //Console.WriteLine( s );

        foreach ( IPAddress ip in addresses )
        {
            Logger?.LogInformation( $"{ip}: {Vxi11DiscovererTests.TryQueryIdentity( ip.ToString() )}" );
        }


        Assert.AreEqual( Vxi11DiscovererTests.PingedHosts.Count, addresses.Count, "Device count is expected to equal pinged hosts count." );

    }

    #endregion

    #region " registered servers "

    /// <summary>   Assert registered servers should ping. </summary>
    /// <param name="endpoints">    The endpoints. </param>
    public static void AssertRegisteredServersShouldPing( IEnumerable<IPEndPoint> endpoints )
    {

        int actualCount = 0;
        foreach ( IPEndPoint endpoint in endpoints )
        {
            Logger?.LogInformation( $"Pinging {endpoint}" );
            Stopwatch sw = Stopwatch.StartNew();
            Assert.IsTrue( VXI11.Vxi11Discoverer.Paping( endpoint ) );
            Logger?.LogInformation( $"{endpoint} port pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms" );

            if ( endpoint.Port == OncRpcPortmapConstants.OncRpcPortmapPortNumber ) { actualCount++; }

            if ( endpoint.Port != OncRpcPortmapConstants.OncRpcPortmapPortNumber )
                Logger?.LogInformation( $"{endpoint}: {Vxi11DiscovererTests.TryQueryIdentity( endpoint.Address.ToString() )}" );
        }

        int expectedCount = 0;
        foreach ( var host in Vxi11DiscovererTests.PingedHosts )
        {
            // count only hosts that are know to respond to the enumeration of registered servers
            if ( !Vxi11DiscovererTests.NonRespondingHosts().Contains( host.ToString() ) )
            {
                expectedCount++;
            }
        }
        Assert.AreEqual( expectedCount, actualCount, $"the number of registered servers must match the expected list or responders" );

    }

    /// <summary>
    /// (Unit Test Method) device explorer should list local registered servers.
    /// </summary>
    /// <remarks>
    /// Notice that the 2450 is not registered. It takes about 9 ms to query the instrument identity.
    /// <code>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///   2023-02-04 18:53:43.054,cc.isr.VXI11.MSTest.DeviceExplorerDiscoverTests.DeviceExplorerDiscoverTests
    ///   2023-02-04 18:53:43.057,Enumerating hosts:
    ///   2023-02-04 18:53:47.500, Starting the embedded Portmap service
    ///   2023-02-04 18:53:47.502, Checking for Portmap service
    ///   2023-02-04 18:53:47.534, No Portmap service available.
    ///   2023-02-04 18:53:47.534,Creating embedded Portmap instance
    ///   2023-02-04 18:53:47.745, Portmap service started; checked 32.3 ms.
    ///   2023-02-04 18:53:47.745,OncRpcEmbeddedPortmapServiceStub started in 244.5 ms
    ///   2023-02-04 18:53:47.749,DeviceExplorerShouldListLocalRegisteredServers using timeout of 4 ms.
    ///   2023-02-04 18:53:47.751,EnumerateRegisteredServers scanning 254 addresses at 192.168.4.255
    ///   2023-02-04 18:53:52.722,EnumerateRegisteredServers scanning 254 addresses at 192.168.0.255
    ///   2023-02-04 18:53:57.974,DeviceExplorer.EnumerateRegisteredServers(IPAddress.Any ) found 4 VXI-11 registered servers( s) in 10224.4 ms:
    ///   
    ///   192.168.0.144:111
    ///   192.168.0.144:1024
    ///   192.168.0.154:111
    ///   192.168.0.154:1024
    ///   2023-02-04 18:53:57.975,Pinging 192.168.0.144:111
    ///   2023-02-04 18:53:57.976,192.168.0.144:111 port pinged in 1.0 ms
    ///   2023-02-04 18:53:57.976,Pinging 192.168.0.144:1024
    ///   2023-02-04 18:53:57.976,192.168.0.144:1024 port pinged in 0.7 ms
    ///   2023-02-04 18:53:58.009,192.168.0.144:1024: KEITHLEY INSTRUMENTS, MODEL DMM7510,04051720,1.7.7b
    ///   
    ///   2023-02-04 18:53:58.010,Pinging 192.168.0.154:111
    ///   2023-02-04 18:53:58.010,192.168.0.154:111 port pinged in 0.7 ms
    ///   2023-02-04 18:53:58.010,Pinging 192.168.0.154:1024
    ///   2023-02-04 18:53:58.011,192.168.0.154:1024 port pinged in 0.6 ms
    ///   2023-02-04 18:53:58.020,192.168.0.154:1024: KEITHLEY INSTRUMENTS, MODEL DAQ6510,04388991,0.0.03i
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "discover" )]
    public void DeviceExplorerShouldListLocalRegisteredServers()
    {
        Logger?.LogInformation(
            $"{nameof( DeviceExplorerShouldListLocalRegisteredServers )} using timeout of {Vxi11DiscovererBroadcastTests.DiscoverTimeout} ms." );
        Stopwatch sw = Stopwatch.StartNew();
        var endpoints = VXI11.Vxi11Discoverer.EnumerateRegisteredServers( IPAddress.Any, Vxi11DiscovererBroadcastTests.DiscoverTimeout, false );
        Assert.IsNotNull( endpoints );

        Logger?.LogInformation(
            $"{nameof( VXI11.Vxi11Discoverer )}.{nameof( VXI11.Vxi11Discoverer.EnumerateRegisteredServers )}( IPAddress.Any ) found {endpoints.Count} VXI-11 registered servers(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms:\n" );
        var s = string.Join( Environment.NewLine, endpoints.Select( x => $"{x}" ).ToArray() );
        Console.WriteLine( s );

        AssertRegisteredServersShouldPing( endpoints );
    }

    #endregion

}
