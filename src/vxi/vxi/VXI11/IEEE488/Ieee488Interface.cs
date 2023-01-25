using System;
using System.Data;
using System.Linq;

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

            DeviceDoCmdResp reply = this.CoreClient.DeviceDoCmd( this.DeviceLink, new DeviceFlags( DeviceOperationFlags.None), this.LockTimeout, this.IOTimeout,
                                                                ( int ) Ieee488InterfaceCommand.SendCommand, true, data.Length, data );
            if ( reply is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.Value != DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Interface.SendCommand )} command.", reply.ErrorCode.Value );
            return reply.GetDataOut();
        }

        /// <summary>   Creates a setup. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="addressList"> List of addresses. </param>
        /// <returns>   A new array of byte. </returns>
        public virtual byte[] CreateSetup( List<byte[]> addressList )
        {
            List<byte> data = new ( new byte[] { ( byte ) (this.BusAddress | ( byte ) GpibCommandArgument.TalkAddress), ( byte ) GpibCommandArgument.Unlisten } );

            foreach ( byte[] addr in addressList )
            {
                if ( addr is not null && addr.Length> 0 )
                {
                    for ( int i = 0; i < addr.Length; i++ )
                    {
                        if ( addr[i] < 0 || addr[i] > 30 )
                        {
                            throw new DeviceException( $"; {nameof( CreateSetup )} failed because {i}-th address {addr[i]} is an invalid bus address.", DeviceErrorCodeValue.InvalidAddress );
                        }
                        data.Add( ( byte ) (addr[i] | ( byte ) ( i == 0 ? GpibCommandArgument.ListenAddress : GpibCommandArgument.SecondaryAddress ) ) );
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
                                                                ( int ) Ieee488InterfaceCommand.BusStatus, interfaceCommand );
        }

        /// <summary>   Read REN line. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   The REN line. </returns>
        public virtual int ReadRenLine()
        {
            return this.BusStatus( ( int ) Ieee488InterfaceCommandOption.RemoteStatus );
        }

        /// <summary>   Reads service request (SRQ) line. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   The service request. </returns>
        public virtual int ReadServiceRequest()
        {
            return this.BusStatus( ( int ) Ieee488InterfaceCommandOption.ServiceRequestStatus );
        }

        /// <summary>   Reads <see cref="Ieee488InterfaceCommandOption.NotDataAcceptedLineStatus"/> NDAC line. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   The NDAC line. </returns>
        public virtual int ReadNdacLine()
        {
            return this.BusStatus( ( int ) Ieee488InterfaceCommandOption.NotDataAcceptedLineStatus );
        }

        /// <summary>   Check if interface device is a system controller. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   True if system controller, false if not. </returns>
        public virtual bool IsSystemController()
        {
            return 0 != this.BusStatus( ( int ) Ieee488InterfaceCommandOption.SystemControllerStatus );
        }

        /// <summary>   Check if interface device is the controller-in-charge. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   True if controller in charge, false if not. </returns>
        public virtual bool IsControllerInCharge()
        {
            return 0 != this.BusStatus( ( int ) Ieee488InterfaceCommandOption.ControllerInChargeStatus );
        }

        /// <summary>   Check if interface device is addressed as a talker. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   True if talker, false if not. </returns>
        public virtual bool IsTalker()
        {
            return 0 != this.BusStatus( ( int ) Ieee488InterfaceCommandOption.TalkerStatus );
        }

        /// <summary>   Check if interface device is addressed as a listener. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <returns>   True if listener, false if not. </returns>
        public virtual bool IsListener()
        {
            return 0 != this.BusStatus( ( int ) Ieee488InterfaceCommandOption.ListenerStatus );
        }

        // Get interface device bus address
        public virtual int GetBusAddress()
        {
            return this.BusStatus( ( int ) Ieee488InterfaceCommandOption.BusAddressStatus );
        }

        #endregion

        #region " Set buss commands "

        /// <summary>   Set ATN line. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <param name="val">  True to value. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public virtual bool SetAtnLine( bool val )
        {
            return this.DeviceLink is not null && this.CoreClient is not null
                   && this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                               ( int ) Ieee488InterfaceCommand.AttentionControl, val );
        }

        /// <summary>   Sets REN line. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <param name="val">  True to value. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        public virtual bool SetRenLine( bool val ) => this.DeviceLink is not null && this.CoreClient is not null
                                                      && this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                               ( int ) Ieee488InterfaceCommand.RemoteEnableControl, val );

        /// <summary>   Pass control to another controller. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="addr"> The address. </param>
        /// <returns>   An int. </returns>
        public virtual int PassControl( int addr )
        {
            return this.DeviceLink is null || this.CoreClient is null
                ? 0
                : addr < 0 || addr > 30
                ? throw new DeviceException( $"; {nameof( PassControl )} failed because {addr} is an invalid bus address.", DeviceErrorCodeValue.InvalidAddress )
                : this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                               ( int ) Ieee488InterfaceCommand.PassControl, addr );
        }

        /// <summary>   Set interface device bus address. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="addr"> The address. </param>
        /// <returns>   An int. </returns>
        public virtual int SetBusAddress( int addr )
        {
            if ( this.DeviceLink is null || this.CoreClient is null ) return 0;

            if ( addr < 0 || addr > 30 )
            {
                throw new DeviceException( $"; {nameof( PassControl )} failed because {addr} is an invalid bus address.", DeviceErrorCodeValue.InvalidAddress );
            }

            int reply = this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                               ( int ) Ieee488InterfaceCommand.BusAddress, addr );

            this.BusAddress = addr;
            return reply;
        }

        /// <summary>   Send Interface Clear (IFC). </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <returns>   A byte[]. </returns>
        public virtual byte[] SendInterfaceClear()
        {
            if ( this.DeviceLink is null || this.CoreClient is null ) return Array.Empty<byte>();

            DeviceDoCmdResp reply = this.CoreClient.DeviceDoCmd( this.DeviceLink, new DeviceFlags( DeviceOperationFlags.None ), this.LockTimeout, this.IOTimeout,
                                                                ( int ) Ieee488InterfaceCommand.InterfaceClearControl, true, 1, Array.Empty<byte>() );
            if ( reply is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.Value != DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Interface.SendInterfaceClear )} command.", reply.ErrorCode.Value );
            return reply.GetDataOut();
        }

        /// <summary>   Find devices. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="address_list"> (Optional) List of address. </param>
        /// <returns>   The found listeners. </returns>
        public virtual List<( int Primary, int Secondary)> FindListeners( List<int>? address_list = null )
        {
            if ( this.DeviceLink is null || this.CoreClient is null ) return new List<( int, int )>();

            if ( address_list is null )
            {
                address_list = Enumerable.Range( 0, 31 ).ToList();
                _ = address_list.Remove( this.BusAddress );
            }
            var found = new List<( int, int )>();
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
                    cmd.Add( (byte) ( addr | ( byte ) GpibCommandArgument.ListenAddress) );

                    _ = this.SendCommand( cmd.ToArray() );

                    _ = this.SetAtnLine( false );

                    Thread.Sleep( 15 );
                    if ( 0 != this.ReadNdacLine() )
                    {
                        found.Add( ( addr, 0 ) );
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
                        Thread.Sleep( 15 );
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
                                Thread.Sleep( 15 );
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
