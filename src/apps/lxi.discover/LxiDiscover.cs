using System.Net;
using System.Text;

using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11.Discover;

/// <summary>   An LXI discover. </summary>
/// <remarks>
/// 2450 (152), 6510 (154) and 7510 (144) are on:  
/// <code>
/// VXI-11 LxiDiscover
/// Discovering devices on IP = 0.0.0.0 with a timeout of 10 ms
///
/// Discovery is estimated to take 5.08 seconds...
/// 
/// Discovering instruments on 192.168.4.255....
/// 2023-02-04 12:39:58.012,ListCoreDevicesAddresses scanning 254 addresses at 192.168.4.255
/// 2023-02-04 12:39:58.042,Checking for Portmap service
/// 2023-02-04 12:39:58.258,No Portmap service available.
/// 2023-02-04 12:39:58.258,Creating embedded Portmap instance
/// 2023-02-04 12:39:58.474,Portmap service started; checked 215.3 ms.
/// Found 0 instruments on 192.168.4.255
///
/// Discovering instruments on 192.168.0.255....
/// 2023-02-04 12:40:08.138,ListCoreDevicesAddresses scanning 254 addresses at 192.168.0.255
/// 2023-02-04 12:40:08.138,Checking for Portmap service
/// 2023-02-04 12:40:08.253,No Portmap service available.
/// 2023-02-04 12:40:08.253, Creating embedded Portmap instance
/// 2023-02-04 12:40:08.469, Portmap service started; checked 115.1 ms.
/// Found 3 instruments on 192.168.0.255
///
/// 192.168.0.144: KEITHLEY INSTRUMENTS, MODEL DMM7510,04051720,1.7.7b
///
/// 192.168.0.152: KEITHLEY INSTRUMENTS, MODEL 2450,01419966,1.6.4c
///
/// 192.168.0.154: KEITHLEY INSTRUMENTS, MODEL DAQ6510,04388991,0.0.03i
///
/// LXI Instruments Discovery complete.If you did not find your instrument
/// then try increasing the timeout value and try again.
///
/// Sometimes devices are not detected properly when the computer/Laptop
/// in which you are running this the discovery program is connected
/// is on WIFI instead of Ethernet.So it is recommended to connect discover
/// the instruments when connected to Ethernet.
///
/// For help try ./LxiDiscover --help
/// </code>
/// </remarks>
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
IP addresses. Thus, it is expected to take at least 2.54 seconds to scan
this broadcast address with the default timeout of 10ms.

A device interface string, such as 'inst0' is required for reporting the
instruments identity (*IDN) strings. Otherwise, only the instrument addresses
are reported. If USB or GPIB instruments, which have a different device interface
string, reside on the network, {nameof( LxiDiscover )} will catch the exceptions
from the identity query thus slowing the discovery.

Command Line: {CommandLineParser.Usage}

Default command line: {CommandLineParser.DefaultArgs}

Specify an empty broadcast address (e.g., {CommandLineParser.IPKey}) or
{IPAddress.Any} (e.g., {CommandLineParser.IPKey}{CommandLineParser.EqualsSign}{IPAddress.Any})
to discover all the instruments listening on the local IPs of this machine.

