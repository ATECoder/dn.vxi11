namespace cc.isr.VXI11.IEEE488
{
    /// <summary>   An IEEE 488 instrument. </summary>
    public class Ieee488Instrument : Ieee488Client
    {

        /// <summary>   Reads status byte. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <returns>   The status byte. </returns>
        public virtual byte ReadStatusByte()
        {
            if ( !this.Connected ) this.Reconnect();

            var response = this.CoreClient!.DeviceReadStb( this.DeviceLink!, Codecs.DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );
            return response is null
                ? throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError )
                : response.ErrorCode.Value != Codecs.DeviceErrorCodeValue.NoError
                    ? throw new DeviceException( "; failed reading status byte", response.ErrorCode.Value )
                    : response.Stb;
        }

        /// <summary>   Send remote command. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        public virtual void Remote()
        {
            if ( !this.Connected ) this.Reconnect();

            var response = this.CoreClient!.DeviceRemote( this.DeviceLink!, Codecs.DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );

            if ( response is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( response.ErrorCode.Value != Codecs.DeviceErrorCodeValue.NoError )
                throw new DeviceException( "; failed sending Remote command", response.ErrorCode.Value );
        }

        /// <summary>   Send local command. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        public virtual void Local()
        {
            if ( !this.Connected ) this.Reconnect();

            var response = this.CoreClient!.DeviceLocal( this.DeviceLink!, Codecs.DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );

            if ( response is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( response.ErrorCode.Value != Codecs.DeviceErrorCodeValue.NoError )
                throw new DeviceException( "; failed sending Local command", response.ErrorCode.Value );
        }

    }
}
