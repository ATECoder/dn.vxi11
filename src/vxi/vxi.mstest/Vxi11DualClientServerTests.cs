using System.ComponentModel;

using cc.isr.ONC.RPC.Portmap;
using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Logging;
using cc.isr.VXI11.Server;

namespace cc.isr.VXI11.MSTest;

[TestClass]
public class Vxi11DualClientServerTests
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
            } ).ContinueWith( failedTask => Vxi11DualClientServerTests.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception! ) ),
                                                                                 TaskContinuationOptions.OnlyOnFaulted );

            Logger.Writer.LogInformation( $"{nameof( Vxi11Server )} waiting running {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * Vxi11DualClientServerTests.ServerStartTimeTypical, Vxi11DualClientServerTests.ServerStartLoopDelay ) )
                throw new InvalidOperationException( "failed starting the ONC/RPC server." );

            Logger.Writer.LogInformation( $"{nameof( Vxi11Server )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
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
        Vxi11Server? server = _server;
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

    private static Vxi11Server? _server;

    internal static void OnThreadException( ThreadExceptionEventArgs e )
    {
        Logger.Writer.LogError( $"An exception occurred during an asynchronous operation", e.Exception );
    }

    internal static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is Vxi11Server ) name = nameof( Vxi11Server );
        if ( sender is OncRpcServerStubBase ) name = nameof( OncRpcServerStubBase );

        Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is not Vxi11Server ) { return; }
        switch ( e.PropertyName )
        {
            case nameof( Vxi11Server.PortNumber ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Vxi11Server ) sender).PortNumber}" );
                break;
            case nameof( Vxi11Server.IPv4Address ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Vxi11Server ) sender).IPv4Address}" );
                break;
            case nameof( Vxi11Server.Running ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {(( Vxi11Server ) sender).Running}" );
                break;
        }
    }

    /// <summary>   (Unit Test Method) server should listen. </summary>
    [TestMethod]
    public void ServerShouldListen()
    {
        Assert.IsTrue( _server?.Running );
    }

    private static VXI11.Client.Vxi11Client AssertOpenClient( string ipv4Address, int lockTimeout, int interfaceNumber )
    {
        VXI11.Client.Vxi11Client client = new();
        client.LockEnabled = true;
        client.LockTimeout = lockTimeout;
        client.ThreadExceptionOccurred += OnThreadException;
        client.Connect( ipv4Address, DeviceNameParser.BuildDeviceName( DeviceNameParser.GenericInterfaceFamily, interfaceNumber ) );
        return client;
    }

    /// <summary>   Assert identity should query. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="ipv4Address">      The IPv4 address. </param>
    /// <param name="repeatCount">      Number of repeats. </param>
    /// <param name="interfaceNumber">  (Optional) The interface number. </param>
    private static void AssertSecondClientShouldOpenAftertimeout( string ipv4Address )
    {
        using VXI11.Client.Vxi11Client instr0a = AssertOpenClient( ipv4Address, 100, 0 );

        using VXI11.Client.Vxi11Client instr0b = AssertOpenClient( ipv4Address, 100, 0 );
    }

    [TestMethod]
    public void ClientShouldOpenAfterTimeout()
    {
        AssertSecondClientShouldOpenAftertimeout( _ipv4Address! );
    }

}
