// See https://aka.ms/new-console-template for more information

using cc.isr.ONC.RPC.Portmap;

AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedException;

try
{
    using cc.isr.VXI11.Server.Vxi11Server server = new();
    server.ThreadExceptionOccurred += OnThreadException;

    Console.WriteLine( "Starting the embedded port map service" );
    using OncRpcEmbeddedPortmapServiceStub epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
    epm.EmbeddedPortmapService!.ThreadExceptionOccurred += OnThreadException;

    Console.WriteLine( "Starting VXI-11 IEEE 488 Server" );
    _ = server.RunAsync();

    _ = server.ServerStarted( 2 * Constants.ServerStartTimeTypical, Constants.ServerStartLoopDelay );

    Console.WriteLine( $"{nameof( cc.isr.VXI11.Server.Vxi11Server )} is {(server.Running ? "running" : "idle")}  {DateTime.Now:ss.fff}" );

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

static void OnThreadException( object? sender, ThreadExceptionEventArgs e )
{
    string name = "unknown";
    if ( sender is cc.isr.VXI11.Server.Vxi11Server ) name = nameof( cc.isr.VXI11.Server.Vxi11Server );
    if ( sender is cc.isr.ONC.RPC.Server.OncRpcServerStubBase ) name = nameof( cc.isr.ONC.RPC.Server.OncRpcServerStubBase );
    if ( e.Exception is Exception )
        Console.WriteLine( $"{name} encountered an exception during an asynchronous operation: {e.Exception}" );
}

#region " unhandled exception handling "

/// <summary>   Raises the unhandled exception event. </summary>
/// <remarks>   2023-06-02. </remarks>
/// <param name="sender">   Source of the event. </param>
/// <param name="e">        Event information to send to registered event handlers. </param>
static void OnUnhandledException( object? sender, UnhandledExceptionEventArgs e )
{
    Console.WriteLine( $"\n Unhandled exception occurred: {e.ExceptionObject}\n" );
}

static void OnTaskSchedulerUnobservedException( object? sender, UnobservedTaskExceptionEventArgs e )
{
    Console.WriteLine( $"{(e.Observed ? "" : "un")}observed exception occurred: {e.Exception}\n" );
}

#endregion


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

