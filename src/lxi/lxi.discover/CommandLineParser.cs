
namespace cc.isr.VXI11.LXI.Discover;

/// <summary>   Command line arguments parser. </summary>
internal class CommandLineParser
{
    private Dictionary<string, string> _argumentDix = new();

    public static CommandLineParser I { get; } = new();

    public static string TimeoutKey { get; } = "--timeout";

    public static string IPKey { get; } = "--ip";

    public static string HelpKey { get; } = "--help";

    public static char Delimiter { get; set; } = ';';

    public static char EqualsSign { get; set; } = '=';

    public static int TimeoutDefault { get; set; } = 100;

    public static string BroadcastAddressDefault { get; set; } = "192.168.0.255";

    public static string DefaultArgs = $"{CommandLineParser.TimeoutKey}{EqualsSign}{TimeoutDefault};{CommandLineParser.IPKey}{EqualsSign}{BroadcastAddressDefault}";
    public static string Usage => $"Usage: ./{nameof( LxiDiscover )} {CommandLineParser.TimeoutKey}{EqualsSign}<timeout_milliseconds>{Delimiter}{CommandLineParser.IPKey}{EqualsSign}<address>{Delimiter}{CommandLineParser.HelpKey}";

    public static string HelpUsage => $"./{nameof( LxiDiscover )} {CommandLineParser.HelpKey}";

    /// <summary>   Query if this object contains the given key. </summary>
    /// <remarks>   2023-02-01. </remarks>
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
        return this._argumentDix.ContainsKey( key ) ? this._argumentDix[key] : string.Empty;
    }

    /// <summary>   Gets a long. </summary>
    /// <param name="key">  The argument key or name. </param>
    /// <returns>   The long. </returns>
    public long GetLong( string key )
    {
        return this._argumentDix.ContainsKey( key ) ? Convert.ToInt64( this._argumentDix[key] ) : 0;
    }

    /// <summary>   Gets a double. </summary>
    /// <param name="key">  The argument key or name. </param>
    /// <returns>   The double. </returns>
    public double GetDouble( string key )
    {
        return this._argumentDix.ContainsKey( key ) ? Convert.ToDouble( this._argumentDix[key] ) : 0;
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
