namespace cc.isr.VXI11.Visa
{
    /// <summary>   A TCP/IP VISA INSTR address also termed resource name. </summary>
    public class TcpipInstrAddress : AddressBase
    {

        /// <summary>   (Immutable) the default protocol. </summary>
        public const string ProtocolDefault = "TCPIP";

        /// <summary>   (Immutable) the default suffix. </summary>
        public const string SuffixDefault = "INSTR";

        /// <summary>   Constructor. </summary>
        /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
        ///                                                 invalid. </exception>
        /// <param name="fullResourceName"> The full resource name. </param>
        public TcpipInstrAddress( string fullResourceName ) : base()
        {
            base.Address = this.ParseAddress( fullResourceName )
                ? fullResourceName
                : throw new InvalidOperationException( $"Failed parsing resource name {fullResourceName}" );

        }

        /// <summary>   Parses the VISA address (resource name). </summary>
        /// <param name="address"> The full resource name. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public bool ParseAddress( string address )
        {
            AddressParser parser = new (ProtocolDefault, SuffixDefault);
            bool result = parser.ParseAddress( address );
            if ( result )
            {
                this.Clone( parser );
                this.InterfaceDeviceAddress = new DeviceAddress( this.Device );
            }
            return result;
        }

        /// <summary>   Gets or sets the interface device address. </summary>
        /// <value> The device address. </value>
        public DeviceAddress InterfaceDeviceAddress { get; private set; }

    }
}
