using System.Net;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Client;

/// <summary>   A VXI-11 instrument client. </summary>
/// <remarks>
/// Implements a <see cref="Vxi11Client"/> with the minimum requirements for a VXI-11 instrument
/// including a <see cref="AbortChannelClient"/> and a <see cref="InterruptChannelServer"/>
/// </remarks>
public class Vxi11InstrumentClient : Vxi11Client
{

    #region " construction and cleanup "

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
    /// resources.
    /// </summary>
    /// <param name="disposing">    True to release large objects and managed and unmanaged resources;
    ///                             false to release only unmanaged resources and large objects. </param>
    protected override void Dispose( bool disposing )
    {
        List<Exception> exceptions = new();
        if ( disposing )
        {
            // dispose managed state (managed objects)

            InterruptChannelServer? interruptServer = this.InterruptServer;
            try
            {
                if ( interruptServer is not null )
                    this.DisableInterruptServerSync();
            }
            catch ( Exception ex )
            {
                { exceptions.Add( ex ); }
            }
            finally
            {
                this.InterruptServer = null;
            }

        }

        // free unmanaged resources and override finalizer

        // set large fields to null

        // call base dispose( bool ).

        try
        {
            base.Dispose( disposing );
        }
        catch ( Exception ex )
        { exceptions.Add( ex ); }
        finally
        {
        }

        if ( exceptions.Any() )
        {
            AggregateException aggregateException = new( exceptions );
            throw aggregateException;
        }
    }

    #endregion

    #region " vxi-11 call implementations: instrument "

    /// <summary>
    /// Creates interrupt channel and starts the <see cref="InterruptChannelServer"/>.
    /// </summary>
    /// <remarks>   2023-01-28. </remarks>
    /// <param name="hostPort"> The host port. </param>
    public virtual async Task CreateInterruptChannel( int hostPort )
    {
        await this.CreateInterruptChannel( this.IPAddress, hostPort );
    }

