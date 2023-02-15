// See https://aka.ms/new-console-template for more information

using cc.isr.ONC.RPC.Logging;
using cc.isr.ONC.RPC.Portmap;

try
{
    using cc.isr.VXI11.Server.Vxi11Server server = new();
    server.ThreadExceptionOccurred += OnThreadExcetion;

    Logger.Writer.LogInformation( "Starting the embedded port map service" );
    using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
    epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadExcetion;

    Logger.Writer.LogInformation( "Starting VXI-11 IEEE 488 Server" );
    _ = server.RunAsync();

    _ = server.ServerStarted( 2 * Constants.ServerStartTimeTypical, Constants.ServerStartLoopDelay );

    Logger.Writer.LogInformation( $"{nameof( cc.isr.VXI11.Server.Vxi11Server )} is {(server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );

    char DoneKey = 'S';
    Console.Write( $"Press {DoneKey} to stop: " );

    while ( !KeyDone( 'S' ) )
    {
    };
}
catch ( System.Exception e )
{
    Console.WriteLine( e.ToString() );
}
//server.stopRpcProcessing();// stop the service
Console.WriteLine( "VXI-11 IEEE 488 Server stopped." );

static void OnThreadExcetion( object sender, ThreadExceptionEventArgs e )
{
    string name = "unknown";
    if ( sender is cc.isr.VXI11.Server.Vxi11Server ) name = nameof( cc.isr.VXI11.Server.Vxi11Server );
    if ( sender is cc.isr.ONC.RPC.Server.OncRpcServerStubBase ) name = nameof( cc.isr.ONC.RPC.Server.OncRpcServerStubBase );

    Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
}

static bool KeyDone( char doneKey )
{
    return Console.KeyAvailable && string.Equals( Console.ReadKey().KeyChar, doneKey );
}

internal static class Constants
{
    /// <summary>   Gets or sets the server start time typical. </summary>
    /// <value> The server start time typical. </value>
    public static int ServerStartTimeTypical { get; set; } = 3500;

    /// <summary>   Gets or sets the server start loop delay. </summary>
    /// <value> The server start loop delay. </value>
    public static int ServerStartLoopDelay { get; set; } = 100;
}
