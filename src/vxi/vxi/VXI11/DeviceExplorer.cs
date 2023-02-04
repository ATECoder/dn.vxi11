using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.Codecs;
using cc.isr.ONC.RPC.Portmap;
using cc.isr.VXI11.Logging;

namespace cc.isr.VXI11;

public class DeviceExplorer
{

    #region " embedded port mapper "


    /// <summary>   Executes the <see cref="ONC.RPC.Server.OncRpcServerStubBase.ThreadExceptionOccurred"/> event. </summary>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        Event information to send to registered event handlers. </param>
    private static void OnThreadException( object sender, ThreadExceptionEventArgs e )
    {
        Logger.Writer.LogError( $"Thread exception", e.Exception );
    }

    /// <summary>   Starts embedded Portmap service. </summary>
    public static OncRpcEmbeddedPortmapServiceStub StartEmbeddedPortmapService()
    {
        // start the embedded service.
        var epm = OncRpcEmbeddedPortmapServiceStub.StartEmbeddedPortmapService();
        if ( epm.EmbeddedPortmapService is not null )
            epm.EmbeddedPortmapService.ThreadExceptionOccurred += OnThreadException;
        return epm;
    }

    /// <summary>   Ping the host using the <see cref="OncRpcPortmapClient"/> service. </summary>
    /// <remarks>   TODO: Timeout does not seem to make a difference. </remarks>
    /// <param name="host">             The host. </param>
    /// <param name="ioTimeout">        (Optional) The overall i/o timeout, which determines how long
    ///                                 to wait for pinging the port . </param>
    /// <param name="transmitTimeout">  (Optional) The transmit timeout, which sets the socket
    ///                                 timeouts during the transmission of messages to the service. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool PortmapPingHost( IPAddress host, int ioTimeout = 100, int transmitTimeout = 25 )
    {
        return OncRpcPortmapClient.TryPingPortmapService( host, ioTimeout, transmitTimeout );
    }

    #endregion 

    #region " local inter network "

    /// <summary>   Enumerate addresses. </summary>
    /// <param name="host"> The host. </param>
    /// <returns>   An array of <see cref="IPAddress"/>s. </returns>
    public static IPAddress[] EnumerateAddresses( IPAddress host )
    {
        List<IPAddress> addresses = new();

        byte[] bytes = host.GetAddressBytes();
        (byte from, byte to)[] range = new (byte from, byte to)[bytes.Length];
        for ( int i = 0; i < bytes.Length; i++ )
            range[i] = (bytes[i] == 255 ? ( byte ) 1 : bytes[i], bytes[i] == 255 ? ( byte ) 254 : bytes[i]);
        int[] idx = new int[bytes.Length];
        for ( int i = 0; i < bytes.Length; i++ )
            idx[i] = BitConverter.IsLittleEndian ? i : bytes.Length - 1 - i;

        for ( byte i = range[idx[0]].from; i <= range[idx[0]].to; i++ )
        {
            bytes[idx[0]] = i;
            for ( byte j = range[idx[1]].from; j <= range[idx[1]].to; j++ )
            {
                bytes[idx[1]] = j;
                for ( byte k = range[idx[2]].from; k <= range[idx[2]].to; k++ )
                {
                    bytes[idx[2]] = k;
                    for ( byte l = range[idx[3]].from; l <= range[idx[3]].to; l++ )
                    {
                        bytes[idx[3]] = l;
                        IPAddress address = new( bytes );
                        addresses.Add( address );
                    }
                }
            }
        }
        return addresses.ToArray();
    }

    private static IPAddress[] _localInterNetworkAddresses = Array.Empty<IPAddress>();

    /// <summary>   Gets local inter network (IPv4) addresses. </summary>
    /// <returns>   An array of IPv4 addresses. </returns>
    public static IPAddress[] GetLocalInterNetworkAddresses()
    {
        if ( _localInterNetworkAddresses == null || _localInterNetworkAddresses.Length == 0 )
        {
            IPAddress[] localIPs = Dns.GetHostAddresses( Dns.GetHostName() );
            _localInterNetworkAddresses = localIPs.Where( ip => ip.AddressFamily == AddressFamily.InterNetwork ).ToArray();
        }
        return _localInterNetworkAddresses;
    }

