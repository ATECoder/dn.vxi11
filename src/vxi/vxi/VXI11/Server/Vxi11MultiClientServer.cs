using System.Net;

namespace cc.isr.VXI11.Server;

/// <summary>   A VXI-11 server capable of serving multiple clients. </summary>
/// <remarks> Implements the minimum requirements for a VXI-11 including an <see cref="AbortChannelServer"/>
/// and <see cref="InterruptChannelClient"/></remarks>
public partial class Vxi11MultiClientServer : Vxi11Server
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public Vxi11MultiClientServer() : this( new Vxi11Device( new Vxi11Instrument() ), IPAddress.Any, 0 )
    {
        throw new NotImplementedException();
    }


    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Vxi11MultiClientServer( IVxi11Device device, IPAddress bindAddr, int port = 0 ) : base( device, bindAddr, port )
    {
        throw new NotImplementedException();
    }

    #endregion

}
