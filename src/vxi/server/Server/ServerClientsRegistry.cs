
using System.Collections.Concurrent;
using System.ComponentModel.Design;

using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Server;


/// <summary>   Manager for server clients. </summary>
/// <remarks>   2023-02-09. </remarks>
public class ServerClientsRegistry
{

    /// <summary>   Default constructor. </summary>
    public ServerClientsRegistry()
    {
        this.LinkedClients = new();
        this.ClientLinks = new();
        this.ClientsQueue = new();
    }

    /// <summary>   Gets or sets a queue of clients locking the instrument. </summary>
    /// <value> A queue of clients locking the instrument. </value>
    private ConcurrentQueue<ServerClientInfo> ClientsQueue { get; set; }

    /// <summary>   Gets or sets the dictionary of <see cref="ServerClientInfo"/> keyed by <see cref="ServerClientInfo.LinkId"/>. </summary>
    /// <value> The <see cref="ServerClientInfo"/> keyed by <see cref="ServerClientInfo.LinkId"/>. </value>
    private ConcurrentDictionary<int, ServerClientInfo> LinkedClients { get; set; }

    /// <summary>   Gets or sets the dictionary of links keyed by <see cref="ServerClientInfo.ClientId"/>. </summary>
    /// <value> The links keyed by <see cref="ServerClientInfo.ClientId"/>. </value>
    private ConcurrentDictionary<int, int> ClientLinks { get; set; }

    /// <summary>   Gets the number of clients. </summary>
    /// <value> The number of clients. </value>
    public int Count => this.LinkedClients.Count;

    /// <summary>   Gets or sets information describing the active server client. </summary>
    /// <value> Information describing the active server client. </value>
    public ServerClientInfo? ActiveServerClient { get; set; }

    /// <summary>   Attempts to select a client. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="linkId">       The link identifier. </param>
    /// <param name="lockTimeout">  (Optional) The update lock timeout. </param>
    /// <returns>   The client. </returns>
    public bool TrySelectClient( int linkId, int? lockTimeout = null )
    {
        if ( this.LinkedClients.TryGetValue( linkId, out ServerClientInfo value ) )
        {
            this.ActiveServerClient = value;
            this.ActiveServerClient.ActivateLockTimeout( lockTimeout );
            return true;
        }
        else
        {
            this.ActiveServerClient = null;
        }
        return false;
    }


    /// <summary>   Query if this object is active client linked. </summary>
    /// <returns>   True if active client linked, false if not. </returns>
    public bool IsActiveClientLinked()
    {
        return 0 != (this.ActiveServerClient?.LinkId ?? 0);
    }

    /// <summary>   Query if this object is active client locked. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <returns>   True if active client locked, false if not. </returns>
    public bool IsActiveClientLocked()
    {
        return this.ActiveServerClient?.IsLocked() ?? false;
    }

    /// <summary>   Query if 'linkId' is locked. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">   The link identifier. </param>
    /// <returns>   True if locked, false if not. </returns>
    public bool IsLocked( int linkId )
    {
        return this.LinkedClients.ContainsKey( linkId)  && this.LinkedClients[linkId].IsLocked();
    }

    /// <summary>   Releases the lock described by linkId. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="linkId">   The link identifier. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public bool ReleaseLock( int linkId )
    {
        if ( this.LinkedClients.ContainsKey( linkId ) && this.LinkedClients[linkId].IsLocked() )
        {
            this.LinkedClients[linkId].ReleaseLockTimeout();
            return true;
        }
        return false;
    }

    /// <summary>   Await lock release asynchronous. </summary>
    /// <remarks>   2023-02-14. </remarks>
    public bool AwaitLockReleaseAsync()
    {
        if ( this.ActiveServerClient is not null )
        {
            Task<bool> awaitingTask = this.AwaitLockReleaseAsync( this.ActiveServerClient.LockTimeout, 5 );
            return awaitingTask.Wait( this.ActiveServerClient.LockTimeout + 2 ) && awaitingTask.Result;
        }
        return true;
    }

    /// <summary>   Await lock release asynchronous. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="timeout">      The timeout. </param>
    /// <param name="loopDelay">    The loop delay. </param>
    /// <returns>   The awaiting task. </returns>
    public virtual async Task<bool> AwaitLockReleaseAsync( int timeout, int loopDelay )
    {
        bool result = false;
        await Task.Factory.StartNew( () => { result = this.AwaitLockRelease( this.ActiveServerClient!.LockTimeout, loopDelay ); } );
        return result;
    }