    /// <summary>   Creates interrupt channel and starts the <see cref="InterruptChannelServer"/>. </summary>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <param name="hostAddress">  The host device IPv4 address. </param>
    /// <param name="hostPort">     The host port. </param>
    public virtual async Task CreateInterruptChannel( IPAddress hostAddress, int hostPort )
    {
        if ( !this.Connected ) this.Reconnect();

        var reply = this.CoreClient!.CreateIntrChan( hostAddress, hostPort );

        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11InstrumentClient.CreateInterruptChannel )} command", reply.ErrorCode );
        else
            await this.EnableInterruptServerAsync();
    }

    /// <summary>   Destroys the interrupt channel. </summary>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    public virtual async Task DestroyInterruptChannel()
    {
        if ( this.InterruptServer is null || !this.InterruptServer.Running ) return;

        // disable the interrupt server
        await this.DisableInterruptServerAsync();

        if ( !this.Connected ) this.Reconnect();

        // send a message to the server to end the interrupt service.

        var reply = this.CoreClient!.DestroyIntrChan();

        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11InstrumentClient.DestroyInterruptChannel )} command", reply.ErrorCode );
    }

    /// <summary>   Enables/disables sending of service requests. </summary>
    /// <param name="enable">   True to enable, false to disable. </param>
    public virtual void EnableSrq( bool enable )
    {
        if ( this.DeviceLink is null || this.CoreClient is null ) return;

        // use the VXI11 Event Codec to build the handle.

        Vxi11EventCodec serviceRequestCodec = Vxi11EventCodec.EncodeInstance( this.ClientId, Vxi11EventType.ServiceRequest );

        this.EnableSrq( enable, serviceRequestCodec.GetHandle() );
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
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11InstrumentClient.EnableSrq )} command.", reply.ErrorCode );
    }

    /// <summary>   Send local command. </summary>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    public virtual void Local()
    {
        if ( !this.Connected ) this.Reconnect();

        var reply = this.CoreClient!.DeviceLocal( this.DeviceLink!, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );

        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11InstrumentClient.Local )} command", reply.ErrorCode );
    }

    /// <summary>   Send remote command. </summary>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    public virtual void Remote()
    {
        if ( !this.Connected ) this.Reconnect();

        var reply = this.CoreClient!.DeviceRemote( this.DeviceLink!, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );

        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11InstrumentClient.Remote )} command", reply.ErrorCode );
    }

    /// <summary>   Reads status byte. </summary>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <returns>   The status byte. </returns>
    public virtual int ReadStatusByte()
    {
        if ( !this.Connected ) this.Reconnect();

        DeviceReadStbResp? reply = this.CoreClient!.DeviceReadStb( this.DeviceLink!, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout );
        return reply is null
            ? throw new DeviceException( DeviceErrorCode.IOError )
            : reply.ErrorCode != DeviceErrorCode.NoError
                ? throw new DeviceException( $"; failed {nameof( Vxi11InstrumentClient.ReadStatusByte )} reading the status byte.", reply.ErrorCode )
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

    /// <summary>   Override this method to handler the VXI-11 event. </summary>
    /// <param name="e">    Event information to send to registered event handlers. </param>
    protected virtual void OnServiceRequested( Vxi11EventArgs e )
    { }
    /// <summary>   Filters and passes on the service request. </summary>
    /// <param name="sender">   Source of the event. </param>
    /// <param name="e">        VXI-11 event information. </param>
    private void HandleServiceRequest( object? sender, Vxi11EventArgs e )
    {
        if ( e.ServiceRequestCodec.ClientId == this.ClientId ) this.OnServiceRequested( e );
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

    /// <summary>   Enables (starts) the interrupt server task. </summary>
    /// <remarks>   2023-01-26. </remarks>
    public virtual async Task EnableInterruptServerAsync()
    {
        await Task.Factory.StartNew( () => { this.StartInterruptServer(); } )
            .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ), TaskContinuationOptions.OnlyOnFaulted );
    }

    /// <summary>   The default time for waiting the Interrupt server to stop listening. </summary>
    public static int InterruptServerDisableTimeoutDefault = 500;

    /// <summary>   The Interrupt server disable loop delay default. </summary>
    public static int InterruptServerDisableLoopDelayDefault = 5;

    /// <summary>   Stops Interrupt server. </summary>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    The loop delay in milliseconds. </param>
    protected virtual void StopInterruptServer( int timeout = 500, int loopDelay = 5 )
    {
        InterruptChannelServer? interruptServer = this.InterruptServer;
        if ( interruptServer?.Running ?? false )
            try
            {
                try
                {
                    interruptServer.ServiceRequested -= this.HandleServiceRequest;
                    interruptServer.StopRpcProcessing();
                    DateTime startTime = DateTime.Now;
                    DateTime endT = DateTime.Now.AddMilliseconds( timeout );
                    while ( endT > DateTime.Now && interruptServer.Running )
                        // allow the task time to address the request
                        Task.Delay( loopDelay ).Wait();
                    if ( interruptServer.Running )
                        throw new InvalidOperationException(
                            $"{nameof( Vxi11InstrumentClient )}.{nameof( StopInterruptServer )} failed stopping {nameof( InterruptChannelServer )} in {(DateTime.Now - startTime).TotalMilliseconds:0}" );
                }
                catch { throw; }
                finally
                {
                    interruptServer.Close();
                }
            }
            catch ( Exception )
            {
                throw;
            }
            finally
            {
                this.InterruptServer = null;
            }
    }

    /// <summary>   Disables (stops) the Interrupt server task. </summary>
    /// <remarks>   2023-01-28. </remarks>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    public virtual async Task DisableInterruptServerAsync( int timeout = 500, int loopDelay = 5 )
    {
        await Task.Factory.StartNew( () => { this.StopInterruptServer( timeout, loopDelay ); } )
            .ContinueWith( failedTask => this.OnThreadException( new ThreadExceptionEventArgs( failedTask.Exception ) ), TaskContinuationOptions.OnlyOnFaulted );

    }

    /// <summary>   Disables the interrupt server synchronously. </summary>
    /// <remarks>   2023-01-30. </remarks>
    /// <param name="timeout">      (Optional) The timeout in milliseconds. </param>
    /// <param name="loopDelay">    (Optional) The loop delay in milliseconds. </param>
    /// <returns>   A Task. </returns>
    public virtual void DisableInterruptServerSync( int timeout = 500, int loopDelay = 5 )
    {
        InterruptChannelServer? interruptServer = this.InterruptServer;
        try
        {
            if ( interruptServer is not null )
            {
                using Task? task = this.DisableInterruptServerAsync();
                task.Wait();
                if ( task.IsFaulted ) throw task.Exception;
            }
        }
        catch ( Exception )
        {
            throw;
        }
        finally
        {
            this.InterruptServer = null;
        }
    }

    #endregion

}
