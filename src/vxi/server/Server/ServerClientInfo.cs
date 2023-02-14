using System.Collections;

using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11.Server;

/// <summary>   Information about the server client. </summary>
public class ServerClientInfo : IEquatable<ServerClientInfo>
{

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-02-13. </remarks>
    /// <param name="createLinkParameters"> The parameters defining the created link. </param>
    /// <param name="linkId">               The identifier of the link. </param>
    public ServerClientInfo( CreateLinkParms createLinkParameters, int linkId )
    {
        this.ClientId = createLinkParameters.ClientId;
        this.DeviceName = createLinkParameters.DeviceName;
        this.LockDevice = createLinkParameters.LockDevice;
        this.LockTimeout = createLinkParameters.LockTimeout;
        this.LinkId = linkId;
        this._handle = Array.Empty<byte>();
    }

    /// <summary>   Gets or sets a value indicating whether the link created. </summary>
    /// <remarks> This is used by the server device to enumerate all the clients which 
    /// are awaiting a reply to their create link query. </remarks>
    /// <value> True if link created, false if not. </value>
    public bool LinkCreated { get; set; }

    /// <summary>   Gets or sets the identifier of the link. </summary>
    /// <value> The identifier of the link. </value>
    public int LinkId { get; set; }

    /// <summary>   Gets or sets the identifier of the client. </summary>
    /// <value> The identifier of the client. </value>
    public int ClientId { get; set; }

    /// <summary>   Gets or sets a value indicating whether the device is locked. </summary>
    /// <remarks>
    /// <see cref="bool"/> types are encoded as <see cref="int"/> with 1 is <see langword="true"/>.
    /// </remarks>
    /// <value> True if lock device, false if not. </value>
    public bool LockDevice { get; set; }

    /// <summary>   Gets or sets the lock timeout. </summary>
    /// <remarks>
    /// The <see cref="LockTimeout"/> determines how long a network instrument server will wait for a
    /// lock to be released. If the device is locked by another link and the <see cref="LockTimeout"/>
    /// is non-zero, the network instrument server allows at least <see cref="LockTimeout"/>
    /// milliseconds for a lock to be released. <para>
    /// 
    /// This value is defined as <see cref="int"/> type in spite of the specifications' call for
    /// using an unsigned integer because the timeout value is unlikely to exceed the maximum integer
    /// value.
    /// </para>
    /// </remarks>
    /// <value> The time to wait on a lock. </value>
    public int LockTimeout { get; set; }

    /// <summary>   Gets or sets the lock release time. </summary>
    /// <value> The lock release time. </value>
    public DateTime LockReleaseTime { get; set; }

    /// <summary>   Active lock timeout. </summary>
    /// <remarks>   2023-02-14. </remarks>
    public void ActivateLockTimeout( int? lockTimeout = null )
    {
        this.LockReleaseTime = this.LockDevice
            ? DateTime.UtcNow.AddMilliseconds( lockTimeout ?? this.LockTimeout )
            : DateTime.UtcNow;
    }

    /// <summary>   Releases the lock timeout. </summary>
    /// <remarks>   2023-02-14. </remarks>
    public void ReleaseLockTimeout()
    {
        this.LockReleaseTime = DateTime.UtcNow;
    }

    /// <summary>   Query if this device is locked by this client. </summary>
    /// <remarks>   2023-02-13. </remarks>
    /// <returns>   True if locked, false if not. </returns>
    public bool IsLocked()
    {
        return this.LockDevice && this.LockReleaseTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Gets or sets the device name also called device name, e.g., inst0, gpib,5 or
    /// usb0[...].
    /// </summary>
    /// <value> The device name. </value>
    public string DeviceName { get; set; }

    private byte[] _handle;

    /// <summary>   Gets the handle. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <returns>   An array of byte. </returns>
    public byte[] GetHandle() { return this._handle; }

    /// <summary>   Gets or sets a value indicating whether the interrupt is enabled. </summary>
    /// <value> True if interrupt enabled, false if not. </value>
    public bool InterruptEnabled { get; private set; }

    /// <summary>   Sets a handle. </summary>
    /// <remarks>   2023-02-09. </remarks>
    /// <param name="enable">   True to enable, false to disable. </param>
    /// <param name="handle">   The handle. </param>
    public void EnableInterrupt( bool enable, byte[] handle )
    {
        this._handle = handle;
        this.InterruptEnabled = enable;
    }

    public bool Equals( ServerClientInfo other )
    {
        return ( other is not null )
            && Enumerable.SequenceEqual( this._handle , other.GetHandle() )
            && this.ClientId == other.ClientId
            && this.LinkId == other.LinkId
            && this.InterruptEnabled == other.InterruptEnabled;
        throw new NotImplementedException();
    }

}
