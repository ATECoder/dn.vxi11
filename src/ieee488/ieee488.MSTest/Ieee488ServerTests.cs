using System.ComponentModel;
using System.Reflection;

using cc.isr.VXI11.Logging;

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
            _ = Task.Factory.StartNew( () => {
                Logger.Writer.LogInformation( "starting the server task; this takes ~11 seconds..." );
                _server.Run();
            } );

            Logger.Writer.LogInformation( $"{nameof( Ieee488Server )} waiting running {DateTime.Now:ss.fff}" );

            // wait till the server is running.

            _ = _server.ServerStarted( 2 * Ieee488ServerTests.ServerStartTimeTypical , Ieee488ServerTests.ServerStartLoopDelay );

            Logger.Writer.LogInformation( $"{nameof( Ieee488Server )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
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
        if ( _server is not null )
        {
            if ( _server.Running )
            {
                _server.StopRpcProcessing();
            }
            _server = null;
        }
    }

    /// <summary>   Uses a task to delay execution without blocking the current thread. </summary>
    /// <param name="delayTime">    The delay time. </param>
    private static async void Delay ( int delayTime )
    {
        await Task.Delay( delayTime );
    }

    private static readonly string? _ipv4Address = "127.0.0.1";

    private static readonly string _identity = "Ieee488 mock device";
    private static Ieee488Server? _server;
    private static Ieee488Device? _device;

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( _server is null ) { return; }
        switch ( e.PropertyName )
        {
            case nameof( Ieee488Server.ReadMessage ):
                Logger.Writer.LogInformation( _server.ReadMessage );
                break;
            case nameof( Ieee488Server.WriteMessage ):
                Logger.Writer.LogInformation( _server.WriteMessage );
                break;
            case nameof( Ieee488Server.CorePortNumber ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {_server?.CorePortNumber}" );
                break;
            case nameof( Ieee488Server.IPv4Address ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {_server?.IPv4Address}" );
                break;
            case nameof( Ieee488Server.Running ):
                Logger.Writer.LogInformation( $"{e.PropertyName} set to {_server?.Running}" );
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
        Ieee488Client ieee488Client = new();

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

        // presently, the mock serve does not support the destroy link RPC. 
        // ieee488Client.Close();
    }

    /// <summary>   (Unit Test Method) identity should query. </summary>
    [TestMethod]
    public void IdentityShouldQuery()
    {
        int count = 1;
        AssertIdentityShouldQuery( _ipv4Address!, count );
    }
}
