using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device explorer tests. </summary>
/// <remarks>   2023-01-16. </remarks>
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
            System.Diagnostics.Debug.WriteLine( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
            _classTestContext = context;
            Console.WriteLine( @$"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} starting {_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );
            DeviceExplorerTests.EnumerateHosts();
        }
        catch ( Exception ex )
        {
            Console.WriteLine( $"Failed initializing fixture: \n{ex} " );
            CleanupFixture();
        }
    }

    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    /// <summary>   Cleanup fixture. </summary>
    [ClassCleanup]
    public static void CleanupFixture()
    {
    }

    #endregion

    #region " helpers "

    /// <summary>   Gets the host. </summary>
    /// <remarks>   2023-01-16. </remarks>
    /// <returns>   The host. </returns>
    internal static IPAddress? GetHost ()
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
                                                "TCPIP0::192.168.0.134::inst0::INSTR",
                                                "TCPIP0::192.168.0.136::inst0::INSTR",
                                                "TCPIP0::192.168.0.146::inst0::INSTR",
                                                "TCPIP0::192.168.0.148::inst0::INSTR",
                                                "TCPIP0::192.168.0.150::inst0::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,4::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,7::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,15::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,22::INSTR",
                                                "TCPIP0::192.168.0.254::gpib0,24::INSTR",
                                                "TCPIP0::192.168.0.132::inst0::INSTR",
                                                "TCPIP0::192.168.0.144::inst0::INSTR",
                                                "TCPIP0::192.168.0.152::inst0::INSTR",
                                                "TCPIP0::192.168.1.100::[USB0::0x05E6::0x7510::04051720::0]INSTR",
                                                ""};

    private static readonly string[] _hosts = new string[] {
                                    "192.168.0.50",
                                    "192.168.0.134",
                                    "192.168.0.136",
                                    "192.168.0.146",
                                    "192.168.0.148",
                                    "192.168.0.150",
                                    "192.168.1.100",
                                    "192.168.0.132",
                                    "192.168.0.254",
                                    "192.168.0.144",
                                    "192.168.0.152",
                                    "" };

    private static readonly List<IPAddress> _pingedHosts = new();


    /// <summary>   Adds a host if ping portmap service to 'timeout'. </summary>
    /// <remarks> This causes an issue with subsequent port map operations. </remarks>
    /// <param name="host">     The host. </param>
    /// <param name="timeout">  The timeout. </param>
    private static void AddHostIfPingPortmapService( string host, int timeout )
    {
        Stopwatch sw = Stopwatch.StartNew();
        Console.WriteLine( $"Portmap ping {host}" );
        if ( DeviceExplorer.PortmapPingHost( IPAddress.Parse( host ), timeout ) )
        {
            _pingedHosts.Add( IPAddress.Parse( host ) );
            Console.WriteLine( $"added {host} in {sw.ElapsedMilliseconds:0} ms." );
        }
    }

    /// <summary>   Enumerate hosts. </summary>
    /// <remarks>   2023-01-16. </remarks>
    public static void EnumerateHosts()
    {
        Console.WriteLine( $"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} enumerating hosts: " );
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
                if ( failed ) Console.WriteLine( $"Exception pinging {host}" );
            }
        }
    }

    /// <summary>   (Unit Test Method) the device explorer should ping the hosts using the portmap service. </summary>
    /// <remarks>   2023-01-16. </remarks>
    [TestMethod]
    public void DeviceExplorerShouldPingHosts()
    {
        Console.WriteLine( $"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} pinging Portmap service: " );
        foreach ( IPAddress host in _pingedHosts )
        {
            if ( DeviceExplorer.PingHost( host, 10 ) )
            {
                Console.Write( $"Pinging {host}" );
                Stopwatch sw = Stopwatch.StartNew();
                Assert.IsTrue( DeviceExplorer.PortmapPingHost( host, 10 ), $"port map at {host} should reply to a ping" );
                Console.WriteLine( $"; done in {sw.ElapsedMilliseconds:0} ms." );
            }
            else
            {
                Console.WriteLine( $"Pinged host {host} not found" );
            }
        }
    }

    #endregion

    #region " devices "

   /// <summary>    (Unit Test Method) device explorer should list devices. </summary>
   /// <remarks>    2023-01-16. </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
   public void DeviceExplorerShouldListDevices()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var devices = DeviceExplorer.ListCoreDevices( _pingedHosts, 100 );
        Assert.IsNotNull( devices );
        Console.WriteLine( @$"{nameof( DeviceExplorer)}.{nameof( DeviceExplorer.ListCoreDevices )} found {devices.Count} Core VXI-11 device(s) in {sw.ElapsedMilliseconds:0} ms:" );

        foreach ( (IPAddress address, int port) in devices )
        {
            Console.Write( $"Pinging {address}:{port}" );
            sw.Start();
            Assert.IsTrue ( DeviceExplorer.Paping( address, port ) );
            Console.Write( $"; in {sw.ElapsedMilliseconds:0} ms" );
        }
    }

    #endregion

    #region " servers "

    /// <summary>   (Unit Test Method) device explorer should list registered servers. </summary>
    /// <remarks>   2023-01-16. </remarks>
    [TestMethod]
    [TestCategory( "192.168.0.xxx" )]
    public void DeviceExplorerShouldListRegisteredServers()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var servers = DeviceExplorer.EnumerateRegisteredServers( _pingedHosts, 100 );
        Assert.IsNotNull( servers );
        Console.WriteLine( @$"{nameof( DeviceExplorer)}.{nameof( DeviceExplorer.EnumerateRegisteredServers )} found {servers.Count} VXI-11 registered servers(s) in {sw.ElapsedMilliseconds:0} ms:" );

        foreach ( (IPAddress address, int port) in servers )
        {
            Console.Write( $"Pinging {address}:{port}" );
            sw.Start();
            Assert.IsTrue( DeviceExplorer.Paping( address, port ) );
            Console.WriteLine( $"; in {sw.ElapsedMilliseconds:0} ms" );
        }
    }

    #endregion

}
