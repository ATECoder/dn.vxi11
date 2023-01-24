using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using cc.isr.ONC.RPC.Client;
using cc.isr.ONC.RPC.Codecs;
using cc.isr.ONC.RPC.Portmap;

namespace cc.isr.VXI11;

public class DeviceExplorer
{

    #region " embedded port mapper "

    /// <summary>   Starts embedded portmap service. </summary>
    public static OncRpcEmbeddedPortmapService StartEmbeddedPortmapService()
    {
        // start the embedded service.
        return OncRpcEmbeddedPortmapService.StartEmbeddedPortmapService();
    }

    /// <summary>   Ping the host using the <see cref="OncRpcPortmapClient"/> service. </summary>
    /// <remarks> TODO: Timeout does not seem to make a difference. </remarks>
    /// <param name="host">     The host. </param>
    /// <param name="transmitTimeout">  The transmit timeout, which sets the socket timeouts during the
    /// transmission of messages to the service. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool PortmapPingHost( IPAddress host, int transmitTimeout )
    {
        return OncRpcPortmapClient.TryPingPortmapService( host, transmitTimeout );
    }

    #endregion 


    #region " enumerate core devices "

    /// <summary>
    /// Lists the VXI-11 Core devices that are listening on the specified <paramref name="hosts"/>.
    /// </summary>
    public static List<(IPAddress host, int port)> ListCoreDevices( IEnumerable<IPAddress> hosts, int connectTimeout, bool startEmbeddedPortmapService )
    {

        // start the embedded service.
        if ( startEmbeddedPortmapService ) _ = DeviceExplorer.StartEmbeddedPortmapService();

        // enumerate the listening devices.
        return ListCoreDevices( hosts, connectTimeout );
    }

    /// <summary>
    /// Lists the VXI-11 Core devices that are listening on the specified <paramref name="hosts"/>.
    /// </summary>
    /// <param name="hosts">    The hosts. </param>
    /// <param name="connectTimeout">  The timeout. </param>
    /// <returns>   The List{(IPAddress host,int port)}; </returns>
    public static List<(IPAddress host, int port)> ListCoreDevices( IEnumerable<IPAddress> hosts, int connectTimeout )
    {
        List<(IPAddress host, int port)> devices = new();
        foreach ( IPAddress host in hosts )
        {
            int port = GetCoreDevicePortNumber( host, connectTimeout );
            if ( port > 0 ) { devices.Add( (host, port) ); }
        }
        return devices;
    }

    /// <summary>
    /// Gets the port of any VXI-11 Core device that is listening on the specific <paramref name="host"/> address.
    /// </summary>
    /// <param name="host">     The host. </param>
    /// <param name="connectTimeout">  The timeout. </param>
    /// <returns>   The port on which a device is listening (if value > 0 ). </returns>
    public static int GetCoreDevicePortNumber( IPAddress host, int connectTimeout )
    {
        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. 
        using OncRpcPortmapClient pmapClient = new( host, OncRpcProtocols.OncRpcTcp, connectTimeout );
        pmapClient.OncRpcClient.IOTimeout = OncRpcTcpClient.IOTimeoutDefault;

        // get a port from this host, if any.
        return pmapClient.GetPort( Vxi11ProgramConstants.DeviceCoreProgram, Vxi11ProgramConstants.DeviceCoreVersion, OncRpcProtocols.OncRpcTcp );
    }

    #endregion 

    #region " enumerate registered servers "

    /// <summary>
    /// Enumerate the registered servers on the specified <paramref name="hosts"/>.
    /// </summary>
    /// <param name="hosts">                        The hosts. </param>
    /// <param name="timeout">                      The timeout. </param>
    /// <param name="startEmbeddedPortmapService">  True to start embedded portmap service. </param>
    /// <returns>   The List{(IPAddress host,int port)} </returns>
    public static List<(IPAddress host, int port)> EnumerateRegisteredServers( IEnumerable<IPAddress> hosts, int timeout, bool startEmbeddedPortmapService )
    {

        // start the embedded service.
        if ( startEmbeddedPortmapService ) _ = DeviceExplorer.StartEmbeddedPortmapService();

        // enumerate the listening devices.
        return EnumerateRegisteredServers( hosts, timeout );
    }

    /// <summary>
    /// Enumerate the registered servers on the specified <paramref name="hosts"/>.
    /// </summary>
    /// <param name="hosts">    The hosts. </param>
    /// <param name="timeout">  The timeout. </param>
    /// <returns>   The List{(IPAddress host,int port)} </returns>
    public static List<(IPAddress host, int port)> EnumerateRegisteredServers( IEnumerable<IPAddress> hosts, int timeout )
    {
        List<(IPAddress host, int port)> registeredDevices = new();

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
    /// <param name="connectTimeout">  The timeout. </param>
    /// <returns>   The List{(IPAddress host,int port)} </returns>
    public static List<(IPAddress host, int port)> EnumerateRegisteredServers( IPAddress host, int connectTimeout )
    {
        List<(IPAddress host, int port)> registeredDevices = new();

        // Create a portmap client object, which can then be used to contact
        // a local or remote ONC/RPC portmap process. 
        using OncRpcPortmapClient pmapClient = new( host, OncRpcProtocols.OncRpcTcp, connectTimeout );
        pmapClient.OncRpcClient.IOTimeout= OncRpcTcpClient.IOTimeoutDefault;

        // Now dump the current list of registered servers.
        OncRpcServerIdentifierCodec[] registeredServers = pmapClient.ListRegisteredServers(); ;
        foreach ( OncRpcServerIdentifierCodec registeredServer in registeredServers )
        {
            if ( registeredServer.Port > 0 ) { registeredDevices.Add( (host, registeredServer.Port) ); }
        }
        return registeredDevices;
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
        try
        {
            using Socket socket = new( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            socket.Blocking = true;
            IAsyncResult result = socket.BeginConnect( ipv4Address, portNumber, null, null );
            bool success = result.AsyncWaitHandle.WaitOne( timeoutMilliseconds, true );
            if ( socket.Connected )
            {
                socket.EndConnect( result );
                socket.Shutdown( SocketShutdown.Both );
                socket.Close();
                // this is required for the server to recover after the socket is closed.
                System.Threading.Thread.Sleep( 1 );
                return true;
            }
            else
            {
                socket.Close();
                return false;
            }
        }
        catch
        {
            return false;
        }
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

