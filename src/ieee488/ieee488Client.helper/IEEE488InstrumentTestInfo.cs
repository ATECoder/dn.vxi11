using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

using cc.isr.VXI11.Logging;
namespace cc.isr.VXI11.IEEE488Client.Helper;

/// <summary>   Values that represent instrument Identifiers. </summary>
public enum InstrumentId
{
    None, K2450 = 2450, K2600 = 2600, K6510 = 6510, K7510 = 7510
}

/// <summary>   Information about the IEEE488 instrument test. </summary>
public static class IEEE488InstrumentTestInfo
{

    private static readonly Dictionary<string, (int ReadAfterWriteDelay, int InterQueryDelay, string IPv4Address)> _instrumentInfo;

    /// <summary>   Gets the cancellation token source. </summary>
    /// <value> The cancellation token source. </value>
    public static CancellationTokenSource CancellationTokenSource { get; private set; }

    static IEEE488InstrumentTestInfo()
    {
        _instrumentInfo = new() {
        { "Echo", (0, 0, "127.0.0.1") },
        { InstrumentId.K2450.ToString() , (0, 0, "192.168.0.152") },
        { InstrumentId.K2600.ToString(), (0, 0, "192.168.0.50") },
        { InstrumentId.K6510.ToString(), (0, 0, "192.168.0.154") },
        { InstrumentId.K7510.ToString(), (0, 0, "192.168.0.144") }
        };
        CancellationTokenSource = new();
    }

    /// <summary>   Attempts to TCP connection a TimeSpan from the given InstrumentId. </summary>
    /// <param name="instrumentId">         Identifier for the instrument. </param>
    /// <param name="connectionTimeout">    The connection timeout. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool TryTcpConnection( InstrumentId instrumentId, TimeSpan connectionTimeout )
    {
        int portNumber = instrumentId == InstrumentId.None ? 13000 : 5025;

        string instrument = instrumentId == InstrumentId.None ? "Echo" : instrumentId.ToString();
        string ipv4Address = _instrumentInfo[instrument].IPv4Address;
        if ( !Paping( ipv4Address, portNumber, ( int ) connectionTimeout.TotalMilliseconds ) )
        {
            QueryInfo = $"Attempt to establish a TCP connection with the {instrument} at {ipv4Address}:{portNumber} aborted after {connectionTimeout.TotalMilliseconds:0}ms";
            return false;
        }
        return true;
    }


    /// <summary>   Resets the clear device. </summary>
    /// <param name="instrumentId">         Identifier for the instrument. </param>
    /// <param name="connectionTimeout">    The connection timeout. </param>
    /// <returns>   A string. </returns>
    public static string ResetClearDevice( InstrumentId instrumentId, TimeSpan connectionTimeout )
    {
        // TO_DO: Add DCL.

        if ( !TryTcpConnection( instrumentId, connectionTimeout ) )
            return string.Empty;

        string command = "*RST";

        string instrument = instrumentId == InstrumentId.None ? "Echo" : instrumentId.ToString();
        TimeSpan readAfterWriteDelay = TimeSpan.FromMilliseconds( _instrumentInfo[instrument].ReadAfterWriteDelay );
        int interQueryDelayMs = _instrumentInfo[instrument].InterQueryDelay;
        string ipv4Address = _instrumentInfo[instrument].IPv4Address;

        QueryInfo = $"{instrument} Delays: Read: {readAfterWriteDelay.TotalMilliseconds:0}ms; Write: {interQueryDelayMs}ms";

        System.Text.StringBuilder builder = new();
        using var vxi11Client = new isr.VXI11.IEEE488.Ieee488Client();

        vxi11Client.Connect( ipv4Address, "inst0" );

        string response = WriteDevice( vxi11Client, command );
        _ = builder.Append( $"{command}: {response}\n" );

        command = "*CLS";
        if ( interQueryDelayMs > 0 ) System.Threading.Thread.Sleep( interQueryDelayMs );
        response = WriteDevice( vxi11Client, command );
        _ = builder.Append( $"{command}: {response}\n" );

        command = "SYST:CLE";
        if ( interQueryDelayMs > 0 ) System.Threading.Thread.Sleep( interQueryDelayMs );
        response = WriteDevice( vxi11Client, command );
        _ = builder.Append( $"{command}: {response}\n" );

        return builder.ToString();
    }

