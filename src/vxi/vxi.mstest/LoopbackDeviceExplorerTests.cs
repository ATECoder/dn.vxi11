using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using cc.isr.VXI11.Logging;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.IEEE488.Mock;

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

    /// <summary>   (Unit Test Method) portmap service should ping. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///   2023-02-02 20:12:39.826,cc.isr.VXI11.MSTest.LoopbackDeviceExplorerTests.LoopbackDeviceExplorerTests
    ///   2023-02-02 20:12:39.829,starting the embedded portmap service
    ///   2023-02-02 20:12:39.830,Checking for portmap service
    ///   2023-02-02 20:12:39.938, No portmap service available.
    ///   2023-02-02 20:12:39.938,Creating embedded portmap instance
    ///   2023-02-02 20:12:40.160, Portmap service started; checked 107.6 ms.
    ///   2023-02-02 20:12:40.161,OncRpcEmbeddedPortmapServiceStub started in 331.6 ms
    ///   2023-02-02 20:12:40.165,2023:02:02:08:12:40.165 pinging Portmap service:
    ///   2023-02-02 20:12:40.169,127.0.0.1 portmap pinged in 4.1 ms.
    /// </code>
    /// </remarks>
    [TestMethod]
    public void PortmapServiceShouldPing()
    {
        Logger.Writer.LogInformation( $"{DateTime.Now:yyyy:MM:dd:hh:mm:ss.fff} pinging Portmap service: " );
        Stopwatch sw = Stopwatch.StartNew();
        IPAddress host = IPAddress.Loopback;
        Assert.IsTrue( DeviceExplorer.PortmapPingHost( host ), $"port map at {IPAddress.Loopback} should reply to a ping" );
        Logger.Writer.LogInformation( $"{host} portmap pinged in {sw.Elapsed.TotalMilliseconds:0.0} ms." );
    }

    #endregion

}
