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
[TestCategory( "192.168.0.xxx" )]
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

            Logger.Writer.LogInformation( $"starting the embedded portmap service" );
            Stopwatch sw = Stopwatch.StartNew();
            _embeddedPortMapService = DeviceExplorer.StartEmbeddedPortmapService();
            _embeddedPortMapService.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

            Logger.Writer.LogInformation( $"{nameof( OncRpcEmbeddedPortmapServiceStub )} started in {sw.ElapsedMilliseconds:0} ms" );
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

        Logger.Writer.LogError( $"Thread exception occurred at {name} instance", e.Exception );
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

    private static readonly string[] _nonRespondingHosts = new string[] {
                                    "192.168.0.152",
                                    "192.168.1.100",
                                    "" };


    private static readonly List<IPAddress> _pingedHosts = new();


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
            _pingedHosts.Add( IPAddress.Parse( host ) );
            Logger.Writer.LogInformation( $"Added {host}; portmap pinged in {sw.ElapsedMilliseconds:0} ms." );
        }
    }

    /// <summary>   Enumerate hosts. </summary>
    public static void EnumerateHosts()
    {
        Logger.Writer.LogInformation( $"enumerating hosts: " );
        foreach ( string host in _hosts )
        {
            if ( string.IsNullOrEmpty( host ) ) continue;
            bool failed = false;
            try
            {
                if ( DeviceExplorer.PingHost( host, 10 ) )
                {
                    _pingedHosts.Add( IPAddress.Parse( host ) );
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
    /// <list type="bullet">2450 (152), 6510 (154), 7510 (144) are on <item>
    /// 2023:01:20:12:01:08.942 pinging Portmap service: </item><item>
    /// Pinging 192.168.0.144; done in 1 ms. </item><item>
    /// Pinging 192.168.0.152; done in 0 ms. </item><item>
    /// Pinging 192.168.0.154; done in 0 ms. </item></list>
    /// </remarks>
    [TestMethod]
    public void DeviceExplorerShouldPingHosts()
    {
        Logger.Writer.LogInformation( $"pinging Portmap service: " );
        foreach ( IPAddress host in _pingedHosts )
        {
            if ( DeviceExplorer.PingHost( host, 10 ) )
            {
                Logger.Writer.LogInformation( $"Pinging {host}" );
                Stopwatch sw = Stopwatch.StartNew();
                Assert.IsTrue( DeviceExplorer.PortmapPingHost( host, 10 ), $"port map at {host} should reply to a ping" );
                Logger.Writer.LogInformation( $"{host} portmap pinged in {sw.ElapsedMilliseconds:0} ms." );
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
    /// <list type="bullet">2450 (152), 6510 (154), 7510 (144) are on <item>
    /// cc.isr.VXI11.MSTest.DeviceExplorerTests.DeviceExplorerTests </item><item>
    /// 2023:01:20:11:52:01.178 starting cc.isr.VXI11.MSTest.DeviceExplorerTests.DeviceExplorerTests </item><item>
    /// 2023:01:20:11:52:01.182 enumerating hosts: </item><item>
    /// DeviceExplorer.ListCoreDevices found 3 Core VXI-11 device(s) in 11 ms:</item><item>
    /// Pinging 192.168.0.144:1024; in 40 ms </item><item>
    /// Pinging 192.168.0.152:1024; in 55 ms </item><item>
    /// Pinging 192.168.0.154:1024; in 85 ms </item></list>
    /// </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldListPingedDevicesEndpoints()
    {
        Stopwatch sw = Stopwatch.StartNew();

        var devices = DeviceExplorer.ListCoreDevicesEndpoints( _pingedHosts, 100,false, true );
        Assert.IsNotNull( devices );
        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.ListCoreDevicesEndpoints )} found {devices.Count} Core VXI-11 device(s) in {sw.ElapsedMilliseconds:0} ms:" );

        foreach ( IPEndPoint endpoint in devices )
        {
            Logger.Writer.LogInformation( $"Pinging {endpoint}" );
            sw.Start();
            Assert.IsTrue( DeviceExplorer.Paping( endpoint ) );
            Logger.Writer.LogInformation( $"{endpoint} pinged in {sw.ElapsedMilliseconds:0} ms" );
        }
        Assert.AreEqual( _pingedHosts.Count, devices.Count, "Device count is expected to equal pinged hosts count." );
    }

    [TestMethod]
    [TestCategory( "192.168.0.255" )]
    public void DeviceExplorerShouldListDevicesEndpoints()
    {
        DateTime startTime = DateTime.Now;
        Stopwatch sw = Stopwatch.StartNew();
        List<IPEndPoint>? endpoints = DeviceExplorer.ListCoreDevicesEndpoints( IPAddress.Parse( "192.168.0.255"), 100, false );
        Assert.IsNotNull( endpoints );
        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.ListCoreDevicesEndpoints )} found {endpoints.Count} Core VXI-11 device(s) in {sw.ElapsedMilliseconds:0} ms:" );
        Assert.AreEqual( _pingedHosts.Count, endpoints.Count, "Device count is expected to equal pinged hosts count." );
        Logger.Writer.LogInformation( $"It took {(DateTime.Now - startTime).Milliseconds:0} to list {endpoints.Count} endpoints." );
    }

    [TestMethod]
    [TestCategory( "192.168.0.255" )]
    public void DeviceExplorerShouldListDevicesAddresses()
    {
        DateTime startTime = DateTime.Now;
        Stopwatch sw = Stopwatch.StartNew();
        List<IPAddress>? addresses = DeviceExplorer.ListCoreDevicesAddresses( IPAddress.Parse( "192.168.0.255" ), 100, false );
        Assert.IsNotNull( addresses );
        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.ListCoreDevicesEndpoints )} found {addresses.Count} Core VXI-11 device(s) in {sw.ElapsedMilliseconds:0} ms:" );
        Assert.AreEqual( _pingedHosts.Count, addresses.Count, "Device count is expected to equal pinged hosts count." );
        Logger.Writer.LogInformation( $"It took {(DateTime.Now - startTime).Milliseconds:0} to list {addresses.Count} endpoints." );
    }

    #endregion

    #region " servers "

    /// <summary>   (Unit Test Method) device explorer should list registered servers. </summary>
    /// <remarks> 
    /// <list type="bullet">2450 (152), 6510 (154), 7510 (144) are on <item>
    /// DeviceExplorer.EnumerateRegisteredServers found 4 VXI-11 registered servers(s) in 4 ms: </item><item>
    /// Pinging 192.168.0.144:111; in 19 ms </item><item>
    /// Pinging 192.168.0.144:1024; in 44 ms </item><item>
    /// Pinging 192.168.0.154:111; in 60 ms </item><item>
    /// Pinging 192.168.0.154:1024; in 75 ms  </item></list>
    /// </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldListRegisteredServers()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var servers = DeviceExplorer.EnumerateRegisteredServers( _pingedHosts, 100 );
        Assert.IsNotNull( servers );
        Logger.Writer.LogInformation( @$"{nameof( DeviceExplorer )}.{nameof( DeviceExplorer.EnumerateRegisteredServers )} found {servers.Count} VXI-11 registered servers(s) in {sw.ElapsedMilliseconds:0} ms:" );

        int actualCount = 0;
        foreach ( IPEndPoint endpoint in servers )
        {
            Logger.Writer.LogInformation( $"Pinging {endpoint}" );
            sw.Start();
            Assert.IsTrue( DeviceExplorer.Paping( endpoint ) );
            Logger.Writer.LogInformation( $"{endpoint} pinged in {sw.ElapsedMilliseconds:0} ms" );

            if ( endpoint.Port == 111 ) { actualCount++; }
        }

        int expectedCount = 0;
        foreach ( var host in _pingedHosts )
        {
            // count only hosts that are know to respond to the enumeration of registered servers
            if ( !_nonRespondingHosts.Contains( host.ToString() ) )
            {
                expectedCount++;
            }
        }
        Assert.AreEqual( expectedCount, actualCount, $"the number of registered servers must match the expected list or responders" );

    }

    #endregion

}
