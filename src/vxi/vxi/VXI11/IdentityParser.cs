namespace cc.isr.VXI11
{
    /// <summary>   An identity parser. </summary>
    /// <remarks>   2023-02-11. </remarks>
    public class IdentityParser : IEquatable<IdentityParser>
    {

        /// <summary>   Constructor. </summary>
        /// <remarks>   2023-02-11. </remarks>
        /// <param name="identity"> The identity. </param>
        public IdentityParser( string identity )
        {
            this.Identity = identity;
            this.Manufacturer = string.Empty;
            this.Model = string.Empty;
            this.Serial = string.Empty;
            this.Firmware = string.Empty;
            this.FirmwareDetails = new();
            this.Parse( identity );
        }

        /// <summary>   Gets or sets the identity. </summary>
        /// <value> The identity. </value>
        public string Identity { get; set; }

        /// <summary>   Gets or sets the manufacturer. </summary>
        /// <value> The manufacturer. </value>
        public string Manufacturer { get; set; }

        /// <summary>   Gets or sets the model. </summary>
        /// <value> The model. </value>
        public string Model { get; set; }

        /// <summary>   Gets or sets the serial. </summary>
        /// <value> The serial. </value>
        public string Serial { get; set; }

        /// <summary>   Gets or sets the firmware. </summary>
        /// <value> The firmware. </value>
        public string Firmware { get; set; }

        /// <summary>   Gets or sets the firmware details. </summary>
        /// <value> The firmware details. </value>
        public Dictionary<string, string> FirmwareDetails { get; set; }

        private void Clear()
        {
            this.Identity = nameof( IdentityParser.Identity );
            this.Manufacturer = nameof( IdentityParser.Manufacturer );
            this.Model = nameof( IdentityParser.Model );
            this.Serial = nameof( IdentityParser.Serial );
            this.Firmware = nameof( IdentityParser.Firmware );
            this.FirmwareDetails.Clear();
        }

        /// <summary>   Parses. </summary>
        /// <remarks>   2023-02-11. </remarks>
        /// <param name="identity"> The identity. </param>
        public virtual void Parse( string identity )
        {
            this.Clear();

            this.Identity = identity;
            string[] values = string.IsNullOrWhiteSpace( identity )
                ? Array.Empty<string>()
                : identity.Split( ',' );

            if ( values.Length > 0 ) this.Manufacturer = values[0];
            if ( values.Length > 1 ) this.Model = values[1];
            if ( values.Length > 2 ) this.Serial = values[2];
            if ( values.Length > 3 ) this.Firmware = values[3];

            this.ParseFirmware();

        }

        /// <summary>   Parse firmware. </summary>
        /// <remarks>   2023-02-11. <para>
        /// 
        /// The firmware element often has additional manufacturer specific information that
        /// is capture in the <see cref="FirmwareDetails"/> dictionary. Classes overriding this
        /// basic class are expected to had key value pairs to this dictionary with keys 
        /// that are exposed using constant members of the custom identity subclass.  
        /// </para></remarks>
        public virtual void ParseFirmware()
        {
            this.FirmwareDetails.Clear();
        }

        /// <summary>   Builds the identity. </summary>
        /// <remarks>   2023-02-11. </remarks>
        /// <returns>   A string. </returns>
        public string BuildIdentity()
        {
            return $"{this.Manufacturer},{this.Model},{this.Serial},{this.Firmware}";
        }

        /// <summary>   Builds the firmware. </summary>
        /// <remarks>   2023-02-11. </remarks>
        /// <returns>   A string. </returns>
        public string BuildFirmware()
        {
            StringBuilder builder = new();
            foreach ( string value in this.FirmwareDetails.Values )
            {
                if ( builder.Length > 0 ) builder.Append( "," );
                builder.Append( value );
            }
            return builder.ToString();

        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <remarks>   2023-02-20. </remarks>
        /// <param name="other">    An object to compare with this object. </param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter;
        /// otherwise, false.
        /// </returns>
        public bool Equals( IdentityParser other )
        {
            return (other is not null)
                && String.Equals( this.Manufacturer, other.Manufacturer )
                && String.Equals( this.Model, other.Model )
                && String.Equals( this.Serial, other.Model )
                && String.Equals( this.Firmware, other.Firmware )
                && String.Equals( this.Model, other.Model );
        }

    }
}
