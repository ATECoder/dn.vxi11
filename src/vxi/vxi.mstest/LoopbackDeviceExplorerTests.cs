using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using cc.isr.ONC.RPC;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device explorer tests. </summary>
/// <remarks>   2023-01-16. </remarks>
[TestClass]
public class LoopbackDeviceExplorerTests
{

    #region " fixture construction and cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="context">  The context. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            System.Diagnostics.Debug.WriteLine( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name} Tester" );
            _classTestContext = context;
            System.Diagnostics.Debug.WriteLine( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name} Tester" );
            Console.WriteLine( $"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} starting the embedded portmap service" );
            Stopwatch sw = Stopwatch.StartNew();
            _embeddedPortMapService = DeviceExplorer.StartEmbeddedPortmapService();
            Console.WriteLine( $"{nameof( OncRpcEmbeddedPortmapService )} started in {sw.ElapsedMilliseconds:0} ms" );
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
        _embeddedPortMapService?.Shutdown();
        _embeddedPortMapService = null;
    }

    private static OncRpcEmbeddedPortmapService? _embeddedPortMapService;

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

    /// <summary>   (Unit Test Method) portmap service should ping. </summary>
    /// <remarks>   2023-01-16. </remarks>
    [TestMethod]
    public void PortmapServiceShouldPing()
    {
        Console.WriteLine( $"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} pinging Portmap service: " );
        Stopwatch sw = Stopwatch.StartNew();
        IPAddress host = IPAddress.Loopback;
        Assert.IsTrue( DeviceExplorer.PortmapPingHost( host, 1000 ), $"port map at {IPAddress.Loopback} should reply to a ping" );
        Console.WriteLine( $"Portmap service pinged {host} in {sw.ElapsedMilliseconds:0} ms." );
    }

    #endregion

}
