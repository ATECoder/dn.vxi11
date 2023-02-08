namespace cc.isr.VXI11.Client;

public class Vxi11GpibInterfaceClient : Vxi11InterfaceClient
{

    #region " GPIB Interface Commands "

    /// <summary>   Creates a setup. </summary>
    /// <remarks>   2023-01-24. </remarks>
    /// <exception cref="DeviceException">  Thrown when a Device error condition occurs. </exception>
    /// <param name="addressList"> List of addresses. </param>
    /// <returns>   A new array of byte. </returns>
    public virtual byte[] CreateSetup( List<byte[]> addressList )
    {
        List<byte> data = new( new byte[] { ( byte ) (this.BusAddress | ( byte ) GpibCommandArgument.TalkAddress), ( byte ) GpibCommandArgument.Unlisten } );

        foreach ( byte[] addr in addressList )
            if ( addr?.Length > 0 )
                for ( int i = 0; i < addr.Length; i++ )
                {
                    if ( addr[i] < 0 || addr[i] > 30 )
                        throw new DeviceException( $"; {nameof( CreateSetup )} failed because {i}-th address {addr[i]} is an invalid bus address.", DeviceErrorCode.InvalidAddress );
                    data.Add( ( byte ) (addr[i] | ( byte ) (i == 0 ? GpibCommandArgument.ListenAddress : GpibCommandArgument.SecondaryAddress)) );
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
                    throw new DeviceException( $"; {nameof( FindListeners )} failed because {addr} is an invalid bus address.", DeviceErrorCode.InvalidAddress );
                cmd.Add( ( byte ) (addr | ( byte ) GpibCommandArgument.ListenAddress) );

                _ = this.SendCommand( cmd.ToArray() );

                _ = this.SetAtnLine( false );

                Task.Delay( readAfterWriteDelay ).Wait();

                if ( 0 != this.ReadNdacLine() )
                    found.Add( (addr, 0) );
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
                        cmd.Add( ( byte ) (sa | ( byte ) GpibCommandArgument.SecondaryAddress) );
                    _ = this.SendCommand( cmd.ToArray() );
                    _ = this.SetAtnLine( false );
                    Task.Delay( readAfterWriteDelay ).Wait();

                    if ( 0 != this.ReadNdacLine() )
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
