namespace cc.isr.VXI11.Codecs
{

    /// <summary>
    /// The <see cref="DeviceGenericParms"/> class defines the request XDR
    /// codec for the <see cref="Vxi11Message.DeviceReadStbProcedure"/>,
    /// <see cref="Vxi11Message.DeviceReadStbProcedure"/>,
    /// <see cref="Vxi11Message.DeviceTriggerProcedure"/>,
    /// <see cref="Vxi11Message.DeviceClearProcedure"/>,
    /// <see cref="Vxi11Message.DeviceRemoteProcedure"/>, and
    /// <see cref="Vxi11Message.DeviceLocalProcedure"/>
    /// RPC messages.</summary>
    /// <remarks> 
    /// The XDR encoding and decoding allows for integers to be passed between hosts, even when those hosts
    /// have different integer representations. <para>
    /// 
    /// All integers defined by the VXI-11 specification are passed over the
    /// network as 32-bit integers, either signed or unsigned as defined. </para><para>
    /// 
    /// Renamed from <c>Device_GenericParms</c>. </para><para>
    /// 
    /// VXI-11 Specifications: </para>
    /// <code>
    /// struct Device_GenericParms {
    ///    Device_Link lid;             /* Device_Link id from connect call */
    ///    Device_Flags flags;          /* flags with options */
    ///    unsigned long lock_timeout;  /* time to wait for lock */
    ///    unsigned long io_timeout;    /* time to wait for I/O */
    /// };
    /// </code>
    /// The generic parameter is used by several of the RPCs to pass the link ID, the operation flags, and the
    /// timeout value to the device.
    /// </remarks>
    public class DeviceGenericParms : IXdrCodec
    {

        /// <summary>   Default constructor. </summary>
        public DeviceGenericParms()
        {
            this._link = new();
            this._flags = new();
        }

        /// <summary>   Constructor. </summary>
        /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
        public DeviceGenericParms( XdrDecodingStreamBase decoder ) : this()
        {
            this.Decode( decoder );
        }

        /// <summary>   Decodes an instance of a <see cref="DeviceGenericParms"/>. </summary>
        /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
        /// <returns>   The <see cref="DeviceGenericParms"/>. </returns>
        public static DeviceGenericParms DecodeInstance( XdrDecodingStreamBase decoder )
        {
            return new DeviceGenericParms( decoder );
        }

        private DeviceLink _link;
        /// <summary>   Gets or sets the <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/> call. </summary>
        /// <value> The identifier of the device link. </value>
        public DeviceLink Link { get => this._link; set => this._link = value ?? new(); }

        private DeviceFlags _flags;
        /// <summary>   Gets or sets the <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/> options. </summary>
        /// <value> The flags. </value>
        public DeviceFlags Flags { get => this._flags; set => this._flags = value ?? new(); }

        /// <summary>   Gets or sets the lock timeout. </summary>
        /// <remarks>
        /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a lock
        /// to be released. If the device is locked by another link and the <see cref="LockTimeout"/> is non-zero,
        /// the network instrument server allows at least <see cref="LockTimeout"/> milliseconds for a lock to be 
        /// released.
        /// </remarks>
        /// <value> The lock timeout. </value>
        public int LockTimeout { get; set; }

        /// <summary>   Gets or sets the i/o timeout. </summary>
        /// <remarks>
        /// The <see cref="IOTimeout"/> determines how long a network instrument server allows an I/O operation 
        /// to take. If the <see cref="IOTimeout"/> is non-zero, the network instrument server allows at least 
        /// <see cref="IOTimeout"/> milliseconds before returning control to the client with a timeout error.
        /// The time it takes for the I/O operation to complete does not include any time spent waiting for the lock.
        /// </remarks>
        /// <value> The i/o timeout. </value>
        public int IOTimeout { get; set; }

        /// <summary>
        /// Encodes -- that is: serializes -- an object into an XDR stream in compliance to RFC 1832.
        /// </summary>
        /// <param name="encoder">  XDR stream to which information is sent for encoding. </param>
        public void Encode( XdrEncodingStreamBase encoder )
        {
            this.Link.Encode( encoder );
            this.Flags.Encode( encoder );
            this.LockTimeout.Encode( encoder );
            this.IOTimeout.Encode( encoder );
        }

        /// <summary>
        /// Decodes -- that is: deserializes -- an object from an XDR stream in compliance to RFC 1832.
        /// </summary>
        /// <param name="decoder">  XDR stream from which decoded information is retrieved. </param>
        public void Decode( XdrDecodingStreamBase decoder )
        {
            this.Link = new DeviceLink( decoder );
            this.Flags = new DeviceFlags( decoder );
            this.LockTimeout = decoder.DecodeInt();
            this.IOTimeout = decoder.DecodeInt();
        }

    }
}
