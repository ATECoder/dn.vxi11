using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.IEEE488.Mock;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device explorer tests. </summary>
[TestClass]
public class DeviceExplorerTests
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

            Logger.Writer.LogInformation( $"Starting the embedded portmap service" );
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
        if ( sender is Ieee488SingleClientMockServer ) name = nameof( Ieee488SingleClientMockServer );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }


    #endregion

    #region " port map tests "

    private static readonly string[] _visaResourceNames = new string[] {
                                                "TCPIP0::192.168.0.50::inst0::INSTR",
                                                "TCPIP0::192.168.0.132::inst0::INSTR",
                                                "TCPIP0::192.168.0.134::inst0::INSTR",
                                                "TCPIP0::192.168.0.136::inst0::INSTR",
                                                "TCPIP0::192.168.0.144::inst0::INSTR",
                                                "TCPIP0::192.168.0.146::inst0::INSTR",
                                                "TCPIP0::192.168.0.148::inst0::INSTR",
                                                "TCPIP0::192.168.0.150::inst0::INSTR",
                                                "TCPIP0::192.168.0.152::inst0::INSTR",
                                                "TCPIP0::192.168.0.154::inst0::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,4::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,7::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,15::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,22::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,24::INSTR",
                                                "TCPIP0::192.168.1.100::[USB0::0x05E6::0x7510::04051720::0]INSTR",
                                                ""};

    private static readonly string[] _hosts = new string[] {
                                    "192.168.0.50",
                                    "192.168.0.132",
                                    "192.168.0.134",
                                    "192.168.0.136",
                                    "192.168.0.146",
                                    "192.168.0.148",
                                    "192.168.0.150",
                                    "192.168.0.144",
                                    "192.168.0.152",
                                    "192.168.0.154",
                                    "192.168.0.254",
                                    "192.168.1.100",
                                    "" };

    /// <summary>   Non responding hosts. </summary>
    /// <returns>   A string[]. </returns>
    public static string[] NonRespondingHosts()
    {
        return new string[] { "192.168.0.152",
                              "192.168.1.100",
                              "" };
    }


    /// <summary>   Gets the pinged hosts. </summary>
    /// <value> The pinged hosts. </value>
    public static List<IPAddress> PingedHosts { get; } = new();


    /// <summary>   Adds a host if ping portmap service to 'timeout'. </summary>
    /// <remarks> This causes an issue with subsequent port map operations. </remarks>
    /// <param name="host">     The host. </param>
    /// <param name="timeout">  The timeout in milliseconds. </param>
    private static void AddHostIfPingPortmapService( string host, int timeout )
    {
        Stopwatch sw = Stopwatch.StartNew();
        Logger.Writer.LogInformation( $"Portmap ping {host}" );
        if ( DeviceExplorer.PortmapPingHost( IPAddress.Parse( host ), timeout ) )
        {
            PingedHosts.Add( IPAddress.Parse( host ) );
            Logger.Writer.LogInformation( $"Added {host}; portmap pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms." );
        }
    }

    /// <summary>   Enumerate hosts. </summary>
    public static void EnumerateHosts()
    {
        Logger.Writer.LogInformation( $"Enumerating hosts: " );
        foreach ( string host in _hosts )
        {
            if ( string.IsNullOrEmpty( host ) ) continue;
            bool failed = false;
            try
            {
                if ( DeviceExplorer.PingHost( host, 10 ) )
                {
                    PingedHosts.Add( IPAddress.Parse( host ) );
                }
            }
            catch ( Exception )
            {
                failed = true;
                throw;
            }
            finally
            {
                if ( failed ) Logger.Writer.LogMemberWarning( $"Exception pinging {host}" );
            }
        }
    }

    /// <summary>   (Unit Test Method) the device explorer should ping the hosts using the portmap service. </summary>
    /// <remarks>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///   2023-02-02 18:18:06.677,pinging Portmap service:
    ///   2023-02-02 18:18:06.678,Pinging 192.168.0.144
    ///   2023-02-02 18:18:06.678,192.168.0.144 portmap pinged in 0.5 ms.
    ///   2023-02-02 18:18:06.679,Pinging 192.168.0.152
    ///   2023-02-02 18:18:06.679,192.168.0.152 portmap pinged in 0.4 ms.
    ///   2023-02-02 18:18:06.680,Pinging 192.168.0.154
    ///   2023-02-02 18:18:06.680,192.168.0.154 portmap pinged in 0.4 ms.
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldPingHosts()
    {
        Logger.Writer.LogInformation( $"pinging Portmap service: " );
        foreach ( IPAddress host in PingedHosts )
        {
            if ( DeviceExplorer.PingHost( host, 10 ) )
            {
                Logger.Writer.LogInformation( $"Pinging {host}" );
                Stopwatch sw = Stopwatch.StartNew();
                Assert.IsTrue( DeviceExplorer.PortmapPingHost( host, 10 ), $"port map at {host} should reply to a ping" );
                Logger.Writer.LogInformation( $"{host} portmap pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms." );
            }
            else
            {
                Assert.Fail( $"Pinged host {host} not found" );
            }
        }
    }

    #endregion

    #region " list devices "

    /// <summary>    (Unit Test Method) device explorer should list the endpoints of all pinged device addresses. </summary>
    /// <remarks>    
    /// <code>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///    2023-02-02 18:18:02.289,cc.isr.VXI11.MSTest.DeviceExplorerTests.DeviceExplorerTests
    ///    2023-02-02 18:18:02.292,Enumerating hosts:
    ///    2023-02-02 18:18:06.300, Starting the embedded portmap service
    ///    2023-02-02 18:18:06.302, Checking for portmap service
    ///    2023-02-02 18:18:06.411, No portmap service available.
    ///    2023-02-02 18:18:06.411,Creating embedded portmap instance
    ///    2023-02-02 18:18:06.637, Portmap service started; checked 109.4 ms.
    ///    2023-02-02 18:18:06.637,OncRpcEmbeddedPortmapServiceStub started in 336.9 ms
    ///    2023-02-02 18:18:06.651,DeviceExplorer.ListCoreDevicesEndpoints found 3 Core VXI-11 device( s) in 8.9 ms:
    ///    2023-02-02 18:18:06.651,Pinging 192.168.0.144:1024
    ///    2023-02-02 18:18:06.652,192.168.0.144:1024 port pinged in 10.3 ms
    ///    2023-02-02 18:18:06.652,Pinging 192.168.0.152:1024
    ///    2023-02-02 18:18:06.653,192.168.0.152:1024 port pinged in 11.1 ms
    ///    2023-02-02 18:18:06.653,Pinging 192.168.0.154:1024
    ///    2023-02-02 18:18:06.654,192.168.0.154:1024 port pinged in 11.7 ms
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldListPingedDevicesEndpoints()
    {
        Stopwatch sw = Stopwatch.StartNew();

        var devices = DeviceExplorer.ListCoreDevicesEndpoints( PingedHosts, 100,false, true );
        Assert.IsNotNull( devices );
        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.ListCoreDevicesEndpoints )} found {devices.Count} Core VXI-11 device(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms:" );

        foreach ( IPEndPoint endpoint in devices )
        {
            Logger.Writer.LogInformation( $"Pinging {endpoint}" );
            sw.Start();
            Assert.IsTrue( DeviceExplorer.Paping( endpoint ) );
            Logger.Writer.LogInformation( $"{endpoint} port pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms" );
        }
        Assert.AreEqual( PingedHosts.Count, devices.Count, "Device count is expected to equal pinged hosts count." );
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
        foreach ( var host in PingedHosts )
        {
            // count only hosts that are know to respond to the enumeration of registered servers
            if ( !NonRespondingHosts().Contains( host.ToString() ) )
            {
                expectedCount++;
            }
        }
        Assert.AreEqual( expectedCount, actualCount, $"the number of registered servers must match the expected list or responders" );

    }

    /// <summary>   (Unit Test Method) device explorer should list pinged registered servers. </summary>
    /// <remarks> 
    /// Note that the 2450 is not registered.
    /// <code>
    /// 2459=0 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///   2023-02-02 18:18:06.673,DeviceExplorer.EnumerateRegisteredServers(addresses ) found 4 VXI-11 registered servers( s) in 4.2 ms
    ///   2023-02-02 18:18:06.673,Pinging 192.168.0.144:111
    ///   2023-02-02 18:18:06.674,192.168.0.144:111 port pinged in 0.6 ms
    ///   2023-02-02 18:18:06.674,Pinging 192.168.0.144:1024
    ///   2023-02-02 18:18:06.675,192.168.0.144:1024 port pinged in 0.6 ms
    ///   2023-02-02 18:18:06.675,Pinging 192.168.0.154:111
    ///   2023-02-02 18:18:06.675,192.168.0.154:111 port pinged in 0.6 ms
    ///   2023-02-02 18:18:06.675,Pinging 192.168.0.154:1024
    ///   2023-02-02 18:18:06.676,192.168.0.154:1024 port pinged in 0.5 ms
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldListPingedRegisteredServers()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var endpoints = DeviceExplorer.EnumerateRegisteredServers( PingedHosts, 100, false );
        Assert.IsNotNull( endpoints );
        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.EnumerateRegisteredServers )}( addresses ) found {endpoints.Count} VXI-11 registered servers(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms" );

        AssertRegisteredServersShouldPing( endpoints );
    }

    #endregion

}
