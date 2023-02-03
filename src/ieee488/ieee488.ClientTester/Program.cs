// See https://aka.ms/new-console-template for more information

using cc.isr.VXI11.Logging;

Console.WriteLine( $"VXI-11 {nameof( cc.isr.VXI11.IEEE488.Ieee488Client )} Tester" );

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

using cc.isr.VXI11.IEEE488.Ieee488Client ieee488Client = new();

ieee488Client.ThreadExceptionOccurred += OnThreadExcetion;

Console.WriteLine();
Console.Write( $"Press key to Connect to {ipv4Address}: " );
Console.ReadKey();

// client.connect("127.0.0.1", "inst0");
Console.WriteLine( $"Connecting to {ipv4Address}" );

ieee488Client.Connect( ipv4Address, "inst0" );

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

    ieee488Client.Close();

}

Console.Write( "\n Press key to end" );
Console.ReadKey();

void SendCommand( string command )
{
    Console.WriteLine( $"Hit any key to send {command} to {ipv4Address}" );
    _ = Console.ReadKey();
    int sentCount = ieee488Client.WriteLine( command );
    if ( sentCount == 0 )
        Console.WriteLine( $"{command} not sent" );
    else if ( ieee488Client.IsQuery( command ) )
    {
        string response = ieee488Client.Read();
        Console.WriteLine( $"{command} sent{(string.IsNullOrEmpty( response ) ? string.Empty : $"; received: {response}")}" );
    }
    else
        Console.WriteLine( $"{command} sent" );
}

static void OnThreadExcetion( object sender, ThreadExceptionEventArgs e )
{
    string name = "unknown";
    if ( sender is cc.isr.VXI11.IEEE488.Ieee488Client ) name = nameof( cc.isr.VXI11.IEEE488.Ieee488Client );

    Logger.Writer.LogError( $"{name}  encountered an exception during an asynchronous operation", e.Exception );
}