";

    /// <summary>   Discover endpoints. </summary>
    /// <remarks>   2023-02-06. </remarks>
    /// <param name="broadcastAddress">         The broadcast address such as "192.169.0.255" or null
    ///                                         (<see cref="System.Net.IPAddress.Any"/> (0.0.0.0). </param>
    /// <param name="timeout">                  The timeout. </param>
    /// <param name="deviceInterfaceString">    The device interface string, e.g., inst0 or gpib0,4. </param>
    private static void DiscoverEndpoints( string broadcastAddress, int timeout, string deviceInterfaceString )
    {
        Console.WriteLine( $"Discovering {deviceInterfaceString} instruments on {broadcastAddress}...." );

        List<IPEndPoint> endpoints = Vxi11Discoverer.ListCoreDevicesEndpoints( IPAddress.Parse( broadcastAddress ), timeout, true );

        Console.WriteLine( $"Found {endpoints.Count} instruments on {broadcastAddress}\n" );
        foreach ( IPEndPoint endpoint in endpoints )
        {
            if ( string.IsNullOrWhiteSpace( deviceInterfaceString ))
                Console.WriteLine( endpoint.ToString() );
            else
                Console.WriteLine( $"{endpoint}: {TryQueryIdentity( endpoint.Address.ToString(), deviceInterfaceString )}" );
        }
    }

    /// <summary>   Discover addresses. </summary>
    /// <remarks>   2023-02-06. </remarks>
    /// <param name="broadcastAddress">         The broadcast address such as "192.169.0.255" or null
    ///                                         (<see cref="System.Net.IPAddress.Any"/> (0.0.0.0). </param>
    /// <param name="timeout">                  The timeout. </param>
    /// <param name="deviceInterfaceString">    The device interface string, e.g., inst0 or gpib0,4. </param>
    private static void DiscoverAddresses( string broadcastAddress, int timeout, string deviceInterfaceString )
    {
        Console.WriteLine( $"Discovering {deviceInterfaceString} instruments on {broadcastAddress}...." );

        List<IPAddress> addresses = Vxi11Discoverer.ListCoreDevicesAddresses( IPAddress.Parse( broadcastAddress ), timeout, true );

        Console.WriteLine( $"Found {addresses.Count} instruments on {broadcastAddress}\n" );
        foreach ( IPAddress address in addresses )
        {
            if ( string.IsNullOrWhiteSpace( deviceInterfaceString ) )
                Console.WriteLine( address.ToString() );
            else
                Console.WriteLine( $"{address}: {TryQueryIdentity( address.ToString(), deviceInterfaceString )}" );
        }
    }

    /// <summary>   Discovers th devices on the specified broadcast address. </summary>
    /// <remarks>   2023-02-06. </remarks>
    /// <param name="broadcastAddress">         The broadcast address such as "192.169.0.255" or null
    ///                                         (<see cref="System.Net.IPAddress.Any"/> (0.0.0.0). </param>
    /// <param name="timeout">                  The timeout. </param>
    /// <param name="deviceInterfaceString">    The device interface string, e.g., inst0 or gpib0,4. </param>
    public static void Discover( string broadcastAddress, int timeout, string deviceInterfaceString )
    {

        Console.WriteLine( $"Discovering {deviceInterfaceString} devices on {broadcastAddress} with a timeout of {timeout} ms\n" );

        // IPAddress does not override '==', which implements reference equality. Must use Equals()

        if ( string.IsNullOrWhiteSpace(broadcastAddress ) || IPAddress.Parse( broadcastAddress ).Equals( IPAddress.Any ) )
        {
            double totalTimeout = 0;
            foreach ( IPAddress address in GetLocalBroadcastAddresses() )
            {
                IPAddress[] ips = Vxi11Discoverer.EnumerateAddresses( address );
                totalTimeout += ips.Length * ( double ) timeout;
            }
            Console.WriteLine( $"Discovery is estimated to take {totalTimeout / 1000} seconds...\n" );
            foreach ( IPAddress address in GetLocalBroadcastAddresses() )
            {
                DiscoverAddresses( address.ToString(), timeout, deviceInterfaceString );
            }
        }
        else
        {
            IPAddress[] ips = Vxi11Discoverer.EnumerateAddresses( IPAddress.Parse( broadcastAddress ) );
            Console.WriteLine( $"Discovery is estimated to take {ips.Length * ( double ) timeout / 1000} seconds...\n" );
            DiscoverAddresses( broadcastAddress, timeout, deviceInterfaceString );
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

    /// <summary>   Queries the instrument identity. </summary>
    /// <remarks>   2023-02-06. </remarks>
    /// <param name="address">                  The instrument network IPv4 address. </param>
    /// <param name="deviceInterfaceString">    The device interface string, e.g., inst0 or gpib0,4. </param>
    /// <returns>   The instrument identity. </returns>
    public static string QueryIdentity( string address, string deviceInterfaceString )
    {
        using Vxi11Client instrument = new();
        instrument.ThreadExceptionOccurred += OnThreadException;
        instrument.Connect( address, deviceInterfaceString );
        return instrument.QueryLine( "*IDN?" ).response;
    }

    /// <summary>   Try query identity. </summary>
    /// <param name="address">                  The instrument network IPv4 address. </param>
    /// <param name="deviceInterfaceString">    The device interface string, e.g., inst0 or gpib0,4. </param>
    /// <returns>   A string. </returns>
    public static string TryQueryIdentity( string address, string deviceInterfaceString )
    {
        try
        {
            return QueryIdentity( address, deviceInterfaceString );
        }
        catch ( DeviceException ex  )
        {
            return $"unable to query identity from TCPIP::{address}::{deviceInterfaceString}::INSTR because: {ex.Reason}({( int ) ex.Reason}) {ex.Message}";
        }
        catch ( Exception ex )
        {
            return $"unable to query identity from TCPIP::{address}::{deviceInterfaceString}::INSTR because: {ex.Message}";
        }
    }

    /// <summary>   Raises the thread exception event. </summary>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    internal static void OnThreadException( object sender, ThreadExceptionEventArgs e )
    {
        string name = "unknown";
        if ( sender is Vxi11Client ) name = nameof( Vxi11Client );
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

