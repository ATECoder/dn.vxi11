using System.ComponentModel;
using System.Reflection;

namespace cc.isr.VXI11
{
    internal static class Support
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

    }
}
