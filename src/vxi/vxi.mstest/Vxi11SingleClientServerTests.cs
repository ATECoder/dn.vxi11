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
    public static void InitializeFixture( TestContext testContext )
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

            Logger.Writer.LogInformation( $"{nameof( Vxi11Server )} waiting running {DateTime.Now:ss.fff}" );

            // because the initializing task is not awaited, we need to wait for the server to start here.

            if ( !_server.ServerStarted( 2 * Vxi11SingleClientServerTests.ServerStartTimeTypical, Vxi11SingleClientServerTests.ServerStartLoopDelay ) )
                throw new InvalidOperationException( "failed starting the ONC/RPC server." );

            Logger.Writer.LogInformation( $"{nameof( Vxi11Server )} is {(_server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );
        }
        catch ( Exception ex )
        {
            Logger.Writer.LogMemberError( "Failed initializing fixture:", ex );
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
                Logger.Writer.LogError( "Failed cleaning up the fixture", ex );
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

    /// <summary>   Assert identity should query. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="ipv4Address">      The IPv4 address. </param>
    /// <param name="repeatCount">      Number of repeats. </param>
    /// <param name="interfaceNumber">  (Optional) The interface number. </param>
    private static void AssertIdentityShouldQuery( string ipv4Address, int repeatCount, int interfaceNumber = 0 )
    {
        using VXI11.Client.Vxi11Client vxi11Client = new();
        vxi11Client.ThreadExceptionOccurred += OnThreadException;

        string command = Vxi11InstrumentCommands.IDNRead;
        vxi11Client.Connect( ipv4Address, DeviceNameParser.BuildDeviceName( DeviceNameParser.GenericInterfaceFamily, interfaceNumber ) );
        Console.WriteLine();
        Logger.Writer.LogVerbose( $"Querying {vxi11Client.DeviceName} {repeatCount} times" );
        string identity = _server!.Device!.ActiveInstrument!.Identity;
        int count = repeatCount;
        while ( repeatCount > 0 )
        {
            repeatCount--;
            (string response, DeviceErrorCode errorCode, string errorDetails) = vxi11Client.TryQuery( $"{command}\n", 0 );
            Assert.AreEqual( DeviceErrorCode.NoError, errorCode , errorDetails );
            Assert.AreEqual( identity, response, $"@count = {count - repeatCount}" );
        }

    }

    /// <summary>   (Unit Test Method) identity should query. </summary>
    /// <remarks>
    /// <code>
    /// Standard Output: 
    ///    2023-02-08 11:54:38.666,cc.isr.VXI11.MSTest.Vxi11ServerTests.Vxi11ServerTests
    ///    2023-02-08 11:54:38.677,Vxi11Server waiting running 38.677
    ///    2023-02-08 11:54:38.677,starting the embedded port map service; this takes ~3.5 seconds.
    ///    2023-02-08 11:54:38.678,Checking for Portmap service
    ///    2023-02-08 11:54:38.712, No Portmap service available.
    ///    2023-02-08 11:54:38.712,Creating embedded Portmap instance
    ///    2023-02-08 11:54:38.923, Portmap service started; checked 34.4 ms.
    ///    2023-02-08 11:54:38.924,starting the server task; this takes ~2.5 seconds.
    ///    2023-02-08 11:54:38.930,Running set to True
    ///    2023-02-08 11:54:45.770, Vxi11Server is running  45.770
    ///    2023-02-08 11:54:45.782,creating link to inst0
    ///    2023-02-08 11:54:45.786, link ID: 1 -> Received：*IDN?
    ///
    ///    2023-02-08 11:54:45.786,Processing '*IDN?'
    ///    2023-02-08 11:54:45.787,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    ///    2023-02-08 11:54:45.804, Running set to False
    /// </code>
    /// </remarks>
    [TestMethod]
    public void IdentityShouldQueryMultipleTimes()
    {
        int count = 5;
        AssertIdentityShouldQuery( _ipv4Address!, count );
    }


    /// <summary>   (Unit Test Method) identity should query multiple device names. </summary>
    /// <remarks>   2023-02-14.
    /// <code>
    /// Standard Output: 
    /// 2023-02-14 15:54:14.175,cc.isr.VXI11.MSTest.Vxi11SingleClientServerTests.Vxi11SingleClientServerTests
    /// 2023-02-14 15:54:14.187,Vxi11Server waiting running 14.187
    /// 2023-02-14 15:54:14.187,starting the embedded port map service; this takes ~3.5 seconds.
    /// 2023-02-14 15:54:14.188,Checking for Portmap service
    /// 2023-02-14 15:54:14.226, No Portmap service available.
    /// 2023-02-14 15:54:14.226,Creating embedded Portmap instance
    /// 2023-02-14 15:54:14.451, Portmap service started; checked 37.3 ms.
    /// 2023-02-14 15:54:14.452,starting the server task; this takes ~2.5 seconds.
    /// 2023-02-14 15:54:14.458,Running set to True
    /// 2023-02-14 15:54:21.219, Vxi11Server is running  21.219
    /// 2023-02-14 15:54:21.232,creating link to inst0
    /// 2023-02-14 15:54:21.236, Querying inst0 1 times
    /// 2023-02-14 15:54:21.238,link ID: 1 -> Received：*IDN?
    ///
    /// 2023-02-14 15:54:21.238,Processing '*IDN?'
    /// 2023-02-14 15:54:21.241,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    /// 2023-02-14 15:54:21.249, creating link to inst1
    /// 2023-02-14 15:54:21.249,Querying inst1 1 times
    /// 2023-02-14 15:54:21.249,link ID: 2 -> Received：*IDN?
    ///
    /// 2023-02-14 15:54:21.249,Processing '*IDN?'
    /// 2023-02-14 15:54:21.249,Query results： INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434。
    /// 2023-02-14 15:54:21.261, Running set to False
    /// </code>
    /// </remarks>
    [TestMethod]
    public void IdentityShouldQueryMultipleDeviceNames()
    {
        int count = 1;
        AssertIdentityShouldQuery( _ipv4Address!, count, 0 );
        AssertIdentityShouldQuery( _ipv4Address!, count, 1 );
    }

}
