namespace cc.isr.VXI11.Visa
{
    /// <summary>   A VISA resource address base class. </summary>
    public abstract class AddressBase
    {

        /// <summary>   Specialized default constructor for use only by derived class. </summary>
        protected AddressBase()
        {
            this.Board = string.Empty;
            this.Protocol = string.Empty;
            this.Device = string.Empty;
            this.Host= string.Empty;
            this.Suffix = string.Empty;
            this.Address = string.Empty;
       }

        /// <summary>   Specialized constructor for use only by derived class. </summary>
        /// <param name="board">    The board. </param>
        /// <param name="host">     The host. </param>
        /// <param name="device">   The device. </param>
        protected AddressBase( string board, string host, string device) : this()
        {
            this.Board = board;
            this.Host = host;
            this.Device = device;
        }

        /// <summary>   Makes a deep copy of this object. </summary>
        /// <remarks>   2023-01-12. </remarks>
        /// <param name="address"> The address of the VISA resource. </param>
        public void Clone( AddressBase address )
        {
            this.Address = address.Address;
            this.Board = address.Board;
            this.Device = address.Device;
            this.Host = address.Host;
            this.Protocol = address.Protocol;
            this.Suffix = address.Suffix;
        }

        /// <summary>   Builds the VISA address of the instrument. </summary>
        /// <remarks>   2023-01-12. </remarks>
        /// <returns>   A string. </returns>
        public virtual string BuildAddress()
        {
            StringBuilder builder = new StringBuilder();
            if ( !string.IsNullOrEmpty( this.Board ) )
                _ = builder.Append( this.Board );

            if ( !string.IsNullOrEmpty( this.Host ) )
            {
                if ( builder.Length > 0 )
                    _ = builder.Append( "::" );

                _ = builder.Append( this.Host );
            }

            if ( !string.IsNullOrEmpty( this.Device ) )
            {
                if ( builder.Length > 0 )
                    _ = builder.Append( "::" );

                _ = builder.Append( this.Device );
            }

            if ( !string.IsNullOrEmpty( this.Suffix ) )
            {
                if ( builder.Length > 0 )
                    _ = builder.Append( "::" );

                _ = builder.Append( this.Suffix );
            }

            return builder.ToString();
        }

        /// <summary>   Gets or sets the address of the VISA resource, which is also called resource name. </summary>
        /// <remarks> The VISA address format is as follows: <para>
        /// ‘Communication/Board Type( USB, GPIB, etc.)::Resource Information( Vendor ID, Product ID, Serial Number, IP address, etc..)::Resource Type’ </para>
        /// </remarks>
        /// <value> The address. </value>
        public string Address { get; protected set; }

        /// <summary>   Gets or sets the board, e.g., TCPIP0. </summary>
        /// <value> The board. </value>
        public string Board { get; set; }

        /// <summary>   Gets or sets the protocol, e.g., TCPIP </summary>
        /// <value> The protocol. </value>
        public string Protocol { get; set; }

        /// <summary>   Gets or sets the host, e.g., 192.168.1.123. </summary>
        /// <value> The host. </value>
        public string Host { get; set; }

        /// <summary>   Gets or sets the device address also termed interface device string, e.g., inst0 or gpib0,5 </summary>
        /// <value> The interface device string. </value>
        public string Device { get; set; }

        /// <summary>   Gets or sets the suffix, e.g., INSTR. </summary>
        /// <value> The suffix. </value>
        public string Suffix { get; set; }

    }

}
