using System.Diagnostics.SymbolStore;

using cc.isr.VXI11.Logging;

Console.WriteLine( $"VXI-11 {nameof( cc.isr.VXI11.Client.Vxi11Client )} Tester" );

string ipv4Address = "192.168.0.144"; // "127.0.0.1";
string deviceInterfaceString = "inst0";

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

Console.WriteLine( $"Connecting to TCPIP::{ipv4Address}::{deviceInterfaceString}::INSTR" );

vxi11Client.Connect( ipv4Address, deviceInterfaceString );

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
    int sentCount = vxi11Client.WriteLine( command );
    if ( sentCount == 0 )
        Console.WriteLine( $"{command} not sent" );
    else if ( vxi11Client.IsQuery( command ) )
    {
        string response = vxi11Client.Read();
        Console.WriteLine( $"{command} sent{(string.IsNullOrEmpty( response ) ? string.Empty : $"; received: {response}")}" );
    }
    else
        Console.WriteLine( $"{command} sent" );
}

static void OnThreadExcetion( object sender, ThreadExceptionEventArgs e )
{
    string name = "unknown";
    if ( sender is cc.isr.VXI11.Client.Vxi11Client ) name = nameof( cc.isr.VXI11.Client.Vxi11Client );

    Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
}
