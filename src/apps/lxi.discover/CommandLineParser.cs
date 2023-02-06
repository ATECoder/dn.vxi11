
using System.Net;
using System.Text;

namespace cc.isr.VXI11.Discover;

/// <summary>   Command line arguments parser. </summary>
internal class CommandLineParser
{
    private Dictionary<string, string> _argumentDix = new();

    /// <summary>
    /// Constructor that prevents a default instance of this class from being created.
    /// </summary>
    private CommandLineParser()  {}

    static CommandLineParser()
    {
        StringBuilder sb = new ();
        _ = sb.Append( $"{CommandLineParser.TimeoutKey}{CommandLineParser.EqualsSign}{CommandLineParser.TimeoutDefault}");
        _ = sb.Append( $"{CommandLineParser.Delimiter}{CommandLineParser.IPKey}{CommandLineParser.EqualsSign}{CommandLineParser.BroadcastAddressDefault}" );
        _ = sb.Append( $"{CommandLineParser.Delimiter}{CommandLineParser.DeviceInterfaceStringKey}{CommandLineParser.EqualsSign}{CommandLineParser.DeviceInstrumentStringDefault}" );
        CommandLineParser.DefaultArgs = sb.ToString();
        _ = sb.Clear();

        _ = sb.Append( $"Usage: ./{nameof( LxiDiscover )} " );
        _ = sb.Append( $"{CommandLineParser.TimeoutKey}{CommandLineParser.EqualsSign}<timeout_milliseconds>" );
        _ = sb.Append( $"{CommandLineParser.Delimiter}{CommandLineParser.IPKey}{CommandLineParser.EqualsSign}<address>" );
        _ = sb.Append( $"{CommandLineParser.Delimiter}{CommandLineParser.DeviceInterfaceStringKey}{CommandLineParser.EqualsSign}inst0" );
        _ = sb.Append( $"{CommandLineParser.Delimiter}{CommandLineParser.HelpKey}" );
        CommandLineParser.Usage = sb.ToString();
        _ = sb.Clear();

        _ = sb.Append( $"./{nameof( LxiDiscover )} {CommandLineParser.HelpKey}" );
        CommandLineParser.HelpUsage = sb.ToString();
        _ = sb.Clear();
    }

    /// <summary>   Gets the singleton instance of the parser. </summary>
    /// <value> The singleton instance of the parse. </value>
    public static CommandLineParser I { get; } = new();

    /// <summary>   Gets or sets the delimiter. </summary>
    /// <value> The delimiter. </value>
    public static char Delimiter { get; set; } = ';';

    /// <summary>   Gets the help key. </summary>
    /// <value> The help key. </value>
    public static string HelpKey { get; } = "--help";

    /// <summary>   Gets or sets the equals sign. </summary>
    /// <value> The equals sign. </value>
    public static char EqualsSign { get; set; } = '=';

    /// <summary>   Gets the timeout key. </summary>
    /// <value> The timeout key. </value>
    public static string TimeoutKey { get; } = "--timeout";

    /// <summary>   Gets or sets the timeout default. </summary>
    /// <value> The timeout default. </value>
    public static int TimeoutDefault { get; set; } = 10;

    /// <summary>   Gets the IP key. </summary>
    /// <value> The IP key. </value>
    public static string IPKey { get; } = "--ip";

    /// <summary>   Gets or sets the broadcast address default. </summary>
    /// <value> The broadcast address default. </value>
    public static string BroadcastAddressDefault { get; set; } = IPAddress.Any.ToString();

    /// <summary>   Gets the device interface string key. </summary>
    /// <value> The device interface string key. </value>
    public static string DeviceInterfaceStringKey { get; } = "--dev";

    /// <summary>   Gets or sets the device instrument string default. </summary>
    /// <value> The device instrument string default. </value>
    public static string DeviceInstrumentStringDefault { get; set; } = "inst0";

    /// <summary>   Gets or sets the default arguments. </summary>
    /// <value> The default arguments. </value>
    public static string DefaultArgs { get; set; }

    /// <summary>   Gets or sets the usage. </summary>
    /// <value> The usage. </value>
    public static string Usage { get; set; }

    public static string HelpUsage { get; set; }

    /// <summary>   Query if this object contains the given key. </summary>
    /// <param name="key">  The string to test for containment. </param>
    /// <returns>   True if the object is in this collection, false if not. </returns>
    public bool Contains( string key )
    {
        return this._argumentDix.ContainsKey( key );
    }

    /// <summary>   Gets a string. </summary>
    /// <param name="key">  The argument key or name. </param>
    /// <returns>   The string. </returns>
    public string GetString( string key )
    {
        return this._argumentDix.TryGetValue( key, out string? value ) ? value : string.Empty;
    }

    /// <summary>   Gets an int. </summary>
    /// <param name="key">  The argument key or name. </param>
    /// <returns>   The int. </returns>
    public int GetInt( string key )
    {
        return ( int ) this.GetLong( key );
    }

    /// <summary>   Gets a long. </summary>
    /// <param name="key">  The argument key or name. </param>
    /// <returns>   The long. </returns>
    public long GetLong( string key )
    {
        return this._argumentDix.TryGetValue( key, out string? value ) ? Convert.ToInt64( value ) : 0;
    }

    /// <summary>   Gets a double. </summary>
    /// <param name="key">  The argument key or name. </param>
    /// <returns>   The double. </returns>
    public double GetDouble( string key )
    {
        return this._argumentDix.TryGetValue( key, out string? value ) ? Convert.ToDouble( value ) : 0;
    }

    /// <summary>   Parse arguments. </summary>
    /// <param name="args">         The arguments. </param>
    /// <param name="defaultArgs">  The default arguments. </param>
    public void ParseArgs( string[] args, string defaultArgs )
    {
        this._argumentDix = new Dictionary<string, string>();
        this.ParseDefaults( defaultArgs );

        foreach ( string arg in args )
        {
            string[] words = arg.Split( EqualsSign );
            this._argumentDix[words[0]] = words.Length == 1 ? string.Empty : words[1];
        }
    }

    /// <summary>   Parse defaults. </summary>
    /// <param name="defaultArgs">  The default arguments. </param>
    private void ParseDefaults( string defaultArgs )
    {
        if ( string.IsNullOrWhiteSpace( defaultArgs ) ) return;
        string[] args = defaultArgs.Split( Delimiter );

        foreach ( string arg in args )
        {
            string[] words = arg.Split( EqualsSign );
            this._argumentDix[words[0]] = words.Length == 1 ? string.Empty : words[1];
        }
    }

}
