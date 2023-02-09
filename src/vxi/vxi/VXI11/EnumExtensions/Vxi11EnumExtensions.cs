using System.ComponentModel;
using System.Reflection;

using cc.isr.VXI11.Client;
using cc.isr.VXI11.Codecs;
using cc.isr.VXI11.Server;

namespace cc.isr.VXI11.EnumExtensions;

/// <summary>   A support class for VXI-11 enum extensions. </summary>
public static partial class Vxi11EnumExtensions
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

}
