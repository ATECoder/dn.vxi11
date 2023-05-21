using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using cc.isr.VXI11.Logging;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.VXI11.MSTest;

/// <summary>   (Unit Test Class) a device explorer tests. </summary>
[TestClass]
public class Vxi11DiscovererLoopbackTests
{

    #region " fixture Construction and Cleanup "

    /// <summary>   Initializes the fixture. </summary>
    /// <param name="testContext"> Gets or sets the test context which provides information about
    /// and functionality for the current test run. </param>
    [ClassInitialize]
    public static void InitializeFixture( TestContext testContext )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );

            Logger.Writer.LogInformation( $"starting the embedded Portmap service" );
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

    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the
    /// current test run.
    /// </summary>
    /// <value> The test context. </value>
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

        Logger.Writer.LogError( $"{name}  encountered an exception during an asynchronous operation", e.Exception );
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
        Logger.Writer.LogInformation( $"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} pinging Portmap service: " );
        Stopwatch sw = Stopwatch.StartNew();
        IPAddress host = IPAddress.Loopback;
        Assert.IsTrue( VXI11.Vxi11Discoverer.PortmapPingHost( host ), $"port map at {IPAddress.Loopback} should reply to a ping" );
        Logger.Writer.LogInformation( $"{host} portmap pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms." );
    }

    #endregion

}
