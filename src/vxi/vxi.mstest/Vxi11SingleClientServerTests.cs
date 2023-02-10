using System.ComponentModel;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Logging;
using cc.isr.VXI11.Server;

namespace cc.isr.VXI11.MSTest;

[TestClass]
public class Vxi11SingleClientServerTests
{

    /// <summary>   Gets or sets the server start time typical. </summary>
    /// <value> The server start time typical. </value>
    public static int ServerStartTimeTypical { get; set; } = 3500;

    /// <summary>   Gets or sets the server start loop delay. </summary>
    /// <value> The server start loop delay. </value>
    public static int ServerStartLoopDelay { get; set; } = 100;


    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            _classTestContext = context;
            Logger.Writer.LogInformation( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name}" );

            _server = new();

            _server.PropertyChanged += OnServerPropertyChanged;
            _server.ThreadExceptionOccurred += OnThreadException;

            _ = Task.Factory.StartNew( () => {

                Logger.Writer.LogInformation( "starting the embedded port map service; this takes ~3.5 seconds." );
                using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
                epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

                Logger.Writer.LogInformation( "starting the server task; this takes ~2.5 seconds." );
                _server.Run();
            } ).ContinueWith( failedTask => Vxi11SingleClientServerTests.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception! ) ),
                                                                                 TaskContinuationOptions.OnlyOnFaulted );

            Logger.Writer.LogInformation( $"{nameof( Vxi11SingleClientServer )} waiting running {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * Vxi11SingleClientServerTests.ServerStartTimeTypical, Vxi11SingleClientServerTests.ServerStartLoopDelay ) )
                throw new InvalidOperationException( "failed starting the ONC/RPC server." );

            Logger.Writer.LogInformation( $"{nameof( Vxi11SingleClientServer )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
        }
        catch ( Exception ex )
        {
            Logger.Writer.LogMemberError( "Failed initializing fixture:", ex );
            CleanupFixture();
        }
    }

    public TestContext? TestContext { get; set; }

    private static TestContext? _classTestContext;

    [ClassCleanup]
    public static void CleanupFixture()
    {
        Vxi11SingleClientServer? server = _server;
        if ( server is not null )
        {
            try
            {
                server.Dispose();
                server.PropertyChanged -= OnServerPropertyChanged;
                server.ThreadExceptionOccurred -= OnThreadException;
            }
            catch ( Exception ex )
            {
                Logger.Writer.LogError( "Failed cleaning up the fixture" , ex );
            }
            finally
            {
                _server = null;
            }
        }
        _classTestContext = null;
    }

    private static readonly string? _ipv4Address = "127.0.0.1";

    private static Vxi11SingleClientServer? _server;

    internal static void OnThreadException( ThreadExceptionEventArgs e )
    {
        Logger.Writer.LogError( $"An exception occurred during an asynchronous operation", e.Exception );
    }

    internal static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is Vxi11SingleClientServer ) name = nameof( Vxi11SingleClientServer );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is not Vxi11SingleClientServer ) { return; }
        switch ( e.PropertyName )
        {
            case nameof( Vxi11SingleClientServer.PortNumber ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Vxi11SingleClientServer ) sender).PortNumber}" );
                break;
            case nameof( Vxi11SingleClientServer.IPv4Address ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Vxi11SingleClientServer ) sender).IPv4Address}" );
                break;
            case nameof( Vxi11SingleClientServer.Running ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Vxi11SingleClientServer ) sender).Running}" );
                break;
        }
    }

    /// <summary>   (Unit Test Method) server should listen. </summary>
    [TestMethod]
    public void ServerShouldListen()
    {
        Assert.IsTrue( _server?.Running );
    }

    /// <summary>   Assert identity should query. </summary>
    /// <param name="ipv4Address">  The IPv4 address. </param>
    /// <param name="repeatCount">  Number of repeats. </param>
    private static void AssertIdentityShouldQuery( string ipv4Address, int repeatCount )
    {
        using VXI11.Client.Vxi11Client vxi11Client = new();
        vxi11Client.ThreadExceptionOccurred += OnThreadException;

        string identity = _server!.Device!.Instrument!.Identity;
        string command = Vxi11InstrumentCommands.IDNRead;
        vxi11Client.Connect( ipv4Address,
                             DeviceNameParser.BuildDeviceName( DeviceNameParser.GenericInterfaceFamily, 0 ) );

        int count = repeatCount;
        while ( repeatCount > 0 )
        {
            repeatCount--;
            (_, string response) = vxi11Client.Query( $"{command}\n", 0 );
            Assert.AreEqual( identity, response, $"@count = {count - repeatCount}" );
        }

    }

    /// <summary>   (Unit Test Method) identity should query. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///    2023-02-08 11:54:38.666,cc.isr.VXI11.MSTest.Vxi11SingleClientServerTests.Vxi11SingleClientServerTests
    ///    2023-02-08 11:54:38.677,Vxi11SingleClientServer waiting running 38.677
    ///    2023-02-08 11:54:38.677,starting the embedded port map service; this takes ~3.5 seconds.
    ///    2023-02-08 11:54:38.678,Checking for Portmap service
    ///    2023-02-08 11:54:38.712, No Portmap service available.
    ///    2023-02-08 11:54:38.712,Creating embedded Portmap instance
    ///    2023-02-08 11:54:38.923, Portmap service started; checked 34.4 ms.
    ///    2023-02-08 11:54:38.924,starting the server task; this takes ~2.5 seconds.
    ///    2023-02-08 11:54:38.930,Running set to True
    ///    2023-02-08 11:54:45.770, Vxi11SingleClientServer is running  45.770
    ///    2023-02-08 11:54:45.782,creating link to inst0
    ///    2023-02-08 11:54:45.786, link ID: 1 -> Received：*IDN?
    ///
    ///    2023-02-08 11:54:45.786,Processing '*IDN?'
    ///    2023-02-08 11:54:45.787,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    ///    2023-02-08 11:54:45.804, Running set to False
    /// </code>
    /// </remarks>
    [TestMethod]
    public void IdentityShouldQuery()
    {
        int count = 1;
        AssertIdentityShouldQuery( _ipv4Address!, count );
    }
}