    /// <summary>   Gets or sets information describing the query. </summary>
    /// <value> Information describing the query. </value>
    public static string QueryInfo { get; set; }

    /// <summary>   Queries the identity. </summary>
    /// <param name="instrumentId">         Identifier for the instrument. </param>
    /// <param name="connectionTimeout">    The connection timeout. </param>
    /// <returns>   The identity. </returns>
    public static string QueryIdentity( InstrumentId instrumentId, TimeSpan connectionTimeout )
    {

        if ( !TryTcpConnection( instrumentId, connectionTimeout ) )
            return string.Empty;

        string command = "*IDN?";
        string instrument = instrumentId == InstrumentId.None ? "Echo" : instrumentId.ToString();
        TimeSpan readAfterWriteDelay = TimeSpan.FromMilliseconds( _instrumentInfo[instrument].ReadAfterWriteDelay );
        int interQueryDelayMs = _instrumentInfo[instrument].InterQueryDelay;
        string ipv4Address = _instrumentInfo[instrument].IPv4Address;

        QueryInfo = $"{instrument} Delays: Read: {readAfterWriteDelay.TotalMilliseconds:0}ms; Write: {interQueryDelayMs}ms";

        System.Text.StringBuilder builder = new();
        using var vxi11Client = new isr.VXI11.IEEE488.Ieee488Client();

        vxi11Client.Connect( ipv4Address, "inst0" );

        string response = QueryDevice( vxi11Client, command, true );
        _ = builder.Append( $"a: {response}\n" );

        if ( interQueryDelayMs > 0 ) System.Threading.Thread.Sleep( interQueryDelayMs );
        response = QueryDevice( vxi11Client, command, true );
        _ = builder.Append( $"b: {response}\n" );

        if ( interQueryDelayMs > 0 ) System.Threading.Thread.Sleep( interQueryDelayMs );
        response = QueryDevice( vxi11Client, command, true );
        _ = builder.Append( $"c: {response}\n" );

        return builder.ToString();
    }

    /// <summary>   Queries a device. </summary>
    /// <param name="vxi11Client">  The VXI-11 client. </param>
    /// <param name="command">      The command. </param>
    /// <param name="trimEnd">      True to trim end. </param>
    /// <returns>   The device. </returns>
    private static string QueryDevice( isr.VXI11.IEEE488.Ieee488Client vxi11Client, string command, bool trimEnd )
    {
        try
        {
            (bool success, string reply) = vxi11Client.Query( $"{command}\n", 0, trimEnd );
            return reply;
        }
        catch ( ApplicationException ex )
        {
            Logger.Writer.LogMemberError( "failed querying the device",  ex );
        }
        return "Exception occurred";
    }

    /// <summary>   Writes a messages to the device. </summary>
    /// <param name="vxi11Client">  The VXI-11 client. </param>
    /// <param name="command">      The command. </param>
    /// <returns>   A string. </returns>
    private static string WriteDevice( isr.VXI11.IEEE488.Ieee488Client vxi11Client, string command )
    {
        try
        {
            int count = vxi11Client.Write( $"{command}\n" );
            return count.ToString();
        }
        catch ( ApplicationException ex )
        {
            Logger.Writer.LogMemberError( "Exception writing to the device.", ex );
        }
        return "Exception occurred";
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
    /// <param name="nameOrAddress">    The name or address. </param>
    /// <returns>   True if it succeeds, false if it fails. </returns>
    public static bool PingHost( string nameOrAddress )
    {
        bool pingable = false;
        try
        {
            using Ping pinger = new();
            PingReply reply = pinger.Send( nameOrAddress );
            pingable = reply.Status == IPStatus.Success;
        }
        catch ( PingException )
        {
            // Discard PingExceptions and return false;
        }
        return pingable;
    }

}