    /// <summary>   Await lock release. </summary>
    /// <remarks>   2023-02-14. </remarks>
    /// <param name="timeout">      The timeout. </param>
    /// <param name="loopDelay">    The loop delay. </param>
    /// <returns>   <see langword="true"/> if it the lock was released; otherwise, <see langword="false"/>  if it fails. </returns>
    public bool AwaitLockRelease( int timeout, int loopDelay )
    {
        // await for the server to stop running
        DateTime endTime = DateTime.Now.AddMilliseconds( timeout );
        while ( ( this.ActiveServerClient?.IsLocked() ?? false) && endTime > DateTime.Now )
        {
            Task.Delay( loopDelay ).Wait();
        }
        return  !( this.ActiveServerClient?.IsLocked() ?? false ); 
    }

    /// <summary>   Query if 'clientId' is active client. </summary>
    /// <param name="clientId"> Identifier for the client. </param>
    /// <returns>   True if active client, false if not. </returns>
    public bool IsActiveClient( int clientId )
    {
        return clientId != (this.ActiveServerClient?.ClientId ?? 0);
    }

    /// <summary>   Query if 'linkId' is active link. </summary>
    /// <param name="linkId">   The link identifier. </param>
    /// <returns>   True if active link, false if not. </returns>
    public bool IsActiveLink( int linkId )
    {
        return linkId == (this.ActiveServerClient?.LinkId ?? 0);
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

    /// <summary>   Adds an active client. </summary>
    /// <remarks>   2023-02-13. </remarks>
    /// <param name="clientInfo">   Information describing the client. </param>
    /// <returns>   The link id for the added client. </returns>
    private int AddActiveClient( ServerClientInfo clientInfo )
    {
        // make this client the active server client and sent it the reply.
        _ = this.LinkedClients.GetOrAdd( clientInfo.LinkId, clientInfo );
        clientInfo.ActivateLockTimeout();
        this.ClientsQueue.Enqueue( clientInfo );
        return this.ClientLinks.GetOrAdd( clientInfo.ClientId, clientInfo.LinkId );
    }

    /// <summary>   Adds a client to the client collection and makes it the active client. </summary>
    /// <remarks>   2023-02-13. </remarks>
    /// <param name="createLinkParameters"> The parameters defining the created link. </param>
    /// <param name="linkId">       Identifier for the link. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    public bool AddClient( CreateLinkParms createLinkParameters, int linkId )
    {
        if ( this.LinkedClients.ContainsKey( linkId ) ) { return false; }
        if ( this.ClientLinks.ContainsKey( createLinkParameters.ClientId) ) { return false; }
        this.ActiveServerClient = new ( createLinkParameters, linkId );
        int link = this.AddActiveClient( this.ActiveServerClient );
        return link == this.ActiveServerClient.LinkId;
    }

    /// <summary>   Removes the linked client described by <paramref name="linkId"/>. </summary>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    public bool RemoveClient( int linkId )
    {
        if ( linkId != 0 )
        {
            bool removed = this.LinkedClients.TryRemove( linkId, out ServerClientInfo removedClient )
                         & this.ClientLinks.TryRemove( removedClient.ClientId, out int removedLink );

            // check if removing the current info.
            if ( removed && removedClient.ClientId == this.ActiveServerClient?.ClientId
                 && removedLink == this.ActiveServerClient?.LinkId )
            {
                // clear the active client info
                this.ActiveServerClient = null;
            }
            return removed;
        }
        else
        { return false; }
    }

    /// <summary>   Enables the interrupt. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="linkId">   The link identifier. </param>
    /// <param name="enable">   True to enable, false to disable. </param>
    /// <param name="handle">   The handle. </param>
    /// <returns>   <see langword="true"/> if it succeeds; otherwise, <see langword="false"/>. </returns>
    public bool EnableInterrupt( int linkId, bool enable, byte[] handle )
    {
        if ( this.IsClientLinked( linkId ) )
        {
            this.LinkedClients[linkId].EnableInterrupt( enable, handle );
            if ( this.ActiveServerClient is null || this.ActiveServerClient.LinkId != linkId )
            {
                return true;
            }
            this.ActiveServerClient.EnableInterrupt( enable, handle );
            return true;
        }
        else
            return false;
    }

}
