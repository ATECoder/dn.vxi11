using System.Net;

namespace cc.isr.VXI11.Server;

/// <summary>   A VXI-11 server capable of serving a single client. </summary>
/// <remarks> Implements the minimum requirements for a VXI-11 including an <see cref="AbortChannelServer"/>
/// and <see cref="InterruptChannelClient"/></remarks>
public partial class Vxi11SingleClientServer : Vxi11Server
{

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public Vxi11SingleClientServer() : this( new Vxi11Device ( new Vxi11Instrument(), new Vxi11Interface() ), IPAddress.Any, 0 )
    {
    }


    /// <summary>   Constructor. </summary>
    /// <param name="device">   current device. </param>
    /// <param name="bindAddr"> The local Internet Address the server will bind to. </param>
    /// <param name="port">     The port number where the server will wait for incoming calls. </param>
    public Vxi11SingleClientServer( IVxi11Device device, IPAddress bindAddr, int port = 0 ) : base( device, bindAddr, port )
    {
    }

    #endregion

}
