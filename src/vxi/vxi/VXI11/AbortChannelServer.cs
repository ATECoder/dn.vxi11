using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Codecs;

using System.Net;

namespace cc.isr.VXI11;

/// <summary>
/// The VXI-11 <see cref="AbortChannelServer"/> class serves the <see cref="Vxi11ProgramConstants.AsyncProgram"/> 
/// for the <see cref="Vxi11Message.DeviceAbortProcedure"/>.
/// </summary>
/// <remarks>
/// A network instrument server's abort channel is typically implemented as an interrupt or signal handler in a
/// single threaded operating system, or as a higher priority thread in a multi-threaded operating system.
/// </remarks>
public class AbortChannelServer : OncRpcServerStubBase, IOncRpcDispatchable
{

    /// <summary>   The default value of the abort port number. </summary>
    public static int AbortPortDefault = 440;

    #region " construction and cleanup "

    /// <summary>   Default constructor. </summary>
    public AbortChannelServer() : this( 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port. </param>
    public AbortChannelServer( int port ) : this( IPAddress.Any, port )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="bindAddr"> The bind address. </param>
    /// <param name="port">     The port. </param>
    public AbortChannelServer( IPAddress bindAddr, int port )
    {
        this._ipv4Address = bindAddr;
        this.PortNumber = port;

        OncRpcProgramInfo[] registeredPrograms = new OncRpcProgramInfo[] {
            new OncRpcProgramInfo(Vxi11ProgramConstants.AsyncProgram, Vxi11ProgramConstants.AsyncVersion),
        };
        this.SetRegisteredPrograms( registeredPrograms );

        OncRpcTransportBase[] transports = new OncRpcTransportBase[] {
        new OncRpcUdpTransport(this, bindAddr, port, registeredPrograms, OncRpcTransportBase.BufferSizeDefault),
        new OncRpcTcpTransport(this, bindAddr, port, registeredPrograms, OncRpcTransportBase.BufferSizeDefault)
        };
        this.SetTransports( transports );
    }

    #endregion

    #region " members "

    private int _portNumber;
    /// <summary>   Gets or sets the port number. </summary>
    /// <value> The port number. </value>
    public int PortNumber
    {
        get => this._portNumber;
        set => _ = this.SetProperty( ref this._portNumber, value );
    }

    private IPAddress _ipv4Address;
    /// <summary>   Gets or sets the host IPv4 address of this server. </summary>
    /// <value> The IPv4 address. </value>
    public IPAddress IPv4Address
    {
        get => this._ipv4Address;
        set => _ = this.SetProperty( ref this._ipv4Address, value );
    }

    #endregion

    #region " action " 

    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <remarks>
    /// This interface has some fairly deep semantics, so please read the description above for how
    /// to use it properly. For background information about fairly deep semantics, please also refer
    /// to <i>Gigzales</i>, <i>J</i>.: Semantics considered harmful. Addison-Reilly, 1992, ISBN 0-542-
    /// 10815-X. <para>
    ///  
    /// See the introduction to this class for examples of how to use this interface properly.</para>
    /// </remarks>
    /// <param name="call">         <see cref="T:cc.isr.ONC.RPC.Server.OncRpcCallInformation" />
    ///                             about the call to handle, like the caller's Internet address, the ONC/RPC
    ///                             call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    <see cref="Vxi11Message">procedure</see>/> number requested. </param>
    public void DispatchOncRpcCall( OncRpcCallHandler call, int program, int version, int procedure )
    {
        if ( version == Vxi11ProgramConstants.InterruptVersion )
            switch ( procedure )
            {
                case ( int ) Vxi11Message.DeviceAbortProcedure:
                    {
                        DeviceLink link = new();
                        call.RetrieveCall( link );
                        DeviceError result = this.HandleAbortRequest( link );
                        call.Reply( result );
                        break;
                    }
                default:
                    call.ReplyProcedureNotAvailable();
                    break;
            }
        else
            call.ReplyProgramNotAvailable();
    }

    #endregion

    #region " event handlers "

    public event EventHandler<DeviceErrorEventArgs>? AbortRequested;

    /// <summary>   Executes the <see cref="AbortRequested"/> event. </summary>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    private DeviceError OnAbortRequested( DeviceErrorEventArgs e )
    {
        var handler = this.AbortRequested;
        handler?.Invoke( this, e );
        return new DeviceError( e.ErrorCodeValue );
    }

    #endregion

    #region " handle remote procedure dispatch."

    /// <summary>
    /// Handles the remote <see cref="Vxi11Message.DeviceAbortProcedure"/> request.
    /// </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="link"> The request of type <see cref="Codecs.DeviceLink"/> to send to the remote
    ///                     procedure call. </param>
    /// <returns>   A DeviceError. </returns>
    public virtual DeviceError HandleAbortRequest( DeviceLink link )
    {
        return link == null
            ? new DeviceError()
            : this.OnAbortRequested( new DeviceErrorEventArgs() );
    }

    #endregion

}