    private static IPAddress[] _localBroadcastAddresses = Array.Empty<IPAddress>();

    /// <summary>   Gets local broadcast addresses. </summary>
    /// <returns>   An array of IPv4 broadcast addresses. </returns>
    public static IPAddress[] GetLocalBroadcastAddresses()
    {
        if ( _localBroadcastAddresses == null || _localBroadcastAddresses.Length == 0 )
        {
            List<IPAddress> ipv4s = new();
            foreach ( IPAddress ip in GetLocalInterNetworkAddresses() )
            {
                if ( ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                {
                    byte[] bytes = ip.GetAddressBytes();
                    bytes[BitConverter.IsLittleEndian ? 3 : 0] = 255;
                    ipv4s.Add( new IPAddress( bytes ) );
                }
            }
            _localBroadcastAddresses = ipv4s.ToArray();
        }
        return _localBroadcastAddresses;
    }

    #endregion 

    #region " core devices on the network "

    /// <summary>
    /// Lists the <see cref="IPEndPoint"/>s of the VXI-11 Core devices that are listening on the
    /// addresses spanned by the specified <paramref name="broadcastAddress"/>
    /// which might include a broadcast address (255) as a host address, such as in 192.168.1.255.
    /// </summary>
    /// <param name="broadcastAddress">             The broadcast address or <see langword="null"/>
    ///                                             or <see cref="IPAddress.Any"/>(0.0.0.0) to scan all local
    ///                                             inter networks. </param>
    /// <param name="connectTimeout">               The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded Portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPEndPoint"/> </returns>
    public static List<IPEndPoint> ListCoreDevicesEndpoints( IPAddress broadcastAddress, int connectTimeout, bool startEmbeddedPortmapService )
    {
        // IPAddress does not override '==', which implements reference equality. Must use Equals()

        if ( broadcastAddress is null || broadcastAddress.Equals( IPAddress.Any ) )
        {
            List<IPEndPoint> endpoints = new();
            foreach ( IPAddress broadcastIP in GetLocalBroadcastAddresses() )
            {
                IPAddress[] addresses = EnumerateAddresses( broadcastIP );
                Logger.Writer.LogVerbose( $"{nameof( ListCoreDevicesEndpoints )} scanning {addresses.Length} addresses at {broadcastIP}" );
                endpoints.AddRange( ListCoreDevicesEndpoints( addresses, connectTimeout, startEmbeddedPortmapService, true ) );
                startEmbeddedPortmapService = false;
            }
            return endpoints;
        }
        else
        {
            IPAddress[] addresses = EnumerateAddresses( broadcastAddress );
            Logger.Writer.LogVerbose( $"{nameof( ListCoreDevicesEndpoints )} scanning {addresses.Length} addresses at {broadcastAddress}" );
            return ListCoreDevicesEndpoints( addresses, connectTimeout, startEmbeddedPortmapService, true );
        }
    }

    /// <summary>
    /// Lists the <see cref="IPAddress"/>s of the VXI-11 Core devices that are listening on the.
    /// addresses spanned by the specified <paramref name="broadcastAddress"/>
    /// which might include a broadcast address (255) as a host address, such as in 192.168.1.255.
    /// </summary>
    /// <remarks>   2023-02-01. </remarks>
    /// <param name="broadcastAddress">             The broadcast address or <see langword="null"/>
    ///                                             or <see cref="IPAddress.Any"/>(0.0.0.0) to scan all local
    ///                                             inter networks. </param>
    /// <param name="connectTimeout">               The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded Portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPAddress"/> </returns>
    public static List<IPAddress> ListCoreDevicesAddresses( IPAddress broadcastAddress, int connectTimeout, bool startEmbeddedPortmapService )
    {
        // IPAddress does not override '==', which implements reference equality. Must use Equals()

        if ( broadcastAddress is null || broadcastAddress.Equals( IPAddress.Any ) )
        {
            List<IPAddress> ips = new();
            foreach ( IPAddress broadcastIP in GetLocalBroadcastAddresses() )
            {
                IPAddress[] addresses = EnumerateAddresses( broadcastIP );
                Logger.Writer.LogVerbose( $"{nameof( ListCoreDevicesAddresses )} scanning {addresses.Length} addresses at {broadcastIP}" );
                ips.AddRange( ListCoreDevicesAddresses( addresses, connectTimeout, startEmbeddedPortmapService ) );
                startEmbeddedPortmapService = false;
            }
            return ips;
        }
        else
        {
            IPAddress[] addresses = EnumerateAddresses( broadcastAddress );
            Logger.Writer.LogVerbose( $"{nameof( ListCoreDevicesAddresses )} scanning {addresses.Length} addresses at {broadcastAddress}" );
            return ListCoreDevicesAddresses( addresses, connectTimeout, startEmbeddedPortmapService );
        }
    }

    /// <summary>
    /// Lists the <see cref="IPAddress"/>s of the VXI-11 Core devices that are listening on the
    /// specified <paramref name="addresses"/>.
    /// </summary>
    /// <param name="addresses">                    The hosts. </param>
    /// <param name="connectTimeout">               The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded Portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPAddress"/> </returns>
    public static List<IPAddress> ListCoreDevicesAddresses( IEnumerable<IPAddress> addresses, int connectTimeout, bool startEmbeddedPortmapService )
    {
        // start the embedded service.
        if ( startEmbeddedPortmapService )
        {
            using OncRpcEmbeddedPortmapServiceStub epm = DeviceExplorer.StartEmbeddedPortmapService();
        }

        List<IPAddress> ips = new();
        foreach ( IPAddress address in addresses )
        {
            if ( DeviceExplorer.PortmapPingHost( address, connectTimeout ) )
                ips.Add( address );
        }
        foreach ( IPAddress host in GetLocalInterNetworkAddresses() )
        {
            if ( ips.Contains( host ) ) _ = ips.Remove( host );
        }

        return ips;
    }

    /// <summary>
    /// Lists the <see cref="IPEndPoint"/>s of the VXI-11 Core devices that are listening on the
    /// specified <paramref name="addresses"/>.
    /// </summary>
    /// <param name="addresses">                    The hosts. </param>
    /// <param name="connectTimeout">               The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded Portmap service. </param>
    /// <param name="pmapPing">                     True to use the Portmap service to ping each
    ///                                             address before getting its port number thus
    ///                                             validating the address as a VXI-11 code
    ///                                             device."/> . </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPEndPoint"/> </returns>
    public static List<IPEndPoint> ListCoreDevicesEndpoints( IEnumerable<IPAddress> addresses, int connectTimeout, bool startEmbeddedPortmapService, bool pmapPing )
    {
        // start the embedded service.
        if ( startEmbeddedPortmapService )
        {
            using OncRpcEmbeddedPortmapServiceStub epm = DeviceExplorer.StartEmbeddedPortmapService();
        }

        List<IPEndPoint> endpoints = new();
        foreach ( IPAddress address in addresses )
        {
            if ( pmapPing && DeviceExplorer.PortmapPingHost( address, connectTimeout ) )
            {
                int port = GetCoreDevicePortNumber( address, connectTimeout );
                if ( port > 0 ) { endpoints.Add( new IPEndPoint( address, port ) ); }
            }
        }
        return endpoints;
    }

    /// <summary>
    /// Gets the port of any VXI-11 Core device that is listening on the specific <paramref name="host"/> address.
    /// </summary>
    /// <param name="host">     The host. </param>
    /// <param name="connectTimeout">  The timeout in milliseconds. </param>
    /// <returns>   The port on which a device is listening (if value > 0 ). </returns>
    public static int GetCoreDevicePortNumber( IPAddress host, int connectTimeout )
    {
        // exclude the host addresses
        if ( GetLocalInterNetworkAddresses().Contains( host ) ) return 0;

        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. 
        using OncRpcPortmapClient pmapClient = new( host, OncRpcProtocol.OncRpcTcp, connectTimeout );
        pmapClient.OncRpcClient!.IOTimeout = OncRpcTcpClient.IOTimeoutDefault;

        // get a port from this host, if any.
        return pmapClient.GetPort( Vxi11ProgramConstants.CoreProgram, Vxi11ProgramConstants.CoreVersion, OncRpcProtocol.OncRpcTcp );
    }

    #endregion 

    #region " registered servers on the network "

    /// <summary>
    /// Enumerate the endpoints or registered servers on the specified <paramref name="broadcastAddress"/>
    /// which might include a broadcast address (255) as a host address, such as in 192.168.1.255.
    /// </summary>
    /// <param name="broadcastAddress">             The broadcast address or <see langword="null"/>
    ///                                             or <see cref="IPAddress.Any"/>(0.0.0.0) to scan all local
    ///                                             inter networks. </param>
    /// <param name="timeout">                      The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded Portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPEndPoint"/> </returns>
    public static List<IPEndPoint> EnumerateRegisteredServers( IPAddress broadcastAddress, int timeout, bool startEmbeddedPortmapService )
    {
        // IPAddress does not override '==', which implements reference equality. Must use Equals()

        if ( broadcastAddress is null || broadcastAddress.Equals( IPAddress.Any ) )
        {
            List<IPEndPoint> endpoints = new();
            foreach ( IPAddress broadcastIP in GetLocalBroadcastAddresses() )
            {
                IPAddress[] addresses = EnumerateAddresses( broadcastIP );
                Logger.Writer.LogVerbose( $"{nameof( EnumerateRegisteredServers )} scanning {addresses.Length} addresses at {broadcastIP}" );
                endpoints.AddRange( EnumerateRegisteredServers( addresses, timeout, startEmbeddedPortmapService ) );
                startEmbeddedPortmapService = false;
            }
            return endpoints;
        }
        else
        {
            IPAddress[] addresses = EnumerateAddresses( broadcastAddress );
            Logger.Writer.LogVerbose( $"{nameof( EnumerateRegisteredServers )} scanning {addresses.Length} addresses at {broadcastAddress}" );
            return EnumerateRegisteredServers( addresses, timeout, startEmbeddedPortmapService );
        }
    }

    /// <summary>
    /// Enumerate the endpoints or registered servers on the specified <paramref name="addresses"/>.
    /// </summary>
    /// <param name="addresses">                    The addresses. </param>
    /// <param name="timeout">                      The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded Portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPEndPoint"/> </returns>
    public static List<IPEndPoint> EnumerateRegisteredServers( IEnumerable<IPAddress> addresses, int timeout, bool startEmbeddedPortmapService )
    {

        // start the embedded service.
        if ( startEmbeddedPortmapService )
        {
            using OncRpcEmbeddedPortmapServiceStub epn = DeviceExplorer.StartEmbeddedPortmapService();
        }

        List<IPEndPoint> registeredDevices = new();

        foreach ( IPAddress host in addresses )
        {
            registeredDevices.AddRange( EnumerateRegisteredServers( host, timeout ) );
        }

        return registeredDevices;
    }

    /// <summary>   Enumerate the registered servers on the specified <paramref name="host"/>. </summary>
    /// <param name="host">     The host. </param>
    /// <param name="connectTimeout">  The timeout in milliseconds. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPEndPoint"/> 
    /// </returns>
    public static List<IPEndPoint> EnumerateRegisteredServers( IPAddress host, int connectTimeout )
    {
        List<IPEndPoint> endpoints = new();

        // exclude the host addresses

        if ( GetLocalInterNetworkAddresses().Contains( host ) ) return endpoints;

        // skip if the Portmap service is not registered on the host

        if ( !Paping( new IPEndPoint( host, OncRpcPortmapConstants.OncRpcPortmapPortNumber ) ) )
            return endpoints;

        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. 

        using OncRpcPortmapClient tcpPmapClient = new( host, OncRpcProtocol.OncRpcTcp, connectTimeout );
        tcpPmapClient.OncRpcClient!.IOTimeout = OncRpcTcpClient.IOTimeoutDefault;

        // Now dump the current list of registered servers.

        OncRpcServerIdentifierCodec[] registeredServers = tcpPmapClient.ListRegisteredServers(); ;
        foreach ( OncRpcServerIdentifierCodec registeredServer in registeredServers )
        {
            if ( registeredServer.Port > 0 ) { endpoints.Add( new IPEndPoint( host, registeredServer.Port ) ); }
        }
        return endpoints;
    }

    #endregion

    #region " socket "

    /// <summary>   Pings port. </summary>
    /// <param name="ipv4Address">          The IPv4 address. </param>
    /// <param name="portNumber">           (Optional) The port number [5025]. </param>
    /// <param name="timeoutMilliseconds">  (Optional) The timeout in milliseconds [10]. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool Paping( IPAddress ipv4Address, int portNumber = 5025, int timeoutMilliseconds = 10 )
    {
        return Paping( ipv4Address.ToString(), portNumber, timeoutMilliseconds );
    }

    /// <summary>   Pings port. </summary>
    /// <param name="ipv4Address">          The IPv4 address. </param>
    /// <param name="portNumber">           (Optional) The port number [5025]. </param>
    /// <param name="timeoutMilliseconds">  (Optional) The timeout in milliseconds [10]. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool Paping( string ipv4Address, int portNumber = 5025, int timeoutMilliseconds = 10 )
    {
        return Paping( new IPEndPoint( IPAddress.Parse( ipv4Address ), portNumber ), timeoutMilliseconds );
    }


    /// <summary>   Pings port. </summary>
    /// <param name="endpoint">             The endpoint. </param>
    /// <param name="timeoutMilliseconds">  (Optional) The timeout in milliseconds. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool Paping( IPEndPoint endpoint, int timeoutMilliseconds = 10 )
    {
        bool pinged = false;
        try
        {
            using Socket socket = new( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            socket.Blocking = true;
            IAsyncResult result = socket.BeginConnect( endpoint, null, null );
            bool success = result.AsyncWaitHandle.WaitOne( timeoutMilliseconds, true );
            if ( socket.Connected )
            {
                socket.EndConnect( result );
                socket.Shutdown( SocketShutdown.Both );
                pinged = true;
            }
        }
        catch ( Exception )
        {
            // Discard Ping Exceptions and return false;
        }
        return pinged;
    }


    /// <summary>   Ping host. </summary>
    /// <param name="nameOrAddress">        The name or address. </param>
    /// <param name="timeoutMilliseconds">  (Optional) The timeout [100] ms. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool PingHost( string nameOrAddress, int timeoutMilliseconds = 100 )
    {
        bool pingable = false;
        try
        {
            using Ping pinger = new();
            PingReply reply = pinger.Send( nameOrAddress, timeoutMilliseconds );
            pingable = reply.Status == IPStatus.Success;
        }
        catch ( PingException )
        {
            // Discard PingExceptions and return false;
        }
        return pingable;
    }

    /// <summary>   Ping host. </summary>
    /// <param name="address">              The address. </param>
    /// <param name="timeoutMilliseconds">  (Optional) The timeout [100] ms. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool PingHost( IPAddress address, int timeoutMilliseconds = 100 )
    {
        bool pingable = false;
        try
        {
            using Ping pinger = new();
            PingReply reply = pinger.Send( address, timeoutMilliseconds );
            pingable = reply.Status == IPStatus.Success;
        }
        catch ( PingException )
        {
            // Discard PingExceptions and return false;
        }
        return pingable;
    }

    #endregion

}

