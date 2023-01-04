using System.ComponentModel;

namespace cc.isr.VXI11.IEEE488.MSTest;

[TestClass]
public class Ieee488ServerTests
{

    [ClassInitialize]
    public static void InitializeFixture( TestContext context )
    {
        try
        {
            System.Diagnostics.Debug.WriteLine( $"{context.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name} Tester" );
            _classTestContext = context;
            System.Diagnostics.Debug.WriteLine( $"{_classTestContext.FullyQualifiedTestClassName}.{System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name} Tester" );
            _device = new( Ieee488ServerTests._identity);
            _server = new( _device ) {
                Listening = false
            };
            _server.PropertyChanged += OnServerPropertyChanged;
            _ = Task.Factory.StartNew( () => {
                Console.WriteLine( "starting the server task; this takes ~11 seconds..." );
                _server.Run();
            } );

            Console.WriteLine( $"{nameof( Ieee488Server )} waiting listening {DateTime.Now:ss.fff}" );
            // wait till the server is running.
            do
            {
                System.Threading.Thread.Sleep( 500 );
            }
            while ( !_server.Listening );
            Console.WriteLine( $"{nameof( Ieee488Server )} is {(_server.Listening ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
        }
        catch ( Exception ex )
        {
            Console.WriteLine( ex.ToString() );
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
            if ( _server.Listening)
            {
                _server.StopRpcProcessing();
            }
            _server = null;
        }
    }

    private static readonly string? _ipv4Address = "127.0.0.1";

    private static readonly string _identity = "Ieee488 mock device";
    private static Ieee488Server? _server;
    private static Ieee488Device? _device;

    private static void OnServerPropertyChanged( object? sender, PropertyChangedEventArgs args )
    {
        switch ( args.PropertyName )
        {
            case nameof( Ieee488Server.ReadMessage ):
                Console.WriteLine( _server?.ReadMessage );
                break;
            case nameof( Ieee488Server.WriteMessage ):
                Console.WriteLine( _server?.WriteMessage );
                break;
            case nameof( Ieee488Server.PortNumber ):
                Console.WriteLine( $"{args.PropertyName} set to {_server?.PortNumber}" );
                break;
            case nameof( Ieee488Server.IPv4Address ):
                Console.WriteLine( $"{args.PropertyName} set to {_server?.IPv4Address}" );
                break;
            case nameof( Ieee488Server.Listening ):
                Console.WriteLine( $"{args.PropertyName} set to {_server?.Listening}" );
                break;
        }
    }

    /// <summary>   (Unit Test Method) server should listen. </summary>
    /// <remarks>   2022-12-24. </remarks>
    [TestMethod]
    public void ServerShouldListen()
    {
        Assert.IsTrue( _server?.Listening );
    }

    /// <summary>   Assert identity should query. </summary>
    /// <remarks>   2022-11-16. </remarks>
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

        // presently, the IEEE488 Client throws an exception when closing against the Mock server 
        // ieee488Client.Close();
    }

    /// <summary>   (Unit Test Method) identity should query. </summary>
    /// <remarks>   2022-11-16. </remarks>
    [TestMethod]
    public void IdentityShouldQuery()
    {
        int count = 1;
        AssertIdentityShouldQuery(_ipv4Address!, count );
    }
}
