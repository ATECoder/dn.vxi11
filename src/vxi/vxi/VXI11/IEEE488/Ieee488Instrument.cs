using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.IEEE488
{
    /// <summary>   An IEEE 488 instrument. </summary>
    public class Ieee488Instrument : Ieee488Client
    {



        #region " VXI-11 call implementations: Instrument "

        /// <summary>   Creates interrupt channel. </summary>
        /// <remarks>   2023-01-24. </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="hostAddress">  The host device IPv4 address. </param>
        /// <param name="hostPort">     The host port. </param>
        public virtual void CreateInterruptChannel( int hostAddress, int hostPort )
        {
            if ( !this.Connected ) this.Reconnect();

            var reply = this.CoreClient!.CreateIntrChan( hostAddress, hostPort );

            if ( reply is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.ErrorCodeValue != Codecs.DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Instrument.CreateInterruptChannel )} command", reply.ErrorCode.ErrorCodeValue );
        }

        /// <summary>   Destroys the interrupt channel. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        public virtual void DestroyInterruptChannel()
        {
            if ( !this.Connected ) this.Reconnect();

            var reply = this.CoreClient!.DestroyIntrChan();

            if ( reply is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.ErrorCodeValue != Codecs.DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Instrument.DestroyInterruptChannel )} command", reply.ErrorCode.ErrorCodeValue );
        }

        /// <summary>   Enables/disables sending of service requests. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="enable">   True to enable, false to disable. </param>
        /// <param name="handle">   The handle. Host specific data for handling the service request. </param>
        public virtual void EnableSrq( bool enable, byte[] handle )
        {
            if ( this.DeviceLink is null || this.CoreClient is null ) return;
            DeviceError reply = this.CoreClient.DeviceEnableSrq( this.DeviceLink, enable, handle );

            if ( reply is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.ErrorCodeValue != DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Instrument.EnableSrq )} command.", reply.ErrorCode.ErrorCodeValue );
        }

        /// <summary>   Send local command. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        public virtual void Local()
        {
            if ( !this.Connected ) this.Reconnect();

            var reply = this.CoreClient!.DeviceLocal( this.DeviceLink!, Codecs.DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );

            if ( reply is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.ErrorCodeValue != Codecs.DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Instrument.Local )} command", reply.ErrorCode.ErrorCodeValue );
        }

        /// <summary>   Send remote command. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        public virtual void Remote()
        {
            if ( !this.Connected ) this.Reconnect();

            var reply = this.CoreClient!.DeviceRemote( this.DeviceLink!, Codecs.DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );

            if ( reply is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.ErrorCodeValue != Codecs.DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Instrument.Remote )} command", reply.ErrorCode.ErrorCodeValue );
        }

        /// <summary>   Reads status byte. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <returns>   The status byte. </returns>
        public virtual byte ReadStatusByte()
        {
            if ( !this.Connected ) this.Reconnect();

            var reply = this.CoreClient!.DeviceReadStb( this.DeviceLink!, Codecs.DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );
            return reply is null
                ? throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError )
                : reply.ErrorCode.ErrorCodeValue != Codecs.DeviceErrorCodeValue.NoError
                    ? throw new DeviceException( $"; failed {nameof( Ieee488Instrument.ReadStatusByte )} reading the status byte.", reply.ErrorCode.ErrorCodeValue )
                    : reply.Stb;
        }

        #endregion

    }
}
