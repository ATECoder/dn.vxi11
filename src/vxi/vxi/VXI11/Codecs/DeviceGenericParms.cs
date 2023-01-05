namespace cc.isr.VXI11.Codecs
{

    /// <summary>
    /// The <see cref="DeviceGenericParms"/> class defines the request XDR
    /// codec for the <see cref="Vxi11MessageConstants.DeviceReadStbProcedure"/>,
    /// <see cref="Vxi11MessageConstants.DeviceReadStbProcedure"/>,
    /// <see cref="Vxi11MessageConstants.DeviceTriggerProcedure"/>,
    /// <see cref="Vxi11MessageConstants.DeviceClearProcedure"/>,
    /// <see cref="Vxi11MessageConstants.DeviceRemoteProcedure"/>, and
    /// <see cref="Vxi11MessageConstants.DeviceLocalProcedure"/>
    /// RPC messages.</summary>
    /// <remarks>   Renamed from <c>Device_GenericParms</c>. <para>
    /// VXI-11 Specifications: </para>
    /// <code>
    /// struct Device_GenericParms {
    ///    Device_Link lid;             /* Device_Link id from connect call */
    ///    Device_Flags flags;          /* flags with options */
    ///    unsigned long lock_timeout;  /* time to wait for lock */
    ///    unsigned long io_timeout;    /* time to wait for I/O */
    /// };
    /// </code>
    /// </remarks>
    public class DeviceGenericParms : IXdrCodec
    {
        /// <summary>   Gets or sets the identifier of the device link from the connect call. </summary>
        /// <value> The identifier of the device link. </value>
        public DeviceLink DeviceLinkId {  get; set; }

        /// <summary>   Gets or sets the flags with options. </summary>
        /// <value> The flags. </value>
        public DeviceFlags Flags { get; set; }

        /// <summary>   Gets or sets the lock timeout. </summary>
        /// <remarks>
        /// The lock timeout determines how long a network instrument server will wait for a lock to be released.
        /// Units for both are in milliseconds.
        /// </remarks>
        /// <value> The lock timeout. </value>
        public int LockTimeout { get; set; }

        /// <summary>   Gets or sets the i/o timeout. </summary>
        /// <remarks>
        /// The i/o timeout value determines how long a network instrument server allows an I/O operation to take.
        /// </remarks>
        /// <value> The i/o timeout. </value>
        public int IOTimeout { get; set; }

        /// <summary>   Default constructor. </summary>
        public DeviceGenericParms()
        {
        }

        /// <summary>   Constructor. </summary>
        /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
        public DeviceGenericParms( XdrDecodingStreamBase decoder )
        {
            this.Decode( decoder );
        }

        /// <summary>
        /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
        /// </summary>
        /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
        public void Encode( XdrEncodingStreamBase encoder )
        {
            this.DeviceLinkId.Encode( encoder );
            this.Flags.Encode( encoder );
            encoder.EncodeInt( this.LockTimeout );
            encoder.EncodeInt( this.IOTimeout );
        }

        /// <summary>
        /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
        /// </summary>
        /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
        public void Decode( XdrDecodingStreamBase decoder )
        {
            this.DeviceLinkId = new DeviceLink( decoder );
            this.Flags = new DeviceFlags( decoder );
            this.LockTimeout = decoder.DecodeInt();
            this.IOTimeout = decoder.DecodeInt();
        }

    }
}
