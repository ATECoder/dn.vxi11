
using System.Collections.Concurrent;
using System.ComponentModel.Design;

namespace cc.isr.VXI11.Server;


/// <summary>   Manager for clients. </summary>
public class ClientsManager
{

    /// <summary>   Default constructor. </summary>
    public ClientsManager()
    {
        this.LinkedClients = new();
        this.ClientLinks = new(); 
    }

    private ConcurrentDictionary<int, int> LinkedClients { get; set; }

    private ConcurrentDictionary<int, int> ClientLinks { get; set; }


    private int _activeLinkId;

    /// <summary>   Gets or sets the active link id. </summary>
    /// <value> The active link. </value>
    public int ActiveLinkId
    {
        get => this._activeLinkId;
        set {
            if ( this._activeLinkId != value )
            {
                this._activeLinkId = value;
                this.ActiveClientId = this.GetClient( value );
            }
        }
    }

    private int _activeClientId;

    /// <summary>   Gets or sets the active client id. </summary>
    /// <value> The active client. </value>
    public int ActiveClientId
    {
        get => this._activeClientId;
        set {
            if ( this._activeClientId != value )
            {
                this._activeClientId = value;
                this.ActiveLinkId = this.GetLink( value );
            }
        }
    }

    /// <summary>   Query if this object is active client linked. </summary>
    /// <returns>   True if active client linked, false if not. </returns>
    public bool IsActiveClientLinked()
    {
        return this._activeClientId != 0;
    }

    /// <summary>   Query if a client, identified by the 'clientId' was linked. </summary>
    /// <param name="clientId"> Identifier for the client. </param>
    /// <returns>   True if client linked, false if not. </returns>
    public bool IsClientLinked( int clientId )
    {
        return this.ClientLinks.ContainsKey( clientId );
    }

    /// <summary>   Query if a link identified by the 'linkId' was created. </summary>
    /// <param name="linkId">   Identifier for the link. </param>
    /// <returns>   True if link created, false if not. </returns>
    public bool IsLinkCreated( int linkId )
    {
        return this.LinkedClients.ContainsKey( linkId );
    }

    /// <summary>   Query if 'linkId' contains client. </summary>
    /// <param name="linkId">   Identifier for the link. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool ContainsClient(int linkId)
        { return this.LinkedClients.ContainsKey(linkId); }

    /// <summary>   Query if 'clientId' contains link. </summary>
    /// <param name="clientId"> Identifier for the client. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool ContainsLink( int clientId )
        { return this.ClientLinks.ContainsKey(clientId); }

    /// <summary>   Gets a client. </summary>
    /// <param name="linkId">   The link identifier. </param>
    /// <returns>   The client. </returns>
    public int GetClient(int linkId)
    {
        if ( !this.LinkedClients.TryGetValue( linkId, out int value ) )
             value = 0;
        return value;
    }

    /// <summary>   Gets client count. </summary>
    /// <returns>   The client count. </returns>
    public int GetClientCount() { return this.LinkedClients.Count; }

    /// <summary>   Gets a Link. </summary>
    /// <param name="clientId">   The identifier. </param>
    /// <returns>   The Link. </returns>
    public int GetLink( int clientId )
    {
        if ( !this.ClientLinks.TryGetValue( clientId, out int value ) )
            value = 0;
        return value;
    }

    /// <summary>   Gets Link count. </summary>
    /// <returns>   The Link count. </returns>
    public int GetLinkCount() { return this.ClientLinks.Count; }

    /// <summary>   Adds a client to 'linkId'. </summary>
    /// <param name="clientId"> Identifier for the client. </param>
    /// <param name="linkId">   Identifier for the link. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool AddClient( int clientId, int linkId )
    {
        if ( this.LinkedClients.ContainsKey( linkId ) ) { return false; }
        if ( this.ClientLinks.ContainsKey( clientId ) ) { return false; }
        _ = this.LinkedClients.GetOrAdd( linkId, clientId );
        _ = this.ClientLinks.GetOrAdd( clientId, linkId );
        return true;
    }

    /// <summary>   Removes the linked client described by <paramref name="linkId"/>. </summary>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool RemoveClient( int linkId )
    {
        int clientId  = this.GetClient( linkId );
        if ( clientId != 0 )
        {
            bool removed = this.LinkedClients.TryRemove( linkId, out int removedClient )
                   && this.ClientLinks.TryRemove( clientId, out int removedLink );
            if ( removed && clientId == this._activeClientId )
            {
                // clear the active client and link
                this._activeClientId = 0;
                this._activeLinkId= 0;
            }
            return removed;
        }
        else
        { return false; }
    }

}
