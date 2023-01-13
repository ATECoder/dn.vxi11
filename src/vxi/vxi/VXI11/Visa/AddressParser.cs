using System.Text.RegularExpressions;

namespace cc.isr.VXI11.Visa
{
    /// <summary>   A parser for the address of a VISA resource. </summary>
    public class AddressParser : AddressBase
    {

        /// <summary>   Constructor. </summary>
        /// <param name="defaultProtocol">  The default protocol. </param>
        /// <param name="defaultSuffix">    The default suffix. </param>
        public AddressParser(string defaultProtocol, string defaultSuffix )  : base()
        {
            this.DefaultProtocol = defaultProtocol;
            this.DefaultSuffix = defaultSuffix;
            this.Suffix = this.DefaultSuffix;
            this.Protocol = this.DefaultProtocol;
            this.RegexPattern = string.Empty;
            this.BuildRegexPattern();
        }

        /// <summary>   Constructor. </summary>
        /// <param name="defaultProtocol">  The default protocol. </param>
        /// <param name="defaultSuffix">    The default suffix. </param>
        /// <param name="board">            The board. </param>
        /// <param name="host">             The host. </param>
        /// <param name="device">           The device. </param>
        public AddressParser( string defaultProtocol, string defaultSuffix, string board, string host, string device ) : this( defaultProtocol, defaultSuffix )
        {
            this.Board = board;
            this.Host = host;
            this.Device = device;
        }

        /// <summary>   Gets or sets the RegEx pattern. </summary>
        /// <value> The RegEx pattern. </value>
        public string RegexPattern { get; set; }

        /// <summary>   Gets or sets the default protocol. </summary>
        /// <value> The default protocol. </value>
        public string DefaultProtocol { get; set; }

        /// <summary>   Gets or sets the default suffix. </summary>
        /// <value> The default suffix. </value>
        public string DefaultSuffix { get; set; }

        /// <summary>   Builds the RegEx pattern for parsing the VISA address. </summary>
        private void BuildRegexPattern()
        {
            StringBuilder builder = new ();
            _ = builder.Append( @$"^(?<{nameof( Board )}>(?<{nameof( AddressBase.Protocol )}>{this.DefaultProtocol})\d*)" );
            _ = builder.Append( @$"(::(?<{nameof( AddressBase.Host )}>[^\s:]+))" );
            _ = builder.Append( @$"(::(?<{nameof( AddressBase.Device )}>[^\s:]+(\[.+\])?))" );
            _ = builder.Append( @$"?(::(?<{nameof( AddressBase.Suffix )}>{this.DefaultSuffix}))$" );
            this.RegexPattern = builder.ToString();
            // this.RegexPattern = @$"^(?<Board>(?<Protocol>TCPIP)\d*)(::(?<Host>[^\s:]+))(::(?<Device>[^\s:]+(\[.+\])?))?(::(?<Suffix>INSTR))$";
            // this.RegexPattern = @$"^(?<{nameof( Board )}>(?<{nameof( AddressBase.Protocol )}>{DefaultProtocol})\d*)(::(?<{nameof( AddressBase.Host )}>)>[^\s:]+))(::(?<{nameof( AddressBase.Device )}>[^\s:]+(\[.+\])?))?(::(?<{nameof( AddressBase.Suffix )}>{DefaultSuffix}))$";
        }

        /// <summary>   Parse the address of the VISA resource. </summary>
        /// <param name="address"> Address of the VISA resource. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public bool ParseAddress( string address )
        {
            if ( address == null ) { return false; }
            var m = Regex.Match( address, this.RegexPattern, RegexOptions.IgnoreCase );
            if ( m == null ) { return false; }
            this.Address = address;
            this.Board = m.Groups[ nameof( AddressBase.Board ) ].Value;
            this.Protocol = m.Groups[nameof( AddressBase.Protocol )].Value;
            this.Host = m.Groups[nameof( AddressBase.Host )].Value;
            this.Device = m.Groups[nameof( AddressBase.Device )].Value;
            this.Suffix = m.Groups[nameof( AddressBase.Suffix )].Value;
            return true;
        }
    }
}
