using System.Diagnostics.Metrics;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

using cc.isr.VXI11.IEEE488;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.LXI.Discover;

internal static class LxiDiscover
{

    public static string HelpDescription = @$"
VXI-11 {nameof( LxiDiscover )} 

Description: Finds all the LXI/VXI-11 instruments listening on the network.

VXI-11 {nameof( LxiDiscover )} is a light-weight Windows program which uses
the OS-independent cc.isr.VXI11 package to discover LXI/VXI-11 devices that
listen on a broadcast network IP such as 192.168.0.255.

The timeout specified in the command line applies to each IP address derived
from the broadcast address. For example, 192.168.0.255 entails 254 such
IP addresses. Thus, it is expected to take at least 25.4 seconds to scan
this broadcast address with the default timeout of 100ms.

Command Line: {CommandLineParser.Usage}

Default command line: {CommandLineParser.DefaultArgs}

Specify an empty broadcast address (e.g., {CommandLineParser.IPKey}) or
{IPAddress.Any} (e.g., {CommandLineParser.IPKey}{CommandLineParser.EqualsSign}{IPAddress.Any})
to discover all the instruments listening on the local IPs of this machine.

";

    public static void DiscoverEndpoints( string ip, int timeout )
    {
        Console.WriteLine( $"\nDiscovering instruments on {ip}...." );

        List<IPEndPoint> endpoints = cc.isr.VXI11.DeviceExplorer.ListCoreDevicesEndpoints( IPAddress.Parse( ip ), timeout, true );

        foreach ( IPEndPoint endpoint in endpoints )
        {
            Console.WriteLine( $"{endpoint}: {QueryInstrumet( endpoint.Address.ToString() )}" );
        }
    }

    public static void DiscoverAddresses( string ip, int timeout )
    {
        Console.WriteLine( $"\nDiscovering instruments on {ip}...." );

        List<IPAddress> addresses = cc.isr.VXI11.DeviceExplorer.ListCoreDevicesAddresses( IPAddress.Parse( ip ), timeout, true );

        foreach ( IPAddress address in addresses )
        {
            Console.WriteLine( $"{address}: {QueryInstrumet( address.ToString() )}" );
        }
    }

    public static void Discover( string ip, int timeout )
    {

        Console.WriteLine( $"Discovering devices on IP={ip} with a timeout of {timeout} ms\n" );

        // IPAddress does not override '==', which implements reference equality. Must use Equals()

        if ( string.IsNullOrWhiteSpace(ip ) || IPAddress.Parse( ip ).Equals( IPAddress.Any ) )
        {
            double totalTimeout = 0;
            foreach ( IPAddress address in GetLocalBroadcastAddresses() )
            {
                IPAddress[] ips = DeviceExplorer.EnumerateAddresses( address );
                totalTimeout += ips.Length * ( double ) timeout;
            }
            Console.WriteLine( $"Discovery is estimated to take {totalTimeout / 1000} seconds\nDiscovering...\n" );
            foreach ( IPAddress address in GetLocalBroadcastAddresses() )
            {
                DiscoverAddresses( address.ToString(), timeout );
            }
        }
        else
        {
            IPAddress[] ips = DeviceExplorer.EnumerateAddresses( IPAddress.Parse( ip ) );
            Console.WriteLine( $"Discovery is estimated to take {ips.Length * ( double ) timeout / 1000} seconds\nDiscovering...\n" );
            DiscoverAddresses( ip, timeout );
        }
        StringBuilder builder = new();
        _ = builder.AppendLine( "LXI Instruments Discovery complete. If you did not find your instrument" );
        _ = builder.AppendLine( "then try increasing the timeout value and try again.\n" );
        _ = builder.AppendLine( "Sometimes devices are not detected properly when the computer/Laptop" );
        _ = builder.AppendLine( "in which you are running this the discovery program is connected" );
        _ = builder.AppendLine( "is on WIFI instead of Ethernet. So it is recommended to connect discover" );
        _ = builder.AppendLine( "the instruments when connected to Ethernet.\n" );
        _ = builder.AppendLine( $"For help try {CommandLineParser.HelpUsage}\n" );
        Console.Write( builder.ToString() );
    }

    public static string QueryInstrumet( string ipv4Address )
    {
        using cc.isr.VXI11.IEEE488.Ieee488Instrument instrument = new();
        instrument.ThreadExceptionOccurred += OnThreadExcetion;
        instrument.Connect( ipv4Address, Visa.DeviceAddress.BuildInterfaceDeviceString( Visa.DeviceAddress.GenericInterfaceFamily,0 ) );
        return instrument.QueryLine( Ieee488Commands.IDNRead ).response;
    }

    internal static void OnThreadExcetion( object sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is cc.isr.VXI11.IEEE488.Ieee488Instrument ) name = nameof( cc.isr.VXI11.IEEE488.Ieee488Instrument );
        Logger.Writer.LogError( $"{name} encountered an exception during an asynchronous operation", e.Exception );
    }

    /// <summary>   Gets local broadcast addresses. </summary>
    /// <returns>   An array of IP address. </returns>
    public static IPAddress[] GetLocalBroadcastAddresses()
    {
        IPAddress[] localIPs = Dns.GetHostAddresses( Dns.GetHostName() );
        List<IPAddress> ipv4s = new();
        foreach ( IPAddress ip in localIPs )
        {
            if ( ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
            {
                byte[] bytes = ip.GetAddressBytes();
                bytes[3] = 255;
                ipv4s.Add( new IPAddress( bytes ) );
            }
        }
        return ipv4s.ToArray();
    }
}

