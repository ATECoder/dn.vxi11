using System.ComponentModel;

using cc.isr.VXI11.Logging;
using cc.isr.VXI11.IEEE488.Mock;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;

namespace cc.isr.VXI11.IEEE488.MSTest;

[TestClass]
public class Ieee488ServerTests
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

            _device = new( Ieee488ServerTests._identity );
            _server = new( _device );

            _server.PropertyChanged += OnServerPropertyChanged;
            _server.ThreadExceptionOccurred += OnThreadException;

            _ = Task.Factory.StartNew( () => {

                Logger.Writer.LogInformation( "starting the embedded port map service; this takes ~3.5 seconds." );
                using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
                epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

                Logger.Writer.LogInformation( "starting the server task; this takes ~2.5 seconds." );
                _server.Run();
            } ).ContinueWith( failedTask => Ieee488ServerTests.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception! ) ),
                                                                                 TaskContinuationOptions.OnlyOnFaulted );

            Logger.Writer.LogInformation( $"{nameof( Ieee488SingleClientMockServer )} waiting running {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * Ieee488ServerTests.ServerStartTimeTypical, Ieee488ServerTests.ServerStartLoopDelay ) )
                throw new InvalidOperationException( "failed starting the ONC/RPC server." );

            Logger.Writer.LogInformation( $"{nameof( Ieee488SingleClientMockServer )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
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
        Ieee488SingleClientMockServer? server = _server;
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
                Logger.Writer.LogError( _identity, ex );
            }
            finally
            {
                _server = null;
            }
        }
        _classTestContext = null;
    }

    private static readonly string? _ipv4Address = "127.0.0.1";

    private static readonly string _identity = "Ieee488 mock device";
    private static Ieee488SingleClientMockServer? _server;
    private static Ieee488Device? _device;

    internal static void OnThreadException( ThreadExceptionEventArgs e )
    {
        Logger.Writer.LogError( $"An exception occurred during an asynchronous operation", e.Exception );
    }

    internal static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is Ieee488SingleClientMockServer ) name = nameof( Ieee488SingleClientMockServer );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is not Ieee488SingleClientMockServer ) { return; }
        switch ( e.PropertyName )
        {
            case nameof( Ieee488SingleClientMockServer.ReadMessage ):
                Logger.Writer.LogInformation( ( ( Ieee488SingleClientMockServer ) sender).ReadMessage );
                break;
            case nameof( Ieee488SingleClientMockServer.WriteMessage ):
                Logger.Writer.LogInformation( (( Ieee488SingleClientMockServer ) sender).WriteMessage );
                break;
            case nameof( Ieee488SingleClientMockServer.PortNumber ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Ieee488SingleClientMockServer ) sender).PortNumber}" );
                break;
            case nameof( Ieee488SingleClientMockServer.IPv4Address ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Ieee488SingleClientMockServer ) sender).IPv4Address}" );
                break;
            case nameof( Ieee488SingleClientMockServer.Running ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Ieee488SingleClientMockServer ) sender).Running}" );
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
        using Ieee488Client ieee488Client = new();
        ieee488Client.ThreadExceptionOccurred += OnThreadException;

        string identity = Ieee488ServerTests._identity;
        string command = "*IDN?";
        ieee488Client.Connect( ipv4Address, "inst0" );

        int count = repeatCount;
        while ( repeatCount > 0 )
        {
            repeatCount--;
            (_, string response) = ieee488Client.Query( $"{command}\n", 0 );
            Assert.AreEqual( identity, response, $"@count = {count - repeatCount}" );
        }

    }

    /// <summary>   (Unit Test Method) identity should query. </summary>
    [TestMethod]
    public void IdentityShouldQuery()
    {
        int count = 1;
        AssertIdentityShouldQuery( _ipv4Address!, count );
    }
}
