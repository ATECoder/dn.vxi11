using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.IEEE488
{
    public class Ieee488Interface : Ieee488Client
    {

        #region " construction and cleanup "

        public override void Connect( string hostAddress, string interfaceDeviceString, int connectTimeout = 3000 )
        {
            base.Connect( hostAddress, interfaceDeviceString, connectTimeout );
            this.BusAddress = this.GetBusAddress();

        }

        /// <summary>   Gets or sets the bus address. </summary>
        /// <value> The bus address. </value>
        public int BusAddress { get; private set; }


        #endregion

        #region " VXI-11 commands "

        public virtual byte[] SendCommand( byte[] data )
        {
            if ( this.DeviceLink is null || this.CoreClient is null ) return Array.Empty<byte>();

            DeviceDoCmdResp reply = this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                                ( int ) Ieee488InterfaceCommand.SendCommand, true, 1, data );
            if ( reply is null )
                throw new DeviceException( DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.ErrorCodeValue != DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Interface.SendCommand )} command.", reply.ErrorCode.ErrorCodeValue );
            return reply.GetDataOut();
        }

        /// <summary>   Creates a setup. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="addressList"> List of addresses. </param>
        /// <returns>   A new array of byte. </returns>
        public virtual byte[] CreateSetup( List<byte[]> addressList )
        {
            List<byte> data = new( new byte[] { ( byte ) (this.BusAddress | ( byte ) GpibCommandArgument.TalkAddress), ( byte ) GpibCommandArgument.Unlisten } );

            foreach ( byte[] addr in addressList )
            {
                if ( addr is not null && addr.Length > 0 )
                {
                    for ( int i = 0; i < addr.Length; i++ )
                    {
                        if ( addr[i] < 0 || addr[i] > 30 )
                        {
                            throw new DeviceException( $"; {nameof( CreateSetup )} failed because {i}-th address {addr[i]} is an invalid bus address.", DeviceErrorCodeValue.InvalidAddress );
                        }
                        data.Add( ( byte ) (addr[i] | ( byte ) (i == 0 ? GpibCommandArgument.ListenAddress : GpibCommandArgument.SecondaryAddress)) );
                    }
                }

            }
            return data.ToArray();
        }

        /// <summary>   Sends a setup. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <param name="addressList">  List of addresses. </param>
        /// <returns>   A byte[]. </returns>
        public virtual byte[] SendSetup( List<byte[]> addressList )
        {
            return this.SendCommand( this.CreateSetup( addressList ) );
        }

        /// <summary>   Bus status. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <param name="interfaceCommand"> The interface command. </param>
        /// <returns>   An int. </returns>
        public virtual int BusStatus( int interfaceCommand )
        {
            return this.DeviceLink is null || this.CoreClient is null
                ? 0
                : this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                                ( int ) Ieee488InterfaceCommand.BusStatus, 2, interfaceCommand );
        }

        /// <summary>   Read REN line. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   1 if the REN message is true, 0 otherwise. </returns>
        public virtual int ReadRenLine()
        {
            return this.BusStatus( ( int ) Ieee488InterfaceCommandOption.RemoteStatus );
        }

        /// <summary>   Reads service request (SRQ) line. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   1 if the SRQ message is true, 0 otherwise. </returns>
        public virtual int ReadServiceRequest()
        {
            return this.BusStatus( ( int ) Ieee488InterfaceCommandOption.ServiceRequestStatus );
        }

        /// <summary>   Reads <see cref="Ieee488InterfaceCommandOption.NotDataAcceptedLineStatus"/> NDAC line. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   1 if the NDAC message is true, 0 otherwise. </returns>
        public virtual int ReadNdacLine()
        {
            return this.BusStatus( ( int ) Ieee488InterfaceCommandOption.NotDataAcceptedLineStatus );
        }

        /// <summary>   Check if interface device is a system controller. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is in the
        /// system control active state, <see langword="false"/> otherwise. </returns>
        public virtual bool IsSystemController()
        {
            return 0 != this.BusStatus( ( int ) Ieee488InterfaceCommandOption.SystemControllerStatus );
        }

        /// <summary>   Check if interface device is the controller-in-charge. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>
        /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is not in the controller
        /// idle state, <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool IsControllerInCharge()
        {
            return 0 != this.BusStatus( ( int ) Ieee488InterfaceCommandOption.ControllerInChargeStatus );
        }

        /// <summary>   Check if interface device is addressed as a talker. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>
        /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is addressed to talk, <see langword="false"/>
        /// otherwise.
        /// </returns>
        public virtual bool IsTalker()
        {
            return 0 != this.BusStatus( ( int ) Ieee488InterfaceCommandOption.TalkerStatus );
        }

        /// <summary>   Check if interface device is addressed as a listener. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>
        /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is addressed to listen, <see langword="false"/>
        /// </returns>
        public virtual bool IsListener()
        {
            return 0 != this.BusStatus( ( int ) Ieee488InterfaceCommandOption.ListenerStatus );
        }

        /// <summary>   Get interface device bus address. </summary>
        /// <remarks>   2023-01-25. </remarks>
        /// <returns>   The TCP/IP-IEEE 488.1 Interface Device's address (0-30). </returns>
        public virtual int GetBusAddress()
        {
            return this.BusStatus( ( int ) Ieee488InterfaceCommandOption.BusAddressStatus );
        }

        #endregion

        #region " Set buss commands "

        /// <summary>   Set ATN line. </summary>
        /// <remarks>
        /// TCP/IP-IEEE 488.1 Interface Device sets the ATN line as follows:
        /// <list type="number"><item>
        /// If the `data_in` parameter is non-zero, then set the ATN line true. </item><item>
        /// If the `data_in` parameter is zero, then set the ATN line false. </item></list>
        /// The returned `data_out` is the same as the received `data_in`.
        /// </remarks>
        /// <param name="value">  The value. Note that a <see cref="bool"/> <see langword="true"/> 
        /// is XDR encoded as 1.
        /// </param>
        public virtual bool SetAtnLine( bool value )
        {
            return this.DeviceLink is not null && this.CoreClient is not null
                && value == this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                           ( int ) Ieee488InterfaceCommand.AttentionControl, 2, value );
        }

        /// <summary>   Sets REN line. </summary>
        /// <remarks>
        /// TCP/IP-IEEE 488.1 Interface Device sets the REN line as follows:
        /// <list type="number"><item>
        /// If the `data_in` parameter is non-zero, then set the SRE( send remote enable) message true.  </item>
        /// <item>
        /// If the `data_in` parameter is zero, then set the SRE( send remote enable) message false.  </item>
        /// </list>
        /// The returned `data_out` is be same as the received `data_in`.
        /// </remarks>
        /// <param name="value">    The value. Note that a <see cref="bool"/> <see langword="true"/>
        ///                         is XDR encoded as 1. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public virtual bool SetRenLine( bool value )
        {
            return this.DeviceLink is not null && this.CoreClient is not null
                && value == this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                           ( int ) Ieee488InterfaceCommand.RemoteEnableControl, 2, value );
        }

        /// <summary>   Pass control to another controller. </summary>
        /// <remarks>
        /// The TCP/IP-IEEE 488.1 Interface Device executes the `PASS CONTROL` control sequence described
        /// in IEEE 488.2, 16.2.14 where the talk address is constructed from the value in `data_in`
        /// bitwise OR-ed with 0x80. The returned `data_out` is the same as the received `data_in`.
        /// </remarks>
        /// <param name="addr"> The address. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public virtual bool PassControl( int addr )
        {
            return this.DeviceLink is not null && this.CoreClient is not null
                    && (addr < 0 || addr > 30
                        ? throw new DeviceException( $"; {nameof( PassControl )} failed because {addr} is an invalid bus address.", DeviceErrorCodeValue.ParameterError )
                        : addr == this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                       ( int ) Ieee488InterfaceCommand.PassControl, 4, addr ));
        }

        /// <summary>   Set interface device bus address. </summary>
        /// <remarks>
        /// the TCP/IP-IEEE 488.1 Interface Device sets its address to the contents of `data_in`. If
        /// `data_in` does not contain a legal value, device_docmd returns immediately with error set to
        /// `parameter error` (5). The returned `data_out` is the same as the received `data_in`.
        /// </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="addr"> The address. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public virtual bool SetBusAddress( int addr )
        {
            if ( this.DeviceLink is null || this.CoreClient is null ) return false;

            if ( addr < 0 || addr > 30 )
            {
                throw new DeviceException( $"; {nameof( PassControl )} failed because {addr} is an invalid bus address.", DeviceErrorCodeValue.ParameterError );
            }

            int reply = this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                               ( int ) Ieee488InterfaceCommand.BusAddress, 4, addr );
            if ( reply == addr )
                this.BusAddress = addr;
            return addr == reply;
        }

        /// <summary>   Send Interface Clear (IFC). </summary>
        /// <remarks>   TCP/IP-IEEE 488.1 Interface Device
        /// executes the `SEND IFC` control sequence described in IEEE 488.2, 16.2.8. The returned `data_out`
        /// has `data_out.data_out_len` set to zero. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <returns>   A byte[]. </returns>
        public virtual byte[] SendInterfaceClear()
        {
            if ( this.DeviceLink is null || this.CoreClient is null ) return Array.Empty<byte>();

            DeviceDoCmdResp reply = this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                                ( int ) Ieee488InterfaceCommand.InterfaceClearControl, true, 1, Array.Empty<byte>() );
            if ( reply is null )
                throw new DeviceException( DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.ErrorCodeValue != DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Interface.SendInterfaceClear )} command.", reply.ErrorCode.ErrorCodeValue );
            return reply.GetDataOut();
        }

        /// <summary>   Find devices. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="address_list">         (Optional) List of address. </param>
        /// <param name="readAfterWriteDelay">  (Optional) The read after write delay in milliseconds. </param>
        /// <returns>   The found listeners. </returns>
        public virtual List<(int Primary, int Secondary)> FindListeners( List<int>? address_list = null, int readAfterWriteDelay = 15 )
        {
            if ( this.DeviceLink is null || this.CoreClient is null ) return new List<(int, int)>();

            if ( address_list is null )
            {
                address_list = Enumerable.Range( 0, 31 ).ToList();
                _ = address_list.Remove( this.BusAddress );
            }
            var found = new List<(int, int)>();
            try
            {
                this.Lock();
                foreach ( var addr in address_list )
                {
                    // check for listener at primary address
                    var cmd = new List<byte> {
                        ( byte ) GpibCommandArgument.Unlisten,
                        ( byte ) GpibCommandArgument.Untalk,
                        ( byte ) (this.BusAddress | ( byte ) GpibCommandArgument.TalkAddress)
                    };

                    if ( addr < 0 || addr > 30 )
                    {
                        throw new DeviceException( $"; {nameof( FindListeners )} failed because {addr} is an invalid bus address.", DeviceErrorCodeValue.InvalidAddress );
                    }
                    cmd.Add( ( byte ) (addr | ( byte ) GpibCommandArgument.ListenAddress) );

                    _ = this.SendCommand( cmd.ToArray() );

                    _ = this.SetAtnLine( false );

                    Task.Delay( readAfterWriteDelay ).Wait();

                    if ( 0 != this.ReadNdacLine() )
                    {
                        found.Add( (addr, 0) );
                    }
                    else
                    {
                        // check for listener at any sub-address
                        cmd = new List<byte> {
                            ( byte ) GpibCommandArgument.Unlisten,
                            ( byte ) GpibCommandArgument.Untalk,
                            ( byte ) (this.BusAddress | ( byte ) GpibCommandArgument.TalkAddress),
                            ( byte ) (addr | ( byte ) GpibCommandArgument.ListenAddress)
                        };

                        foreach ( var sa in Enumerable.Range( 0, 31 ) )
                        {
                            cmd.Add( ( byte ) (sa | ( byte ) GpibCommandArgument.SecondaryAddress) );
                        }
                        _ = this.SendCommand( cmd.ToArray() );
                        _ = this.SetAtnLine( false );
                        Task.Delay( readAfterWriteDelay ).Wait();

                        if ( 0 != this.ReadNdacLine() )
                        {
                            // find specific sub-address
                            foreach ( var sa in Enumerable.Range( 0, 31 ) )
                            {
                                cmd = new List<byte> {
                                    ( byte ) GpibCommandArgument.Unlisten,
                                    ( byte ) GpibCommandArgument.Untalk,
                                    ( byte ) (this.BusAddress | ( byte ) GpibCommandArgument.TalkAddress),
                                    ( byte ) (addr | ( byte ) GpibCommandArgument.ListenAddress),
                                    ( byte ) (sa | ( byte ) GpibCommandArgument.SecondaryAddress)
                                };

                                _ = this.SendCommand( cmd.ToArray() );
                                _ = this.SetAtnLine( false );
                                Task.Delay( readAfterWriteDelay ).Wait();

                                if ( 0 != this.ReadNdacLine() )
                                    found.Add( (addr, sa) );
                            }
                        }
                    }
                }
                this.Unlock();
            }
            catch
            {
                this.Unlock();
                throw;
            }
            return found;
        }


        #endregion

    }
}
