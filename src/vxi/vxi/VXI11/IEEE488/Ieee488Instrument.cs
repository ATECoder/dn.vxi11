using System.Net;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.IEEE488
{
    /// <summary>   An IEEE 488 instrument. </summary>
    public class Ieee488Instrument : Ieee488Client
    {

        #region " construction and cleanup "

        /// <summary>   Closes this object. </summary>
        /// <exception cref="AggregateException">   Thrown when an Aggregate error condition occurs. </exception>
        public override void Close()
        {

            List<Exception> exceptions = new();

            try
            {
                this.DisableInterruptServer();
            }
            catch ( Exception ex )
            {
                exceptions.Add( ex );
            }
            finally
            {
                // leave this to the dispose: this.InterruptServer = null;
            }

            try
            {
                base.Close();
            }
            catch ( Exception ex )
            {
                exceptions.Add( ex );
            }

            if ( exceptions.Any() )
            {
                AggregateException aggregateException = new( exceptions );
                throw aggregateException;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        /// <remarks>   2023-01-28. </remarks>
        /// <param name="disposing">    True to release both managed and unmanaged resources; false to
        ///                             release only unmanaged resources. </param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                // dispose managed state (managed objects)
            }

            try
            {
                this.InterruptServer?.Dispose();
                this.InterruptServer = null;
            }
            finally
            {
                base.Dispose( disposing );
            }
        }

        #endregion


        #region " VXI-11 call implementations: Instrument "

        /// <summary>
        /// Creates interrupt channel and starts the <see cref="InterruptChannelServer"/>.
        /// </summary>
        /// <remarks>   2023-01-28. </remarks>
        /// <param name="hostPort"> The host port. </param>
        public virtual void CreateInterruptChannel( int hostPort )
        {
            this.CreateInterruptChannel(  this.IPAddress, hostPort );
        }

        /// <summary>   Creates interrupt channel and starts the <see cref="InterruptChannelServer"/>. </summary>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <param name="hostAddress">  The host device IPv4 address. </param>
        /// <param name="hostPort">     The host port. </param>
        public virtual void CreateInterruptChannel( IPAddress hostAddress, int hostPort )
        {
            if ( !this.Connected ) this.Reconnect();

            var reply = this.CoreClient!.CreateIntrChan( hostAddress, hostPort );

            if ( reply is null )
                throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError );
            else if ( reply.ErrorCode.ErrorCodeValue != Codecs.DeviceErrorCodeValue.NoError )
                throw new DeviceException( $"; failed sending the {nameof( Ieee488Instrument.CreateInterruptChannel )} command", reply.ErrorCode.ErrorCodeValue );
            else
                this.EnableInterruptServer();
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

            DeviceReadStbResp? reply = this.CoreClient!.DeviceReadStb( this.DeviceLink!, Codecs.DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );
            return reply is null
                ? throw new DeviceException( Codecs.DeviceErrorCodeValue.IOError )
                : reply.ErrorCode.ErrorCodeValue != Codecs.DeviceErrorCodeValue.NoError
                    ? throw new DeviceException( $"; failed {nameof( Ieee488Instrument.ReadStatusByte )} reading the status byte.", reply.ErrorCode.ErrorCodeValue )
                    : reply.Stb;
        }

        #endregion

        #region " interrupt server "

        private int _interruptPortNumber;
        /// <summary>   Gets or sets the Interrupt port number. </summary>
        /// <value> The InterruptPort number. </value>
        public int InterruptPortNumber
        {
            get => this._interruptPortNumber;
            set => _ = this.SetProperty( ref this._interruptPortNumber, value );
        }

        /// <summary>   Handles the service request. </summary>
        /// <remarks>   2023-01-26. </remarks>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        VXI-11 event information. </param>
        protected virtual void HandleServiceRequest( object sender, Vxi11EventArgs e )
        {
        }

        protected InterruptChannelServer? InterruptServer { get; set; }

        /// <summary>   Device Interrupt. </summary>
        /// <remarks>
        /// To successfully complete a <c>device_Interrupt</c> RPC, a network instrument server SHALL: <para>
        /// 
        /// 1. Initiate termination of any core channel, in-progress RPC associated with the link except
        /// destroy_link, device_enable_srq, and device_unlock. </para><para>
        /// 
        /// 2. Return with error set to 0, no error, to indicate successful completion </para><para>
        /// 
        /// The intent of this rule is to handle the <c>device_Interrupt</c> RPC ahead of the other operations, but
        /// due to operating system specific implementation details the timeliness cannot be guaranteed. </para>
        /// <para>
        /// 
        /// The <c>device_Interrupt</c> RPC only Interrupts an in-progress RPC, not a queued RPC. </para><para>
        /// 
        /// After replying to the <c>device_Interrupt</c> call, the network instrument server SHALL reply to the
        /// original in-progress call which was Interrupted with error set to 23, Interrupted.  </para><para>
        /// 
        /// Receiving 0 on the Interrupt call at the network instrument client only means that the Interrupt was
        /// successfully delivered to the network instrument server. </para><para>
        /// 
        /// The <c>link id</c> parameter is compared against the active link identifiers . If none match,
        /// <c>device_Interrupt</c> SHALL terminate with error set to 4 invalid link identifier.  </para><para>
        /// 
        /// The operation of <c>device_Interrupt</c> SHALL NOT be affected by locking  </para>
        /// </remarks>
        /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
        /// <returns>   A DeviceErrorCode. </returns>
        protected virtual void StartInterruptServer()
        {
            if ( this.InterruptServer is null )
            {
                this.InterruptServer = new InterruptChannelServer( this.IPAddress, this.InterruptPortNumber );
                this.InterruptServer.ServiceRequested += this.HandleServiceRequest;
                this.InterruptServer.Run();
            }
        }

        /// <summary>   Enables (starts) the interrupt server thread. </summary>
        /// <remarks>   2023-01-26. </remarks>
        public virtual void EnableInterruptServer()
        {
            if ( this.InterruptServer is not null ) return;

            Thread listenThread = new( new ThreadStart( () => this.StartInterruptServer() ) ) {
                Name = "VXI-11 Interrupt Channel Server Thread",
                IsBackground = true
            };
            listenThread.Start();
        }

        /// <summary>   The default time for waiting the Interrupt server to stop listening. </summary>
        public static int InterruptServerDisableTimeoutDefault = 500;

        /// <summary>   The Interrupt server disable loop delay default. </summary>
        public static int InterruptServerDisableLoopDelayDefault = 50;

        /// <summary>   Stops Interrupt server. </summary>
        /// <param name="timeout">      (Optional) The timeout. </param>
        /// <param name="loopDelay">    The loop delay. </param>
        protected virtual void StopInterruptServer( int timeout = 500, int loopDelay = 50 )
        {
            if ( this.InterruptServer is not null && this.InterruptServer.Running )
            {
                this.InterruptServer.ServiceRequested -= this.HandleServiceRequest;
                this.InterruptServer.StopRpcProcessing();
                DateTime endT = DateTime.Now.AddMilliseconds( timeout );
                while ( endT > DateTime.Now && this.InterruptServer.Running )
                {
                    // allow the thread time to address the request
                    Task.Delay( 50 ).Wait();
                }
                this.InterruptServer.Close();
                this.InterruptServer = null;
            }
        }

        /// <summary>   Disables (stops) the Interrupt server thread. </summary>
        /// <remarks>   2023-01-28. </remarks>
        /// <param name="timeout">      (Optional) The timeout. </param>
        /// <param name="loopDelay">    (Optional) The loop delay. </param>
        public virtual void DisableInterruptServer( int timeout = 500, int loopDelay = 50 )
        {
            if ( this.InterruptServer is not null ) return;

            Thread listenThread = new( new ThreadStart( () => this.StopInterruptServer( timeout, loopDelay ) ) ) {
                Name = "VXI-11 Interrupt Channel Server Disabling Thread",
                IsBackground = true
            };
            listenThread.Start();
        }

        #endregion

    }
}
