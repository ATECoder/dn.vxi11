using System.ComponentModel;
using System.Net.Sockets;
using System.Net;
using System.Reflection;

namespace cc.isr.VXI11;

public static class Support
{

    /// <summary>   Gets a description from an Enum. </summary>
    /// <remarks>   2023-01-07. </remarks>
    /// <param name="value">    An enum constant representing the value option. </param>
    /// <returns>   The description. </returns>
    public static string GetDescription( this Enum value )
    {
        return
            value
                .GetType()
                .GetMember( value.ToString() )
                .FirstOrDefault()
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description
            ?? value.ToString();
    }

    /// <summary>   Converts a value to an int 32. </summary>
    /// <param name="value">    The value. </param>
    /// <returns>   The given data converted to an int 32. </returns>
    public static int ConvertToInt32( this string value )
    {
        return string.IsNullOrEmpty( value )
            ? 0
            : value.StartsWith( "0x" )
                ? Convert.ToInt32( value[2..], 16 )
                : Convert.ToInt32( value );
    }

    /// <summary>   Gets the local host <see cref="IPAddress"/>. </summary>
    /// <remarks>   2023-01-16. </remarks>
    /// <returns>   The host. </returns>
    internal static IPAddress? GetHost()
    {
        IPHostEntry host = Dns.GetHostEntry( Dns.GetHostName() );
        var ipAddress = host
            .AddressList
            .FirstOrDefault( ip => ip.AddressFamily == AddressFamily.InterNetwork );
        return ipAddress;
    }

    #region " client identifiers "

    /// <summary>   Gets or sets the client identifier exclusive upper bound. </summary>
    /// <value> The client identifier exclusive upper bound. </value>
    public static int ClientIdentifierUpperBound { get; set; } = short.MaxValue;

    /// <summary>   Holds the unique client identities for this solution. </summary>
    private static readonly HashSet<int> _clientIdentifiers = new ();

    /// <summary>   Generates a unique random client identifier. </summary>
    /// <remarks>   2023-01-17. </remarks>
    /// <returns>   The unique random client identifier. </returns>
    public static int GenerateUniqueRandomClientIdentifier()
    {
        Random rng = new();
        int clientId;
        do
        {
            clientId = rng.Next( 0, ClientIdentifierUpperBound );
        }
        while ( _clientIdentifiers.Contains( clientId ) );
        _ = _clientIdentifiers.Add( clientId );
        return clientId;
    }

    #endregion

}
