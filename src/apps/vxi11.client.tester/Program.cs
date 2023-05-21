using cc.isr.VXI11;
using cc.isr.VXI11.Logging;

AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobsserverException;

Console.WriteLine( $"VXI-11 {nameof( cc.isr.VXI11.Client.Vxi11Client )} Tester" );

string ipv4Address = "192.168.0.144"; // "127.0.0.1";
string deviceName = "inst0";

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

using cc.isr.VXI11.Client.Vxi11Client vxi11Client = new();

vxi11Client.ThreadExceptionOccurred += OnThreadExcetion;

Console.WriteLine();
Console.Write( $"Press key to Connect to {ipv4Address}: " );
Console.ReadKey();

Console.WriteLine( $"Connecting to TCPIP::{ipv4Address}::{deviceName}::INSTR" );

vxi11Client.Connect( ipv4Address, deviceName );

if ( ipv4Address == "127.0.0.1" )
{
    string command = "*IDN?";
    SendCommand( command );

    // closing client throws an exception when using the local mock server.
    // 
}
else
{
    string command = "*RST";
    SendCommand( command );

    command = "*CLS";
    SendCommand( command );

    command = "SYST:CLE";
    SendCommand( command );

    command = "*IDN?";
    SendCommand( command );

    Console.WriteLine( $"closing {ipv4Address}" );

    vxi11Client.Close();

}

Console.Write( "\n Press key to end" );
Console.ReadKey();

void SendCommand( string command )
{
    Console.WriteLine( $"Hit any key to send {command} to {ipv4Address}" );
    _ = Console.ReadKey();
    (int sentCount, DeviceErrorCode errorCode, string errorDetails) = vxi11Client.TryWriteLine( command );
    if ( errorCode != DeviceErrorCode.NoError )
        Console.WriteLine( $"{command} write failed;  {DeviceException.BuildErrorMessage( $"; {errorDetails}", errorCode )}" );
    else if ( sentCount == 0 )
        Console.WriteLine( $"{command} not sent" );
    else if ( vxi11Client.IsQuery( command ) )
    {
        (string response, errorCode, errorDetails) = vxi11Client.TryRead();
        if (errorCode != DeviceErrorCode.NoError )
            Console.WriteLine( $"{command} read failed;  {DeviceException.BuildErrorMessage( $"; {errorDetails}", errorCode )}" );
        Console.WriteLine( $"{command} sent{(string.IsNullOrEmpty( response ) ? string.Empty : $"; received: {response}")}" );
    }
    else
        Console.WriteLine( $"{command} sent" );
}

static void OnThreadExcetion( object? sender, ThreadExceptionEventArgs e )
{
    string name = "unknown";
    if ( sender is cc.isr.VXI11.Client.Vxi11Client ) name = nameof( cc.isr.VXI11.Client.Vxi11Client );

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


