using System.Diagnostics;
using System.Net;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device explorer tests. </summary>
[TestClass]
public class Vxi11DiscovererTests
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
            Vxi11DiscovererTests.EnumerateHosts();

            Logger.Writer.LogInformation( $"Starting the embedded Portmap service" );
            Stopwatch sw = Stopwatch.StartNew();
            _embeddedPortMapService = VXI11.Vxi11Discoverer.StartEmbeddedPortmapService();
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

    #region " query device "

    /// <summary>   Queries the instrument identity. </summary>
    /// <remarks>   2023-02-04. </remarks>
    /// <param name="address">  The instrument <see cref="System.Net.Sockets.AddressFamily.InterNetwork"/>
    ///                         (IPv4) address. </param>
    /// <returns>   The identity. </returns>
    public static string TryQueryIdentity( string address )
    {
        return TryQueryIdentity( address, "inst0" );
    }

    /// <summary>   Tries to query the instrument identity. </summary>
    /// <remarks>   2023-02-06. </remarks>
    /// <param name="address">      The instrument network IPv4 address. </param>
    /// <param name="deviceName">   The device name, e.g., inst0 or gpib0,4. </param>
    /// <returns>   The instrument identity. </returns>
    public static string TryQueryIdentity( string address, string deviceName )
    {
        using cc.isr.VXI11.Client.Vxi11Client instrument = new();
        instrument.ThreadExceptionOccurred += OnThreadException;
        instrument.Connect( address, deviceName );
        (string reply, DeviceErrorCode errorCode, string errorDetails) = instrument.TryQueryLine( "*IDN?" );
        return errorCode == DeviceErrorCode.NoError
            ? reply
            : $"unable to query identity from TCPIP::{address}::{deviceName}::INSTR because: {DeviceException.BuildErrorMessage( $"; {errorDetails}", errorCode )}";
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


    /// <summary>   Adds a host if ping Portmap service to 'timeout'. </summary>
    /// <remarks> This causes an issue with subsequent port map operations. </remarks>
    /// <param name="host">     The host. </param>
    /// <param name="timeout">  The timeout in milliseconds. </param>
    private static void AddHostIfPingPortmapService( string host, int timeout )
    {
        Stopwatch sw = Stopwatch.StartNew();
        Logger.Writer.LogInformation( $"Portmap ping {host}" );
        if ( VXI11.Vxi11Discoverer.PortmapPingHost( IPAddress.Parse( host ), timeout ) )
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
                if ( VXI11.Vxi11Discoverer.PingHost( host, 10 ) )
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

    /// <summary>   (Unit Test Method) the device explorer should ping the hosts using the Portmap service. </summary>
    /// <remarks>
    /// 2450 (152) 6510 (154) and 7510 (144) are on.
    /// Standard Output: 
    ///   2023-02-04 19:23:10.667,cc.isr.VXI11.MSTest.DeviceExplorerTests.DeviceExplorerTests
    ///   2023-02-04 19:23:10.671,Enumerating hosts:
    ///   2023-02-04 19:23:15.020,Starting the embedded Portmap service
    ///   2023-02-04 19:23:15.021,Checking for Portmap service
    ///   2023-02-04 19:23:15.053,No Portmap service available.
    ///   2023-02-04 19:23:15.053,Creating embedded Portmap instance
    ///   2023-02-04 19:23:15.264,Portmap service started; checked 32.5 ms.
    ///   2023-02-04 19:23:15.265,OncRpcEmbeddedPortmapServiceStub started in 244.9 ms
    ///   2023-02-04 19:23:15.269,pinging Portmap service:
    ///   
    ///   2023-02-04 19:23:15.270,Pinging 192.168.0.144
    ///   2023-02-04 19:23:15.273,192.168.0.144 portmap pinged in 2.4 ms.
    ///   2023-02-04 19:23:15.315,192.168.0.144: KEITHLEY INSTRUMENTS, MODEL DMM7510,04051720,1.7.7b
    ///   
    ///   2023-02-04 19:23:15.315,Pinging 192.168.0.152
    ///   2023-02-04 19:23:15.316,192.168.0.152 portmap pinged in 0.5 ms.
    ///   2023-02-04 19:23:15.326,192.168.0.152: KEITHLEY INSTRUMENTS, MODEL 2450,01419966,1.6.4c
    ///   
    ///   2023-02-04 19:23:15.327,Pinging 192.168.0.154
    ///   2023-02-04 19:23:15.327,192.168.0.154 portmap pinged in 0.5 ms.
    ///   2023-02-04 19:23:15.356,192.168.0.154: KEITHLEY INSTRUMENTS, MODEL DAQ6510,04388991,0.0.03i
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldPingHosts()
    {
        Logger.Writer.LogInformation( $"pinging Portmap service:\n" );
        foreach ( IPAddress host in PingedHosts )
        {
            if ( VXI11.Vxi11Discoverer.PingHost( host, 10 ) )
            {
                Logger.Writer.LogInformation( $"Pinging {host}" );
                Stopwatch sw = Stopwatch.StartNew();
                Assert.IsTrue( VXI11.Vxi11Discoverer.PortmapPingHost( host, 10 ), $"port map at {host} should reply to a ping" );
                Logger.Writer.LogInformation( $"{host} portmap pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms." );
                Logger.Writer.LogInformation( $"{host}: {Vxi11DiscovererTests.TryQueryIdentity( host.ToString() )}" );
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
    ///   2023-02-04 19:19:59.702,cc.isr.VXI11.MSTest.DeviceExplorerTests.DeviceExplorerTests
    ///   2023-02-04 19:19:59.705,Enumerating hosts:
    ///   2023-02-04 19:20:04.017, Starting the embedded Portmap service
    ///   2023-02-04 19:20:04.019, Checking for Portmap service
    ///   2023-02-04 19:20:04.052, No Portmap service available.
    ///   2023-02-04 19:20:04.052,Creating embedded Portmap instance
    ///   2023-02-04 19:20:04.278, Portmap service started; checked 33.0 ms.
    ///   2023-02-04 19:20:04.278,OncRpcEmbeddedPortmapServiceStub started in 260.6 ms
    ///   2023-02-04 19:20:04.292,DeviceExplorer.ListCoreDevicesEndpoints found 3 Core VXI-11 device( s) in 9.7 ms:
    ///   
    ///   2023-02-04 19:20:04.292,Pinging 192.168.0.144:1024
    ///   2023-02-04 19:20:04.294,192.168.0.144:1024 port pinged in 1.6 ms
    ///   2023-02-04 19:20:04.328,192.168.0.144:1024: KEITHLEY INSTRUMENTS, MODEL DMM7510,04051720,1.7.7b
    ///   
    ///   2023-02-04 19:20:04.328,Pinging 192.168.0.152:1024
    ///   2023-02-04 19:20:04.329,192.168.0.152:1024 port pinged in 0.8 ms
    ///   2023-02-04 19:20:04.339,192.168.0.152:1024: KEITHLEY INSTRUMENTS, MODEL 2450,01419966,1.6.4c
    ///   
    ///   2023-02-04 19:20:04.339,Pinging 192.168.0.154:1024
    ///   2023-02-04 19:20:04.340,192.168.0.154:1024 port pinged in 0.6 ms
    ///   2023-02-04 19:20:04.368,192.168.0.154:1024: KEITHLEY INSTRUMENTS, MODEL DAQ6510,04388991,0.0.03i
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldListPingedDevicesEndpoints()
    {
        Stopwatch sw = Stopwatch.StartNew();

        var devices = VXI11.Vxi11Discoverer.ListCoreDevicesEndpoints( PingedHosts, 100, false, true );
        Assert.IsNotNull( devices );
        Logger.Writer.LogInformation(
            $"{nameof( VXI11.Vxi11Discoverer )}.{nameof( VXI11.Vxi11Discoverer.ListCoreDevicesEndpoints )} found {devices.Count} Core VXI-11 device(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms:\n" );

        foreach ( IPEndPoint endpoint in devices )
        {
            Logger.Writer.LogInformation( $"Pinging {endpoint}" );
            sw.Restart();
            Assert.IsTrue( VXI11.Vxi11Discoverer.Paping( endpoint ) );
            Logger.Writer.LogInformation( $"{endpoint} port pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms" );
            if ( endpoint.Port != OncRpcPortmapConstants.OncRpcPortmapPortNumber )
                Logger.Writer.LogInformation( $"{endpoint}: {Vxi11DiscovererTests.TryQueryIdentity( endpoint.Address.ToString() )}" );
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
            Assert.IsTrue( VXI11.Vxi11Discoverer.Paping( endpoint ) );
            Logger.Writer.LogInformation( $"{endpoint} port pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms\n" );

            if ( endpoint.Port == OncRpcPortmapConstants.OncRpcPortmapPortNumber ) { actualCount++; }
            if ( endpoint.Port != OncRpcPortmapConstants.OncRpcPortmapPortNumber )
                Logger.Writer.LogInformation( $"{endpoint}: {Vxi11DiscovererTests.TryQueryIdentity( endpoint.Address.ToString() )}" );

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
    ///   2023-02-04 19:21:54.634,cc.isr.VXI11.MSTest.DeviceExplorerTests.DeviceExplorerTests
    ///   2023-02-04 19:21:54.638,Enumerating hosts:
    ///   2023-02-04 19:21:59.005, Starting the embedded Portmap service
    ///   2023-02-04 19:21:59.007, Checking for Portmap service
    ///   2023-02-04 19:21:59.039, No Portmap service available.
    ///   2023-02-04 19:21:59.039,Creating embedded Portmap instance
    ///   2023-02-04 19:21:59.264, Portmap service started; checked 32.1 ms.
    ///   2023-02-04 19:21:59.265,OncRpcEmbeddedPortmapServiceStub started in 259.1 ms
    ///   2023-02-04 19:21:59.281,DeviceExplorer.EnumerateRegisteredServers(addresses ) found 4 VXI-11 registered servers( s) in 12.1 ms
    ///   
    ///   2023-02-04 19:21:59.282,Pinging 192.168.0.144:111
    ///   2023-02-04 19:21:59.282,192.168.0.144:111 port pinged in 0.7 ms
    ///   2023-02-04 19:21:59.282,Pinging 192.168.0.144:1024
    ///   2023-02-04 19:21:59.283,192.168.0.144:1024 port pinged in 0.6 ms
    ///   2023-02-04 19:21:59.300,192.168.0.144:1024: KEITHLEY INSTRUMENTS, MODEL DMM7510,04051720,1.7.7b
    ///   
    ///   2023-02-04 19:21:59.300,Pinging 192.168.0.154:111
    ///   2023-02-04 19:21:59.301,192.168.0.154:111 port pinged in 0.7 ms
    ///   2023-02-04 19:21:59.301,Pinging 192.168.0.154:1024
    ///   2023-02-04 19:21:59.302,192.168.0.154:1024 port pinged in 0.6 ms
    ///   2023-02-04 19:21:59.311,192.168.0.154:1024: KEITHLEY INSTRUMENTS, MODEL DAQ6510,04388991,0.0.03i
    /// </code>
    /// </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldListPingedRegisteredServers()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var endpoints = VXI11.Vxi11Discoverer.EnumerateRegisteredServers( PingedHosts, 100, false );
        Assert.IsNotNull( endpoints );
        Logger.Writer.LogInformation( @$"{nameof( VXI11.Vxi11Discoverer )}.{nameof( VXI11.Vxi11Discoverer.EnumerateRegisteredServers )}( addresses ) found {endpoints.Count} VXI-11 registered servers(s) in {sw.Elapsed.TotalMilliseconds:0.0} ms" );

        AssertRegisteredServersShouldPing( endpoints );
    }

    #endregion

}
