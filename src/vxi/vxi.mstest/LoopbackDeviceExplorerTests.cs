using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using cc.isr.VXI11.Logging;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device explorer tests. </summary>
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
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );

            Logger.Writer.LogInformation( $"starting the embedded portmap service" );
            Stopwatch sw = Stopwatch.StartNew();
            _embeddedPortMapService = DeviceExplorer.StartEmbeddedPortmapService();
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
        _embeddedPortMapService?.Dispose();
        _embeddedPortMapService = null;
    }

    private static OncRpcEmbeddedPortmapServiceStub? _embeddedPortMapService;

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

    /// <summary>   (Unit Test Method) portmap service should ping. </summary>
    [TestMethod]
    public void PortmapServiceShouldPing()
    {
        Logger.Writer.LogInformation( $"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} pinging Portmap service: " );
        Stopwatch sw = Stopwatch.StartNew();
        IPAddress host = IPAddress.Loopback;
        Assert.IsTrue( DeviceExplorer.PortmapPingHost( host ), $"port map at {IPAddress.Loopback} should reply to a ping" );
        Logger.Writer.LogInformation( $"Portmap service pinged {host} in {sw.ElapsedMilliseconds:0} ms." );
    }

    #endregion

}
