using System.ComponentModel;
using System.Net.Sockets;
using System.Net;
using System.Reflection;

namespace cc.isr.VXI11
{
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

        /// <summary>   Generates a client identifier with a more-or-less random value. </summary>
        /// <remarks>   Presently, the 'random' value is based on a seed of <see cref="DateTime.Now"/>.Ticks
        /// that is XOR'ed with its 31 right shifted value
        /// </remarks>
        /// <returns>   The client identifier. </returns>
        public static int GenerateClientIdentifier()
        {
            // Initialize the client identifier with some more-or-less random value.
            long seed = DateTime.Now.Ticks;
            return ( int ) seed ^ ( int ) (seed >> (32 & 0x1f));
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


    }
}
