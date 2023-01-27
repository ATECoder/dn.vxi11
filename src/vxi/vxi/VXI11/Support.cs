using System.ComponentModel;
using System.Net.Sockets;
using System.Net;
using System.Reflection;

namespace cc.isr.VXI11;

public static class Support
{

    /// <summary>   Gets a description from an Enum. </summary>
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

    /// <summary>   A <see cref="string"/> extension method that converts a value to an <see cref="int"/>. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="value">    The value. </param>
    /// <returns>   The given data converted to an int 32. </returns>
    public static int ToInt( this string value )
    {
        return string.IsNullOrEmpty( value )
            ? 0
            : value.StartsWith( "0x" )
                ? Convert.ToInt32( value[2..], 16 )
                : Convert.ToInt32( value );
    }

    /// <summary>   Gets the local host <see cref="IPAddress"/>. </summary>
    /// <returns>   The host. </returns>
    internal static IPAddress? GetHost()
    {
        IPHostEntry host = Dns.GetHostEntry( Dns.GetHostName() );
        var ipAddress = host
            .AddressList
            .FirstOrDefault( ip => ip.AddressFamily == AddressFamily.InterNetwork );
        return ipAddress;
    }

    #region " IP Address converters "

    /// <summary>   An <see cref="IPAddress"/> extension method that converts the address to an <see cref="uint"/>. </summary>
    /// <remarks>
    /// IP addresses are in network order (big-endian), while <see cref="int"/>s are little-endian on
    /// Windows, so to get a correct value, you must reverse the bytes before converting on a little-
    /// endian system. <para>
    /// 
    /// Also, even for IPv4, an int can't hold addresses bigger than 127.255.255.255, e.g. the
    /// broadcast address (255.255.255.255), so use a uint. </para><para>
    /// 
    /// <see href="https://stackoverflow.com/questions/461742/how-to-convert-an-ipv4-address-into-a-integer-in-c">
    /// stack overflow</see> </para>
    /// </remarks>
    /// <param name="address">  The address to act on. </param>
    /// <returns>   Address as an unsigned integer. </returns>
    public static uint ToUInt( this IPAddress address )
    {
        byte[] bytes = address.GetAddressBytes();

        // flip big-endian(network order) to little-endian
        if ( BitConverter.IsLittleEndian )
        {
            Array.Reverse( bytes );
        }
        return BitConverter.ToUInt32( bytes, 0 );
    }

    /// <summary>   An <see cref="uint"/> extension method that converts the address to an IP address. </summary>
    /// <remarks>   2023-01-26. </remarks>
    /// <param name="address">  The address to act on. </param>
    /// <returns>   Address as a string. </returns>
    public static string ToIPAddress( this uint address )
    {
        byte[] bytes = BitConverter.GetBytes( address );

        // flip little-endian to big-endian(network order)
        if ( BitConverter.IsLittleEndian )
        {
            Array.Reverse( bytes );
        }

        return new IPAddress( bytes ).ToString();
    }

    #endregion

    #region " client identifiers "

    /// <summary>   Gets or sets the client identifier exclusive upper bound. </summary>
    /// <value> The client identifier exclusive upper bound. </value>
    public static int ClientIdentifierUpperBound { get; set; } = short.MaxValue;

    /// <summary>   Holds the unique client identities for this solution. </summary>
    private static readonly HashSet<int> _clientIdentifiers = new();

    /// <summary>   Generates a unique random client identifier. </summary>
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
