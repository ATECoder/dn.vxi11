using System.Net;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.IEEE488;


/// <summary>   A VXI-11 client. </summary>
/// <remarks>   2022-12-02. </remarks>
public class Ieee488Client : IDisposable
{

    #region " construction and cleanup "

    private DeviceCoreClient _coreClient;
    private DeviceLink _link;

    /// <summary>
    /// Connect
    /// </summary>
    /// <param name="ipv4Address">Device IPv4 address</param>
    /// <param name="device">Device name, e.g., inst0 or gpib0,8</param>
    public void Connect( string ipv4Address, string device )
    {
        this._coreClient = new DeviceCoreClient( IPAddress.Parse( ipv4Address ), OncRpcProtocols.OncRpcTcp );
        CreateLinkParms createLinkParam = new() {
            Device = device
        };
        CreateLinkResp linkResp = this._coreClient.CreateLink( createLinkParam );
        this._link = linkResp.DeviceLinkId;
        this.MaxRecvSize = linkResp.MaxReceiveSize;
        this.Connected = true;
    }

    /// <summary>   Query if this object is disposed. </summary>
    /// <remarks>   2022-12-02. </remarks>
    /// <returns>   True if disposed, false if not. </returns>
    public bool IsDisposed()
    {
        return this._coreClient is null;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <remarks>   2022-12-02. </remarks>
    void IDisposable.Dispose()
    {
        this.Dispose( true );
        // Take this object off the finalization(Queue) and prevent finalization code 
        // from executing a second time.
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <remarks>   2022-12-02. </remarks>
    /// <param name="disposing">    True to release both managed and unmanaged resources; false to
    ///                             release only unmanaged resources. </param>
    private void Dispose( bool disposing )
    {
        if ( !this.IsDisposed() && disposing )
            _ = this.Close();
    }

    /// <summary>   Closes this object. </summary>
    /// <remarks>   2022-12-02. </remarks>
    public DeviceError Close()
    {
        this.Connected = false;
        DeviceError deviceError = new();
        if ( this._link is not null )
            try
            {
                deviceError = this._coreClient?.DestroyLink( this._link );
            }
            catch
            {
                throw;
            }
            finally
            {
                this._link = null;
            }
        try
        {
            this._coreClient?.Close();
        }
        catch
        {
            throw;
        }
        finally
        {
            this._coreClient = null;
        }
        return deviceError ?? new DeviceError();
    }

    #endregion

    #region " Properties "

    /// <summary>   Gets or sets the maximum size of the receive. </summary>
    /// <value> The maximum size of the receive. </value>
    public int MaxRecvSize { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the end-or-identify (EOI) terminator is enabled.
    /// </summary>
    /// <remarks>
    /// The driver must be configured so that when talking on the bus it sends a write termination
    /// string (e.g., a line-feed or line-feed followed by a carriage return) with EOI as the
    /// terminator, and when listening on the bus it expects a read termination (e.g., a line-feed)
    /// with EOI as the terminator. The IEEE-488.2 EOI (end-or-identify) message is interpreted as a 
    /// <c>new line</c> character and can be used to terminate a message in place of a <c>new line</c>
    /// character. A <c>carriage return</c> followed by a <c>new line</c> is also accepted. Message
    /// termination will always reset the current SCPI message path to the root level.
    /// </remarks>
    /// <value> True if EOI is enabled, false if not. </value>
    public bool Eoi { get; set; } = true;

    /// <summary>   Gets or sets the read termination. </summary>
    /// <value> The read termination. </value>
    public byte ReadTermination { get; set; } = ( byte ) '\n';

    /// <summary>   Gets or sets the read timeout in milliseconds. </summary>
    /// <value> The read timeout in milliseconds. </value>
    public int ReadTimeout { get; set; } = 1000;

    /// <summary>   Gets or sets the write timeout in milliseconds. </summary>
    /// <value> The write timeout in milliseconds. </value>
    public int WriteTimeout { get; set; } = 1000;

    /// <summary>   Gets or sets the lock timeout in milliseconds. </summary>
    /// <remarks> </remarks>
    /// <value> The lock timeout. </value>
    public int LockTimeout { get; set; } = 1000;

    /// <summary>   Gets or sets the write termination. </summary>
    /// <value> The write termination. </value>
    public byte[] WriteTermination { get; set; } = { ( byte ) '\n' };

    /// <summary>   Gets or sets a value indicating whether the VXI Core Client is connected. </summary>
    /// <value> True if connected, false if not. </value>
    public bool Connected { get; private set; }

    #endregion

    #region " Send and Receive "

    /// <summary>   Query if 'data' is write terminated. </summary>
    /// <remarks>   2022-12-02. </remarks>
    /// <param name="data">         . </param>
    /// <param name="termination">  The termination. </param>
    /// <returns>   True if write terminated, false if not. </returns>
    private static bool IsWriteTerminated( byte[] data, byte[] termination )
    {
        bool terminated = data is not null && termination is not null && termination.Length > 0 && data.Length > termination.Length;
        if ( !terminated ) return terminated;
        for ( int i = 0; i < termination!.Length; i++ )
            terminated &= data![^(i + 1)] == termination[i];
        return terminated;
    }

    /// <summary>   Query if 'data' is write terminated. </summary>
    /// <remarks>   2022-12-02. </remarks>
    /// <param name="data"> . </param>
    /// <returns>   True if write terminated, false if not. </returns>
    private bool IsWriteTerminated( byte[] data )
    {
        return IsWriteTerminated( data, this.WriteTermination );
    }

    /// <summary>   Query if 'data' is query. </summary>
    /// <remarks>   2022-12-02. </remarks>
    /// <param name="data"> . </param>
    /// <returns>   True if query, false if not. </returns>
    private bool IsQuery( byte[] data )
    {
        return data is not null && data.Length > 1 && this.IsWriteTerminated( data ) && Array.IndexOf( data, ( byte ) '?' ) > -1;
    }

    /// <summary>   Send this message. </summary>
    /// <remarks>   2022-12-02. </remarks>
    /// <param name="data"> . </param>
    /// <returns>   A <see cref="DeviceWriteResp">device write response</see> . </returns>
    public DeviceWriteResp Send( byte[] data )
    {
        DeviceWriteResp resp = new();
        if ( this._link is not null && this._coreClient is not null )
        {
            DeviceWriteParms writeParam = new() {
                DeviceLinkId = this._link,
                IOTimeout = this.WriteTimeout, // in ms
                LockTimeout = this.LockTimeout, // in ms
                Flags = new DeviceFlags( this.Eoi ? 0x8 : 0 ),
                Data = data
            };
            resp = this._coreClient.DeviceWrite( writeParam );
        }
        return resp;
    }

    /// <summary>   Send this message to the VXI-11 server. </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <param name="message">  The message. </param>
    /// <returns>   A <see cref="DeviceWriteResp">device write response</see> . </returns>
    public DeviceWriteResp Send( string message )
    {
        return this.Send( Encoding.Default.GetBytes( message ) );
    }

    /// <summary>   Receives a reply from the VXI-11 server. </summary>
    /// <remarks>   2022-12-02. </remarks>
    /// <returns>   A <see cref="DeviceReadResp">device read response</see> . </returns>
    public DeviceReadResp Receive()
    {
        DeviceReadResp resp = new();
        if ( this._link is not null && this._coreClient is not null )
        {
            DeviceReadParms readParam = new() {
                DeviceLinkId = _link,
                RequestSize = this.MaxRecvSize, // response.Length,
                IOTimeout = this.ReadTimeout,
                LockTimeout = this.LockTimeout,
                Flags = new DeviceFlags(),
                TermChar = this.ReadTermination
            };
            resp = this._coreClient.DeviceRead( readParam );
        }
        return resp;
    }

    /// <summary>   Receives a reply from the VXI-11 server. </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <param name="byteCount">    Number of bytes. </param>
    /// <returns>   A <see cref="DeviceReadResp">device read response</see> . </returns>
    public DeviceReadResp Receive( int byteCount )
    {
        DeviceReadResp resp = new();
        if ( this._link is not null && this._coreClient is not null )
        {
            DeviceReadParms readParam = new() {
                DeviceLinkId = _link,
                RequestSize = byteCount,
                IOTimeout = this.ReadTimeout,
                LockTimeout = this.LockTimeout,
                Flags = new DeviceFlags(),
                TermChar = this.ReadTermination
            };
            resp = this._coreClient.DeviceRead( readParam );
        }
        return resp;
    }

    /// <summary>   Send and receive if query. </summary>
    /// <remarks>   2022-12-12. </remarks>
    /// <param name="data">                     . </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay. </param>
    /// <returns>   A Tuple. </returns>
    public (DeviceWriteResp writeResponse, DeviceReadResp readResponse) SendReceive( byte[] data, int millisecondsReadDelay = 3 )
    {
        DeviceWriteResp writeResponse = this.Send( data );
        if ( writeResponse.Error.Value == OncRpcException.OncRpcSuccess )
            if ( this.IsQuery( data ) )
            {
                Thread.Sleep( millisecondsReadDelay );
                DeviceReadResp readResponse = this.Receive();
                return (writeResponse, readResponse);
            }
            else
                return (writeResponse, new DeviceReadResp());
        else
            // RPC error
            return (writeResponse, new DeviceReadResp());
    }

    /// <summary>
    /// Send a message to and receives a replay from the VXI-11 server if sending a query message.
    /// </summary>
    /// <remarks>   2022-12-12. </remarks>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay. </param>
    /// <returns>   A Tuple. </returns>
    public (DeviceWriteResp writeResponse, DeviceReadResp readResponse) SendReceive( string message, int millisecondsReadDelay = 3 )
    {
        return this.SendReceive( Encoding.Default.GetBytes( message ), millisecondsReadDelay );
    }

    #endregion

    #region " Write "

    /// <summary>   Sends a message to the VXI-11 server. </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <exception cref="OncRpcException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="message">  The message. </param>
    /// <returns>   An int. </returns>
    public int Write( string message )
    {
        if ( string.IsNullOrEmpty( message ) ) return 0;
        (DeviceWriteResp writeResponse, _) = this.SendReceive( Encoding.Default.GetBytes( message ) );
        if ( writeResponse.Error.Value != OncRpcException.OncRpcSuccess )
            throw new OncRpcException( writeResponse.Error.Value );
        else
            return writeResponse.Size;
    }

    /// <summary>   Sends a message with termination to the VXI-11 server. </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <param name="message">  The message. </param>
    /// <returns>   An int. </returns>
    public int WriteLine( string message )
    {
        return this.Write( $"{message}{this.WriteTermination}" );
    }

    /// <summary>
    /// Sends a message to the VXI-11 server and returns an exception message or the message length.
    /// </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <param name="message">  The message. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) TryWrite( string message )
    {
        if ( string.IsNullOrEmpty( message ) ) return (false, $"{nameof( message )} is empty");
        (DeviceWriteResp writeResponse, _) = this.SendReceive( Encoding.Default.GetBytes( message ) );
        if ( writeResponse.Error.Value != OncRpcException.OncRpcSuccess )
        {
            var ex = new OncRpcException( writeResponse.Error.Value );
            return (false, $"RPC error #{ex.Reason}), {ex.Message}, sending {message} to {this._coreClient.Client.Host}:{this._coreClient.Client.Port}");
        }
        else
            return (true, $"{writeResponse.Size}");
    }

    /// <summary>
    /// Sends a message with termination to the VXI-11 server and returns an exception message or the
    /// message length.
    /// </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <param name="message">  The message. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) TryWriteLine( string message )
    {
        return this.TryWrite( $"{message}{this.WriteTermination}" );
    }

    #endregion

    #region " Read "

    /// <summary>   Receives a message from the VXI-11 server. </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <exception cref="OncRpcException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="trimEnd">  (Optional) True to trim end. </param>
    /// <returns>   A string. </returns>
    public string Read( bool trimEnd = false )
    {
        DeviceReadResp readResponse = this.Receive();
        if ( (readResponse.Error?.Value).GetValueOrDefault( OncRpcException.OncRpcSuccess ) != OncRpcException.OncRpcSuccess )
            throw new OncRpcException( readResponse.Error.Value );
        else
        {
            int length = (readResponse.Data?.Length).GetValueOrDefault( 0 ) - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
            return length > 0
                ? Encoding.Default.GetString( readResponse.Data, 0, length )
                : string.Empty;
        }
    }

    /// <summary>   Tries to receive a message from the VXI-11 server. </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <param name="trimEnd">  (Optional) True to trim end. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) TryRead( bool trimEnd = false )
    {
        DeviceReadResp readResponse = this.Receive();
        if ( (readResponse.Error?.Value).GetValueOrDefault( OncRpcException.OncRpcSuccess ) != OncRpcException.OncRpcSuccess )
        {
            var ex = new OncRpcException( readResponse.Error.Value );
            return (false, $"RPC error #{ex.Reason}), {ex.Message}, reading from {this._coreClient.Client.Host}:{this._coreClient.Client.Port}");
        }
        else
        {
            int length = (readResponse.Data?.Length).GetValueOrDefault( 0 ) - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
            return length > 0
                ? (true, Encoding.Default.GetString( readResponse.Data, 0, length ))
                : (true, string.Empty);
        }
    }

    /// <summary>   Receives single-precision values from the VXI-11 server. </summary>
    /// <remarks>   2022-11-14. </remarks>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>   The number of received bytes. </returns>
    public int Read( int offset, int count, ref float[] values )
    {
        DeviceReadResp readResponse = this.Receive( count * 4 + offset + 1 );
        // Need to convert to the byte array into single
        Buffer.BlockCopy( readResponse.Data, offset, values, 0, values.Length * 4 );
        return readResponse.Data.Length;
    }

    #endregion

    #region " Query "


    /// <summary>   Gets or sets the default encoding. </summary>
    /// <remarks>
    /// The default encoding for VXI-11 is <see cref="Encoding.ASCII"/>, which is a subset of <see cref="Encoding.UTF8"/>
    /// </remarks>
    /// <value> The default encoding. </value>
    public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

    /// <summary>   Sends a query message to and receives a message from the VXI-11 server. </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <exception cref="OncRpcException">  Thrown when an OncRpc error condition occurs. </exception>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) Query( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        if ( string.IsNullOrEmpty( message ) ) return (false, $"{nameof( message )} is empty");
        (DeviceWriteResp writeResponse, DeviceReadResp readResponse) = this.SendReceive( DefaultEncoding.GetBytes( message ), millisecondsReadDelay );
        if ( writeResponse.Error.Value != OncRpcException.OncRpcSuccess )
            throw new OncRpcException( writeResponse.Error.Value );
        else if ( (readResponse.Error?.Value).GetValueOrDefault( OncRpcException.OncRpcSuccess ) != OncRpcException.OncRpcSuccess )
            throw new OncRpcException( readResponse.Error.Value );
        else
        {
            int length = (readResponse.Data?.Length).GetValueOrDefault( 0 ) - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
            return length > 0
                ? (true, DefaultEncoding.GetString( readResponse.Data, 0, length ))
                : (true, string.Empty);
        }
    }

    /// <summary>   Sends a query message with termination to the VXI-11 server and returns the reply message. </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>   The line. </returns>
    public (bool success, string response) QueryLine( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        return this.Query( $"{message}{this.WriteTermination}", millisecondsReadDelay, trimEnd );
    }


    /// <summary>
    /// Sends a query message to the VXI-11 server and returns the replay message or an exception
    /// message.
    /// </summary>
    /// <remarks>   2022-12-12. </remarks>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) TryQuery( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        if ( string.IsNullOrEmpty( message ) ) return (false, $"{nameof( message )} is empty");

        (DeviceWriteResp writeResponse, DeviceReadResp readResponse) = this.SendReceive( Encoding.Default.GetBytes( message ), millisecondsReadDelay );
        if ( writeResponse.Error.Value != OncRpcException.OncRpcSuccess )
        {
            var ex = new OncRpcException( writeResponse.Error.Value );
            return (false, $"RPC error #{ex.Reason}), {ex.Message}, sending {message} to {this._coreClient.Client.Host} : {this._coreClient.Client.Port}");
        }
        else if ( (readResponse.Error?.Value).GetValueOrDefault( OncRpcException.OncRpcSuccess ) != OncRpcException.OncRpcSuccess )
        {
            var ex = new OncRpcException( readResponse.Error.Value );
            return (false, $"RPC error #{ex.Reason}), {ex.Message}, querying {message} from {this._coreClient.Client.Host} : {this._coreClient.Client.Port}");
        }
        else
        {
            int length = (readResponse.Data?.Length).GetValueOrDefault( 0 ) - (trimEnd && this.ReadTermination != 0 ? 1 : 0);
            return length > 0
                ? (true, Encoding.Default.GetString( readResponse.Data, 0, length ))
                : (true, string.Empty);
        }
    }

    /// <summary>
    /// Sends a query message with termination to the VXI-11 server and returns the replay message or
    /// an exception message.
    /// </summary>
    /// <remarks>   2022-12-13. </remarks>
    /// <param name="message">                  The message. </param>
    /// <param name="millisecondsReadDelay">    (Optional) The milliseconds read delay . </param>
    /// <param name="trimEnd">                  (Optional) True to trim end. </param>
    /// <returns>   A Tuple. </returns>
    public (bool success, string response) TryQueryLine( string message, int millisecondsReadDelay = 3, bool trimEnd = false )
    {
        return this.TryQuery( $"{message}{this.WriteTermination}", millisecondsReadDelay, trimEnd );
    }

    /// <summary>   Sends a query message and reads the reply as a single-precision values. </summary>
    /// <remarks>   2022-11-14. </remarks>
    /// <param name="message">  The message. </param>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>   The number of received bytes. </returns>
    public int Query( string message, int offset, int count, ref float[] values )
    {
        if ( string.IsNullOrEmpty( message ) ) return 0;
        _ = this.Write( message );
        return this.Read( offset, count, ref values );
    }

    /// <summary>   Sends a query message with termination and reads the reply as a single-precision values. </summary>
    /// <remarks>   2022-11-14. </remarks>
    /// <param name="message">  The message. </param>
    /// <param name="offset">   The offset into the received bytes. </param>
    /// <param name="count">    Number of single precision values. </param>
    /// <param name="values">   [in,out] the single precision values. </param>
    /// <returns>   The number of received bytes. </returns>
    public int QueryLine( string message, int offset, int count, ref float[] values )
    {
        if ( string.IsNullOrEmpty( message ) ) return 0;
        _ = this.WriteLine( message );
        return this.Read( offset, count, ref values );
    }

    #endregion
}
