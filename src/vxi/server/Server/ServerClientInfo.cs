using System.Collections;

namespace cc.isr.VXI11.Server;

/// <summary>   Information about the server client. </summary>
public class ServerClientInfo : IEquatable<ServerClientInfo>
{

    /// <summary>   Constructor. </summary>
    /// <param name="clientId"> The identifier of the client. </param>
    /// <param name="linkId">   The identifier of the link. </param>
    public ServerClientInfo( int clientId, int linkId )
    {
        this.ClientId = clientId;
        this.LinkId = linkId;
        this._handle = Array.Empty<byte>();
    }

    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    public int ClientId { get; set; }

    /// <summary>   Gets or sets the identifier of the link. </summary>
    /// <value> The identifier of the link. </value>
    public int LinkId { get; set; }

    private byte[] _handle;

    /// <summary>   Gets the handle. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <returns>   An array of byte. </returns>
    public byte[] GetHandle() { return _handle; }

    /// <summary>   Gets or sets a value indicating whether the interrupt is enabled. </summary>
    /// <value> True if interrupt enabled, false if not. </value>
    public bool InterruptEnabled { get; private set; }

    /// <summary>   Sets a handle. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="enable">   True to enable, false to disable. </param>
    /// <param name="handle">   The handle. </param>
    public void EnableInterrupt( bool enable, byte[] handle )
    {
        this._handle = handle;
        this.InterruptEnabled = enable;
    }

    public bool Equals( ServerClientInfo other )
    {
        return ( other is not null )
            && Enumerable.SequenceEqual( this._handle , other.GetHandle() )
            && ClientId == other.ClientId
            && LinkId == other.LinkId
            && InterruptEnabled == other.InterruptEnabled;
        throw new NotImplementedException();
    }

}
