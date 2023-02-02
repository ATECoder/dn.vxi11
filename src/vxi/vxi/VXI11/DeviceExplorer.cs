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

    /// <summary>   Starts embedded portmap service. </summary>
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

    #region " enumerate core devices "

    /// <summary>   Enumerate addresses. </summary>
    /// <param name="host"> The host. </param>
    /// <returns>   An array of <see cref="IPAddress"/>s. </returns>
    public static IPAddress[] EnumerateAddresses( IPAddress host )
    {
        List<IPAddress> addresses = new ();

        byte[] bytes = host.GetAddressBytes();
        byte[][] byteRange = new byte[bytes.Length][];
        for ( int i = 0; i <= 3; i++ )
            byteRange[i] = new byte[] { bytes[i] == 255 ? ( byte ) 1 : bytes[i], bytes[i] == 255 ? ( byte ) 254 : bytes[i] };

        for ( byte i = byteRange[0][0]; i < byteRange[0][1]; i++ )
        {
            bytes[0] = i;
            for ( byte j = byteRange[1][0]; j < byteRange[1][1]; j++ )
            {
                bytes[1] = j;
                for ( byte k = byteRange[2][0]; k < byteRange[2][1]; k++ )
                {
                    bytes[2] = k;
                    for ( byte l = byteRange[3][0]; l < byteRange[2][1]; l++ )
                    {
                        bytes[3] = l;
                        IPAddress address = new( bytes );
                        addresses.Add( address );
                    }
                }
            }
        }
        return addresses.ToArray();
    }

    /// <summary>
    /// Lists the <see cref="IPEndPoint"/>s of the VXI-11 Core devices that are listening on the
    /// addresses spanned by the specified <paramref name="broadcastAddress"/>
    /// which might include a broadcast address (255) as a host address, such as in 192.168.1.255.
    /// </summary>
    /// <remarks>   2023-02-01. </remarks>
    /// <param name="broadcastAddress">             The broadcast address. </param>
    /// <param name="connectTimeout">               The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPEndPoint"/> </returns>
    public static List<IPEndPoint> ListCoreDevicesEndpoints( IPAddress broadcastAddress, int connectTimeout, bool startEmbeddedPortmapService )
    {
        IPAddress[] addresses = EnumerateAddresses( broadcastAddress );
        return ListCoreDevicesEndpoints( addresses, connectTimeout, startEmbeddedPortmapService, true );
    }

    /// <summary>
    /// Lists the <see cref="IPAddress"/>s of the VXI-11 Core devices that are listening on the.
    /// addresses spanned by the specified <paramref name="broadcastAddress"/>
    /// which might include a broadcast address (255) as a host address, such as in 192.168.1.255.
    /// </summary>
    /// <remarks>   2023-02-01. </remarks>
    /// <param name="broadcastAddress">             The broadcast address. </param>
    /// <param name="connectTimeout">               The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPAddress"/> </returns>
    public static List<IPAddress> ListCoreDevicesAddresses( IPAddress broadcastAddress, int connectTimeout, bool startEmbeddedPortmapService )
    {
        IPAddress[] addresses = EnumerateAddresses( broadcastAddress );
        return ListCoreDevicesAddresses( addresses, connectTimeout, startEmbeddedPortmapService );
    }

    /// <summary>
    /// Lists the <see cref="IPAddress"/>s of the VXI-11 Core devices that are listening on the
    /// specified <paramref name="addresses"/>.
    /// </summary>
    /// <param name="addresses">                    The hosts. </param>
    /// <param name="connectTimeout">               The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPAddress"/> </returns>
    public static List<IPAddress> ListCoreDevicesAddresses( IEnumerable<IPAddress> addresses, int connectTimeout, bool startEmbeddedPortmapService )
    {
        // start the embedded service.
        if ( startEmbeddedPortmapService )
        {
            using OncRpcEmbeddedPortmapServiceStub epm = DeviceExplorer.StartEmbeddedPortmapService();
        }

        List<IPAddress> ipAddresses = new();
        foreach ( IPAddress address in addresses )
        {
            if ( DeviceExplorer.PortmapPingHost( address, connectTimeout ) )
                ipAddresses.Add( address );
        }
        return ipAddresses;
    }

    /// <summary>
    /// Lists the <see cref="IPEndPoint"/>s of the VXI-11 Core devices that are listening on the
    /// specified <paramref name="addresses"/>.
    /// </summary>
    /// <param name="addresses">                    The hosts. </param>
    /// <param name="connectTimeout">               The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded portmap service. </param>
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
        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. 
        using OncRpcPortmapClient pmapClient = new( host, OncRpcProtocol.OncRpcTcp, connectTimeout );
        pmapClient.OncRpcClient!.IOTimeout = OncRpcTcpClient.IOTimeoutDefault;

        // get a port from this host, if any.
        return pmapClient.GetPort( Vxi11ProgramConstants.CoreProgram, Vxi11ProgramConstants.CoreVersion, OncRpcProtocol.OncRpcTcp );
    }

    #endregion 

    #region " enumerate registered servers "

    /// <summary>
    /// Enumerate the endpoints or registered servers on the specified <paramref name="addresses"/>.
    /// </summary>
    /// <param name="addresses">                        The hosts. </param>
    /// <param name="timeout">                      The timeout in milliseconds. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded portmap service. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPEndPoint"/> </returns>
    public static List<IPEndPoint> EnumerateRegisteredServers( IEnumerable<IPAddress> addresses, int timeout, bool startEmbeddedPortmapService )
    {

        // start the embedded service.
        if ( startEmbeddedPortmapService )
        {
            using OncRpcEmbeddedPortmapServiceStub epn = DeviceExplorer.StartEmbeddedPortmapService();
        }

        // enumerate the listening devices.
        return EnumerateRegisteredServers( addresses, timeout );
    }

    /// <summary>
    /// Enumerate the registered servers on the specified <paramref name="hosts"/>.
    /// </summary>
    /// <param name="hosts">    The hosts. </param>
    /// <param name="timeout">  The timeout in milliseconds. </param>
    /// <returns>   The <see cref="List{T}"/> where T:<see cref="IPEndPoint"/> </returns>
    public static List<IPEndPoint> EnumerateRegisteredServers( IEnumerable<IPAddress> hosts, int timeout )
    {
        List<IPEndPoint> registeredDevices = new();

        foreach ( IPAddress host in hosts )
        {
            foreach ( var registeredDevice in EnumerateRegisteredServers( host, timeout ) )
            {
                registeredDevices.Add( registeredDevice );
            }
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

        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. 
        using OncRpcPortmapClient pmapClient = new( host, OncRpcProtocol.OncRpcTcp, connectTimeout );
        pmapClient.OncRpcClient!.IOTimeout = OncRpcTcpClient.IOTimeoutDefault;

        // Now dump the current list of registered servers.
        OncRpcServerIdentifierCodec[] registeredServers = pmapClient.ListRegisteredServers(); ;
        foreach ( OncRpcServerIdentifierCodec registeredServer in registeredServers )
        {
            if ( registeredServer.Port > 0 ) { endpoints.Add( new IPEndPoint(host, registeredServer.Port) ); }
        }
        return endpoints;
    }

    #endregion

    #region " socket "

    /// <summary>   Pings port. </summary>
    /// <param name="ipv4Address">          The IPv4 address. </param>
    /// <param name="portNumber">           (Optional) The port number. </param>
    /// <param name="timeoutMilliseconds">  (Optional) The timeout in milliseconds. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool Paping( IPAddress ipv4Address, int portNumber = 5025, int timeoutMilliseconds = 10 )
    {
        return Paping( ipv4Address.ToString(), portNumber, timeoutMilliseconds );
    }

    /// <summary>   Pings port. </summary>
    /// <param name="ipv4Address">          The IPv4 address. </param>
    /// <param name="portNumber">           (Optional) The port number. </param>
    /// <param name="timeoutMilliseconds">  (Optional) The timeout in milliseconds. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool Paping( string ipv4Address, int portNumber = 5025, int timeoutMilliseconds = 10 )
    {
        return Paping( new IPEndPoint( IPAddress.Parse( ipv4Address ), portNumber), timeoutMilliseconds );
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

