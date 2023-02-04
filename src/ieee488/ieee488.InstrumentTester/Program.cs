// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;

using cc.isr.VXI11.Logging;

Console.WriteLine( $"VXI-11 {nameof( cc.isr.VXI11.IEEE488.Ieee488Instrument)} Tester" );

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

using cc.isr.VXI11.IEEE488.Ieee488Instrument instrument = new();

instrument.ThreadExceptionOccurred += OnThreadExcetion;

Console.WriteLine();
Console.Write( $"Press key to Connect to {ipv4Address}: " );
Console.ReadKey();

// client.connect("127.0.0.1", "inst0");
Console.WriteLine( $"Connecting to {ipv4Address}" );

instrument.Connect( ipv4Address, cc.isr.VXI11.Visa.DeviceAddress.BuildInterfaceDeviceString( cc.isr.VXI11.Visa.DeviceAddress.GenericInterfaceFamily, 0) );

if ( ipv4Address == "127.0.0.1" )
{
    string command = cc.isr.VXI11.IEEE488.Ieee488Commands.IDNRead;
    SendCommand( command );

    // closing client throws an exception when using the local mock server.
    // 
}
else
{
    string command = cc.isr.VXI11.IEEE488.Ieee488Commands.RST;
    SendCommand( command );

    command = cc.isr.VXI11.IEEE488.Ieee488Commands.CLS;
    SendCommand( command );

    command = "SYST:CLE";
    SendCommand( command );

    command = cc.isr.VXI11.IEEE488.Ieee488Commands.IDNRead;
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
    int sentCount = instrument.WriteLine( command );
    if ( sentCount == 0 )
        Console.WriteLine( $"{command} not sent" );
    else if ( instrument.IsQuery( command ) )
    {
        string response = instrument.Read();
        Console.WriteLine( $"{command} sent{(string.IsNullOrEmpty( response ) ? string.Empty : $"; received: {response}")}" );
    }
    else
        Console.WriteLine( $"{command} sent" );
}

static void OnThreadExcetion( object sender, ThreadExceptionEventArgs e )
{
    string name = "unknown";
    if ( sender is cc.isr.VXI11.IEEE488.Ieee488Instrument ) name = nameof( cc.isr.VXI11.IEEE488.Ieee488Instrument );

    Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
}
