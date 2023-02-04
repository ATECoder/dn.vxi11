using System.Diagnostics;
using System.Net;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device discover tests. </summary>
[TestClass]
public class DeviceExplorerDiscover
{

    #region " fixture construction and cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
            DeviceExplorerTests.EnumerateHosts();

            Logger.Writer.LogInformation( $"Starting the embedded Portmap service" );
            Stopwatch sw = Stopwatch.StartNew();
            _embeddedPortMapService = DeviceExplorer.StartEmbeddedPortmapService();
            _embeddedPortMapService.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

            Logger.Writer.LogInformation( $"{nameof( OncRpcEmbeddedPortmapServiceStub )} started in {sw.Elapsed.TotalMilliseconds:0.0} ms" );
        }
        catch ( Exception ex )
        {
            Logger.Writer.LogMemberError( $"Failed initializing fixture:", ex );
            CleanupFixture();
        }
    }

    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
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
                Logger.Writer.LogError( "Exception cleaning up fixture", ex );
            }
            finally
            {
                _embeddedPortMapService = null;
                _classTestContext = null;
            }
        }
    }

    private static OncRpcEmbeddedPortmapServiceStub? _embeddedPortMapService;

    internal static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }

    #endregion

    #region " discover devices "

    /// <summary>   (Unit Test Method) device explorer should list devices endpoints. </summary>
    /// <remarks>   
    /// <code>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///   2023-02-02 18:42:51.447,cc.isr.VXI11.MSTest.DeviceExplorerDiscover.DeviceExplorerDiscover
    ///   2023-02-02 18:42:51.450,Enumerating hosts:
    ///   2023-02-02 18:42:55.502, Starting the embedded Portmap service
    ///   2023-02-02 18:42:55.503, Checking for Portmap service
    ///   2023-02-02 18:42:55.613, No Portmap service available.
    ///   2023-02-02 18:42:55.613,Creating embedded Portmap instance
    ///   2023-02-02 18:42:55.840, Portmap service started; checked 109.7 ms.
    ///   2023-02-02 18:42:55.840,OncRpcEmbeddedPortmapServiceStub started in 337.9 ms
    ///   2023-02-02 18:42:55.846,ListCoreDevicesEndpoints scanning 254 addresses at 192.168.4.255
    ///   2023-02-02 18:43:24.033, ListCoreDevicesEndpoints scanning 254 addresses at 192.168.0.255
    ///   2023-02-02 18:43:51.909, DeviceExplorer.ListCoreDevicesEndpoints found 3 Core VXI-11 device( s) in 56059.1 ms:
    ///   192.168.0.144:1024
    ///   192.168.0.152:1024
    ///   192.168.0.154:1024
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "discover" )]
    public void DeviceExplorerShouldListDevicesEndpoints()
    {
        DateTime startTime = DateTime.Now;
        Stopwatch sw = Stopwatch.StartNew();
        List<IPEndPoint>? endpoints = DeviceExplorer.ListCoreDevicesEndpoints( IPAddress.Any, 100, false );
        Assert.IsNotNull( endpoints );

        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.ListCoreDevicesEndpoints )} found {endpoints.Count} Core VXI-11 device(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms:" );

        var s = string.Join( Environment.NewLine, endpoints.Select( x => $"{x}" ).ToArray() );
        Console.WriteLine( s );

        Assert.AreEqual( DeviceExplorerTests.PingedHosts.Count, endpoints.Count, "Device count is expected to equal pinged hosts count." );

    }

    /// <summary>   (Unit Test Method) device explorer should list devices addresses. </summary>
    /// <remarks>   
    /// <code>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///    2023-02-02 18:27:21.365,cc.isr.VXI11.MSTest.DeviceExplorerDiscover.DeviceExplorerDiscover
    ///    2023-02-02 18:27:21.368,Enumerating hosts:
    ///    2023-02-02 18:27:25.398, Starting the embedded Portmap service
    ///    2023-02-02 18:27:25.400, Checking for Portmap service
    ///    2023-02-02 18:27:25.508, No Portmap service available.
    ///    2023-02-02 18:27:25.509,Creating embedded Portmap instance
    ///    2023-02-02 18:27:25.719, Portmap service started; checked 108.3 ms.
    ///    2023-02-02 18:27:25.719,OncRpcEmbeddedPortmapServiceStub started in 320.7 ms
    ///    2023-02-02 18:27:25.725,ListCoreDevicesAddresses scanning 254 addresses at 192.168.4.255
    ///    2023-02-02 18:27:53.865, ListCoreDevicesAddresses scanning 254 addresses at 192.168.0.255
    ///    2023-02-02 18:28:21.677, DeviceExplorer.ListCoreDevicesEndpoints found 3 Core VXI-11 device( s) in 55945.7 ms:
    ///    192.168.0.144
    ///    192.168.0.152
    ///    192.168.0.154
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "discover" )]
    public void DeviceExplorerShouldListDevicesAddresses()
    {
        DateTime startTime = DateTime.Now;
        Stopwatch sw = Stopwatch.StartNew();
        List<IPAddress>? addresses = DeviceExplorer.ListCoreDevicesAddresses( IPAddress.Any, 100, false );
        Assert.IsNotNull( addresses );

        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.ListCoreDevicesEndpoints )} found {addresses.Count} Core VXI-11 device(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms:" );

        var s = string.Join( Environment.NewLine, addresses.Select( x => $"{x}" ).ToArray() );
        Console.WriteLine( s );

        Assert.AreEqual( DeviceExplorerTests.PingedHosts.Count, addresses.Count, "Device count is expected to equal pinged hosts count." );

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
            Logger.Writer.LogInformation( $"Pinging {endpoint}" );
            Stopwatch sw = Stopwatch.StartNew();
            Assert.IsTrue( DeviceExplorer.Paping( endpoint ) );
            Logger.Writer.LogInformation( $"{endpoint} port pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms" );

            if ( endpoint.Port == 111 ) { actualCount++; }
        }

        int expectedCount = 0;
        foreach ( var host in DeviceExplorerTests.PingedHosts )
        {
            // count only hosts that are know to respond to the enumeration of registered servers
            if ( !DeviceExplorerTests.NonRespondingHosts().Contains( host.ToString() ) )
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
    /// Notice that the 2450 is not registered.
    /// <code>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///    2023-02-02 19:22:30.287,cc.isr.VXI11.MSTest.DeviceExplorerDiscover.DeviceExplorerDiscover
    ///    2023-02-02 19:22:30.290,Enumerating hosts:
    ///    2023-02-02 19:22:34.670,Starting the embedded Portmap service
    ///    2023-02-02 19:22:34.672,Checking for Portmap service
    ///    2023-02-02 19:22:34.781,No Portmap service available.
    ///    2023-02-02 19:22:34.781,Creating embedded Portmap instance
    ///    2023-02-02 19:22:34.994,Portmap service started; checked 108.9 ms.
    ///    2023-02-02 19:22:34.994,OncRpcEmbeddedPortmapServiceStub started in 323.9 ms
    ///    2023-02-02 19:22:35.001,EnumerateRegisteredServers scanning 254 addresses at 192.168.4.255
    ///    2023-02-02 19:22:40.346,EnumerateRegisteredServers scanning 254 addresses at 192.168.0.255
    ///    2023-02-02 19:22:45.425,DeviceExplorer.EnumerateRegisteredServers( IPAddress.Any ) found 4 VXI-11 registered servers( s) in 10425.3 ms:
    ///    192.168.0.144:111
    ///    192.168.0.144:1024
    ///    192.168.0.154:111
    ///    192.168.0.154:1024
    ///    2023-02-02 19:22:45.426,Pinging 192.168.0.144:111
    ///    2023-02-02 19:22:45.427,192.168.0.144:111 port pinged in 1.1 ms
    ///    2023-02-02 19:22:45.427,Pinging 192.168.0.144:1024
    ///    2023-02-02 19:22:45.428,192.168.0.144:1024 port pinged in 0.7 ms
    ///    2023-02-02 19:22:45.428,Pinging 192.168.0.154:111
    ///    2023-02-02 19:22:45.428,192.168.0.154:111 port pinged in 0.6 ms
    ///    2023-02-02 19:22:45.428,Pinging 192.168.0.154:1024
    ///    2023-02-02 19:22:45.429,192.168.0.154:1024 port pinged in 0.5 ms
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "discover" )]
    public void DeviceExplorerShouldListLocalRegisteredServers()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var endpoints = DeviceExplorer.EnumerateRegisteredServers( IPAddress.Any, 100, false );
        Assert.IsNotNull( endpoints );

        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.EnumerateRegisteredServers )}( IPAddress.Any ) found {endpoints.Count} VXI-11 registered servers(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms:" );
        var s = string.Join( Environment.NewLine, endpoints.Select( x => $"{x}" ).ToArray() );
        Console.WriteLine( s );

        AssertRegisteredServersShouldPing( endpoints );
    }

    #endregion

}
