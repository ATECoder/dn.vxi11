using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Client;

/// <summary>   A VXI-11 interface client. </summary>
public class Vxi11InterfaceClient : Vxi11Client
{

    #region " construction and cleanup "

    /// <summary>   Connects. </summary>
    /// <param name="hostAddress">              The host address. </param>
    /// <param name="interfaceDeviceString">    The device name. </param>
    /// <param name="connectTimeout">           (Optional) The connect timeout. </param>
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

    /// <summary>   Sends a command using the <see cref="CoreChannelClient.DeviceDoCmd(DeviceDoCmdParms)"/>
    /// RPC Call. </summary>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <exception cref="DeviceException">              Thrown when a Device error condition occurs. </exception>
    /// <param name="commandCode">  The command code. </param>
    /// <param name="dataSize">     Size of individual data elements. </param>
    /// <param name="data">         The data. </param>
    /// <returns>   A byte[]. </returns>
    public virtual byte[] SendCommand( int commandCode, int dataSize, byte[] data )
    {
        if ( this.DeviceLink is null || this.CoreClient is null )
            throw new InvalidOperationException( "Interface connection has yet to be established" );
           
        DeviceDoCmdResp reply = this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                                             commandCode, true, dataSize, data );
        
        if ( reply is null )
            throw new DeviceException( DeviceErrorCode.IOError );
        else if ( reply.ErrorCode != DeviceErrorCode.NoError )
            throw new DeviceException( $"; failed sending the {nameof( Vxi11InterfaceClient.SendCommand )} command.", reply.ErrorCode );
        return reply.GetDataOut();
    }

    /// <summary>   Sends a command using the <see cref="CoreChannelClient.DeviceDoCmd(DeviceDoCmdParms)"/>
    /// RPC Call. </summary>
    /// <param name="commandCode">  The command code. </param>
    /// <param name="dataSize">     Size of individual data elements. </param>
    /// <param name="data">         The data. </param>
    /// <returns>   A byte[]. </returns>
    public virtual byte[] SendCommand( InterfaceCommand commandCode, int dataSize, byte[] data )
    {
        return this.SendCommand( ( int ) commandCode, dataSize, data );
    }

    /// <summary>   Sends a command using the <see cref="CoreChannelClient.DeviceDoCmd(DeviceDoCmdParms)"/>
    /// RPC Call. </summary>
    /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
    ///                                                 invalid. </exception>
    /// <param name="commandCode">  The command code. </param>
    /// <param name="dataSize">     Size of individual data elements. </param>
    /// <param name="value">        The value. Note that a <see cref="bool"/> <see langword="true"/>
    ///                             is XDR encoded as 1. </param>
    /// <returns>   an integer. </returns>
    public virtual int SendCommand( int commandCode, int dataSize, int value )
    {
        if ( this.DeviceLink is null || this.CoreClient is null )
            throw new InvalidOperationException( "Interface connection has yet to be established" );

        return this.CoreClient.DeviceDoCmd( this.DeviceLink, DeviceOperationFlags.None, this.LockTimeout, this.IOTimeout,
                                            commandCode, dataSize, value );
    }

    /// <summary>
    /// Sends a command using the <see cref="CoreChannelClient.DeviceDoCmd(DeviceDoCmdParms)"/>
    /// RPC Call.
    /// </summary>
    /// <param name="commandCode">  The command code. </param>
    /// <param name="dataSize">     Size of individual data elements. </param>
    /// <param name="value">        The value. Note that a <see cref="bool"/> <see langword="true"/>
    ///                             is XDR encoded as 1. </param>
    /// <returns>   an integer. </returns>
    public virtual int SendCommand( InterfaceCommand commandCode, int dataSize, int value )
    {
        return this.SendCommand( ( int ) commandCode, dataSize, value );
    }

    /// <summary>   Sends a command using the <see cref="CoreChannelClient.DeviceDoCmd(DeviceDoCmdParms)"/>
    /// RPC Call. </summary>
    /// <remarks>   2023-02-07. </remarks>
    /// <param name="commandCode">  The command code. </param>
    /// <param name="dataSize">     Size of individual data elements. </param>
    /// <param name="value">        The value. Note that a <see cref="bool"/> <see langword="true"/>
    ///                             is XDR encoded as 1. </param>
    /// <returns>   an integer. </returns>
    public virtual int SendCommand( InterfaceCommand commandCode, int dataSize, InterfaceCommandOption value )
    {
        return this.SendCommand( ( int ) commandCode, dataSize, ( int ) value );
    }

    #endregion

    #region " Interface methods "

    /// <summary>   Sends a command. </summary>
    /// <param name="data"> The data. </param>
    /// <returns>   A byte[]. </returns>
    public virtual byte[] SendCommand( byte[] data )
    {
        return this.SendCommand( InterfaceCommand.SendCommand, 1, data );
    }

    /// <summary>   Reads bus status. </summary>
    /// <param name="interfaceCommand"> The interface command option <see cref="InterfaceCommandOption"/>. </param>
    /// <returns>   An int. </returns>
    public virtual int ReadBusStatus( InterfaceCommandOption interfaceCommand )
    {
        return this.SendCommand( InterfaceCommand.BusStatus, 2, interfaceCommand );
    }

    /// <summary>   Read REN line. </summary>
    /// <returns>   1 if the REN message is true, 0 otherwise. </returns>
    public virtual int ReadRenLine()
    {
        return this.ReadBusStatus( InterfaceCommandOption.RemoteStatus );
    }

    /// <summary>   Reads service request (SRQ) line. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   1 if the SRQ message is true, 0 otherwise. </returns>
    public virtual int ReadServiceRequest()
    {
        return this.ReadBusStatus( InterfaceCommandOption.ServiceRequestStatus );
    }

    /// <summary>   Reads <see cref="InterfaceCommandOption.NotDataAcceptedLineStatus"/> NDAC line. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   1 if the NDAC message is true, 0 otherwise. </returns>
    public virtual int ReadNdacLine()
    {
        return this.ReadBusStatus( InterfaceCommandOption.NotDataAcceptedLineStatus );
    }

    /// <summary>   Check if interface device is a system controller. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>   <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is in the
    /// system control active state, <see langword="false"/> otherwise. </returns>
    public virtual bool IsSystemController()
    {
        return 0 != this.ReadBusStatus( InterfaceCommandOption.SystemControllerStatus );
    }

    /// <summary>   Check if interface device is the controller-in-charge. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is not in the controller
    /// idle state, <see langword="false"/> otherwise.
    /// </returns>
    public virtual bool IsControllerInCharge()
    {
        return 0 != this.ReadBusStatus( InterfaceCommandOption.ControllerInChargeStatus );
    }

    /// <summary>   Check if interface device is addressed as a talker. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is addressed to talk, <see langword="false"/>
    /// otherwise.
    /// </returns>
    public virtual bool IsTalker()
    {
        return 0 != this.ReadBusStatus( InterfaceCommandOption.TalkerStatus );
    }

    /// <summary>   Check if interface device is addressed as a listener. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <returns>
    /// <see langword="true"/> if the TCP/IP-IEEE 488.1 Interface Device is addressed to listen, <see langword="false"/>
    /// </returns>
    public virtual bool IsListener()
    {
        return 0 != this.ReadBusStatus( InterfaceCommandOption.ListenerStatus );
    }

    /// <summary>   Get interface device bus address. </summary>
    /// <remarks>   2023-01-25. </remarks>
    /// <returns>   The TCP/IP-IEEE 488.1 Interface Device's address (0-30). </returns>
    public virtual int GetBusAddress()
    {
        return this.ReadBusStatus( InterfaceCommandOption.BusAddressStatus );
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
        return value == (this.SendCommand( InterfaceCommand.AttentionControl, 2, value ? 1 : 0 ) == 1);
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
        return value == (this.SendCommand( InterfaceCommand.RemoteEnableControl, 2, value ? 1 : 0 ) == 1);
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
                    ? throw new DeviceException( $"; {nameof( PassControl )} failed because {addr} is an invalid bus address.", DeviceErrorCode.ParameterError )
                    : addr == this.SendCommand( InterfaceCommand.PassControl, 4, addr ));
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
            throw new DeviceException( $"; {nameof( PassControl )} failed because {addr} is an invalid bus address.", DeviceErrorCode.ParameterError );

        int reply = this.SendCommand( InterfaceCommand.BusAddress, 4, addr );
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

        return this.SendCommand( InterfaceCommand.InterfaceClearControl, 1, Array.Empty<byte>() );
    }

    #endregion

}
