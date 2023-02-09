using System;
using System.Collections.Generic;
using System.Text;

namespace cc.isr.VXI11.Server
{
    /// <summary>   Manager for clients. </summary>
    public class ClientsManager
    {

        /// <summary>   Default constructor. </summary>
        public ClientsManager()
        {
            this.LinkedClients = new();
            this.ClientLinks = new(); 
        }

        private System.Collections.Generic.Dictionary<int, int> LinkedClients { get; set; }

        private System.Collections.Generic.Dictionary<int, int> ClientLinks { get; set; }

        /// <summary>   Query if 'linkId' contains client. </summary>
        /// <param name="linkId">   Identifier for the link. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public bool ContainsClient(int linkId)
            { return LinkedClients.ContainsKey(linkId); }

        /// <summary>   Query if 'clientId' contains link. </summary>
        /// <param name="clientId"> Identifier for the client. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public bool ContainsLink( int clientId )
            { return ClientLinks.ContainsKey(clientId); }

        /// <summary>   Gets a client. </summary>
        /// <param name="id">   The identifier. </param>
        /// <returns>   The client. </returns>
        public int GetClient(int id) { return LinkedClients[id]; }

        /// <summary>   Gets client count. </summary>
        /// <returns>   The client count. </returns>
        public int GetClientCount() { return LinkedClients.Count; }

        /// <summary>   Adds a client to 'linkId'. </summary>
        /// <param name="clientId"> Identifier for the client. </param>
        /// <param name="linkId">   Identifier for the link. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public bool AddClient( int clientId, int linkId )
        {
            if ( this.LinkedClients.ContainsKey( linkId ) ) { return false; }
            if ( this.ClientLinks.ContainsKey( clientId ) ) { return false; }
            this.LinkedClients.Add( linkId, clientId );
            this.ClientLinks.Add( clientId, linkId );
            return true;
        }
    }
}
