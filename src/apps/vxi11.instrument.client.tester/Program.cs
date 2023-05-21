// See https://aka.ms/new-console-template for more information

using cc.isr.VXI11;
using cc.isr.VXI11.Logging;
using cc.isr.VXI11.Client;
using cc.isr.VXI11.Server;

AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobsserverException;

Console.WriteLine( $"VXI-11 {nameof( Vxi11InstrumentClient )} Tester" );

string ipv4Address = "192.168.0.144"; // "127.0.0.1";

bool ready = false;
while ( !ready )
{
    Console.Write( $"Enter IP Address, e.g., '127.0.0.1' [{ipv4Address}]: " );
    string? enteredIp = Console.ReadLine();
    ipv4Address = string.IsNullOrWhiteSpace( enteredIp ) ? ipv4Address : enteredIp;
    Console.WriteLine();
    Console.Write( $"Connect to {ipv4Address}? " );
    var yesno = Console.ReadKey();
    ready = yesno.KeyChar == 'y' || yesno.KeyChar == 'Y';
}

using Vxi11InstrumentClient instrument = new();

instrument.ThreadExceptionOccurred += OnThreadExcetion;

Console.WriteLine();
Console.Write( $"Press key to Connect to {ipv4Address}: " );
Console.ReadKey();

// client.connect("127.0.0.1", "inst0");
Console.WriteLine( $"Connecting to {ipv4Address}" );

instrument.Connect( ipv4Address, DeviceNameParser.BuildDeviceName( DeviceNameParser.GenericInterfaceFamily, 0 ) );

if ( ipv4Address == "127.0.0.1" )
{
    string command = Vxi11InstrumentCommands.IDNRead;
    SendCommand( command );

    // closing client throws an exception when using the local mock server.
    // 
}
else
{
    string command = Vxi11InstrumentCommands.RST;
    SendCommand( command );

    command = Vxi11InstrumentCommands.CLS;
    SendCommand( command );

    command = "SYST:CLE";
    SendCommand( command );

    command = Vxi11InstrumentCommands.IDNRead;
    SendCommand( command );

    Console.WriteLine( $"closing {ipv4Address}" );

    instrument.Close();

}

Console.Write( "\n Press key to end" );
Console.ReadKey();

void SendCommand( string command )
{
    Console.WriteLine( $"Hit any key to send {command} to {ipv4Address}" );
    _ = Console.ReadKey();
    (int sentCount, DeviceErrorCode errorCode, string errorDetails) = instrument.TryWriteLine( command );
    if ( errorCode != DeviceErrorCode.NoError )
        Console.WriteLine( $"{command} write failed;  {DeviceException.BuildErrorMessage( $"; {errorDetails}", errorCode )}" );
    else if ( sentCount == 0 )
        Console.WriteLine( $"{command} not sent" );
    else if ( instrument.IsQuery( command ) )
    {
        (string response, errorCode, errorDetails) = instrument.TryRead();
        if ( errorCode != DeviceErrorCode.NoError )
            Console.WriteLine( $"{command} read failed;  {DeviceException.BuildErrorMessage( $"; {errorDetails}", errorCode )}" );
        Console.WriteLine( $"{command} sent{(string.IsNullOrEmpty( response ) ? string.Empty : $"; received: {response}")}" );
    }
    else
        Console.WriteLine( $"{command} sent" );
}

static void OnThreadExcetion( object? sender, ThreadExceptionEventArgs e )
{
    string name = "unknown";
    if ( sender is Vxi11InstrumentClient ) name = nameof( Vxi11InstrumentClient );

    Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
}

#region " Unhandled exception handling "

static void OnUnhandledException( object? sender, UnhandledExceptionEventArgs e )
{
    Console.WriteLine( $"\n Unhandled exception occurred: {e.ExceptionObject}\n" );
}

static void OnTaskSchedulerUnobsserverException( object? sender, UnobservedTaskExceptionEventArgs e )
{
    Console.WriteLine( $"{(e.Observed ? "" : "un")}observed exception occurred: {e.Exception}\n" );
}

#endregion


