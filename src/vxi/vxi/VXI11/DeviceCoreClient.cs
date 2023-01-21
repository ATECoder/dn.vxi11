using System.Net;

using cc.isr.ONC.RPC.Client;
using cc.isr.VXI11.Codecs;

namespace cc.isr.VXI11;

/// <summary>
/// The class <see cref="DeviceCoreClient"/> implements the client stub proxy for the
/// <see cref="Vxi11ProgramConstants.DeviceCoreProgram"/> remote program. It provides method
/// stubs which, when called, in turn call the appropriate remote method (procedure).
/// </summary>
/// <remarks>
/// <para>
///  
/// Renamed from <c>vxi11_DEVICE_CORE_Client</c> </para>
/// </remarks>
public class DeviceCoreClient : OncRpcClientStubBase
{

    #region " construction and cleanup "

    /// <summary>
    /// Constructs a <see cref="DeviceCoreClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceCoreProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <param name="client">   The ONC/RPC client connection object implementing the particular
    ///                         protocol and program. </param>
    public DeviceCoreClient( OncRpcClientBase client ) : base( client )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceCoreClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceCoreProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program.. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocols.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocols.OncRpcTcp"/>. </param>
    public DeviceCoreClient( IPAddress host, OncRpcProtocols protocol, int timeout ) : this( host, Vxi11ProgramConstants.DeviceCoreProgram,
                                                                                                   Vxi11ProgramConstants.DeviceCoreVersion,
                                                                                                   0, protocol, timeout )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceCoreClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceCoreProgram"/> 
    /// remote program can be accessed. 
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program. </param>
    /// <param name="port">     The Port number at host where the remote program can be reached. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC
    ///                         calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocols.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocols.OncRpcTcp"/>. </param>
    public DeviceCoreClient( IPAddress host, int port, OncRpcProtocols protocol, int timeout ) : this( host, Vxi11ProgramConstants.DeviceCoreProgram,
                                                                                                       Vxi11ProgramConstants.DeviceCoreVersion, port,
                                                                                                       protocol, timeout )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceCoreClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceCoreProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program. </param>
    /// <param name="program">  The Remote program number. </param>
    /// <param name="version">  The Remote program version number. </param>
    /// <param name="port">     The Port number at host where the remote program can be reached. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC
    ///                         calls. </param>
    /// <param name="timeout">  The transmit timeout for <see cref="OncRpcProtocols.OncRpcUdp"/>
    ///                         or the connection timeout for <see cref="OncRpcProtocols.OncRpcTcp"/>. </param>
    public DeviceCoreClient( IPAddress host, int program, int version, int port, OncRpcProtocols protocol,
                                                                       int timeout ) : base( host, program, version, port, protocol, timeout )
    {
    }

    #endregion

    #region " defaults "

    /// <summary>   Gets or sets the default encoding. </summary>
    /// <remarks>
    /// The default encoding for VXI-11 is <see cref="Encoding.ASCII"/>, which is a subset of <see cref="Encoding.UTF8"/>
    /// </remarks>
    /// <value> The default encoding. </value>
    public static Encoding EncodingDefault { get; set; } = Encoding.UTF8;

    #endregion

    #region " members "

    #endregion

    #region " remote procedure calls "

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.CreateLinkProcedure"/>; Opens a link to a
    /// device.
    /// </summary>
    /// <remarks>   Renamed from <c>create_link_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="clientId">                 Identifier for the client. </param>
    /// <param name="lockDevice">               True to lock, false to unlock the device. </param>
    /// <param name="lockTimeout">              The lock timeout. </param>
    /// <param name="interfaceDeviceString">    The interface device string. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public CreateLinkResp CreateLink( int clientId, bool lockDevice, int lockTimeout, string interfaceDeviceString )
    {
        CreateLinkParms request = new() {
            ClientId = clientId,
            LockDevice = lockDevice,
            LockTimeout = lockTimeout,
            Device = interfaceDeviceString
        };
        return this.CreateLink( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.CreateLinkProcedure"/>; Opens a link
    /// to a device.
    /// </summary>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <remarks> Renamed from <c>create_link_1</c> </remarks>
    /// <param name="request"> The request of type <see cref="Codecs.CreateLinkParms"/> to
    ///                        send using the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public CreateLinkResp CreateLink( CreateLinkParms request )
    {
        CreateLinkResp reply = new();
        this.Client?.Call( ( int ) Vxi11Message.CreateLinkProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, reply );
        return reply;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceWriteProcedure"/>;
    /// Device receives a message.
    /// </summary>
    /// <remarks>   Renamed from <c>device_write_1</c> </remarks>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <param name="lockTimeout">  The lock timeout, which determines how long a network instrument
    ///                             server will wait for a lock to be released. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="data">         The data to send. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceWriteResp"/>.
    /// </returns>
    public DeviceWriteResp DeviceWrite( DeviceLink link, int ioTimeout, int lockTimeout, DeviceFlags flags, byte[] data )
    {
        DeviceWriteParms request = new() {
            Link = link,
            IOTimeout = ioTimeout,
            LockTimeout = lockTimeout,
            Flags = flags
        };
        request.SetData( data );
        return DeviceWrite( request );
    }

    /// <summary>  
    /// Calls remote procedure <see cref="Vxi11Message.DeviceWriteProcedure"/>; 
    /// Device receives a message. </summary>
    /// <remarks> Renamed from <c>device_write_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request"> The request of type <see cref="Codecs.DeviceWriteParms"/> to send using the remote procedure call. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceWriteResp"/>. </returns>
    public DeviceWriteResp DeviceWrite( DeviceWriteParms request )
    {
        DeviceWriteResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceWriteProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceReadProcedure"/>;
    /// Device returns a result.
    /// </summary>
    /// <remarks>   Renamed from <c>device_read_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="requestSize">  Size of the request in number of bytes. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <param name="lockTimeout">  The lock timeout. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="termChar">     The termination character; valid if flags <see cref="DeviceOperationFlags.TerminationCharacterSet"/>
    ///                             is set. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceReadResp"/>.
    /// </returns>
    public DeviceReadResp DeviceRead( DeviceLink link, int requestSize, int ioTimeout, int lockTimeout, DeviceFlags flags, byte termChar )
    {
        DeviceReadParms request = new() {
            Link = link,
            RequestSize = requestSize,
            IOTimeout = ioTimeout,
            LockTimeout = lockTimeout,
            Flags = flags,
            TermChar = termChar
        };
        return this.DeviceRead( request );
    }

    /// <summary> Calls remote procedure <see cref="Vxi11Message.DeviceReadProcedure"/>;
    /// Device returns a result. </summary>
    /// <remarks> Renamed from <c>device_read_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request"> The request of type <see cref="Codecs.DeviceReadParms"/> to send to the remote procedure call. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceReadResp"/>. </returns>
    public DeviceReadResp DeviceRead( DeviceReadParms request )
    {
        DeviceReadResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceReadProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceReadStbProcedure"/>, the device is to
    /// return its status byte encapsulated in the <see cref="DeviceReadStbResp"/> codec.
    /// </summary>
    /// <remarks>   Renamed from <c>device_readstb_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="lockTimeout">  The lock timeout, which determines how long a network instrument
    ///                             server will wait for a lock to be released. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceReadStbResp"/>.
    /// </returns>
    public DeviceReadStbResp DeviceReadStb( DeviceLink link, DeviceOperationFlags flags, int lockTimeout, int ioTimeout )
    {
        DeviceGenericParms request = new() {
            Link = link,
            Flags = new DeviceFlags( flags ),
            LockTimeout = lockTimeout,
            IOTimeout = ioTimeout
        };
        return DeviceReadStb( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceReadStbProcedure"/>, the device is to
    /// return its status byte encapsulated in the <see cref="DeviceReadStbResp"/> codec.
    /// </summary>
    /// <remarks>   Renamed from <c>device_readstb_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceGenericParms"/> to send
    ///                         with the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceReadStbResp"/>.
    /// </returns>
    public DeviceReadStbResp DeviceReadStb( DeviceGenericParms request )
    {
        DeviceReadStbResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceReadStbProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceTriggerProcedure"/>;
    /// Device executes a trigger.
    /// </summary>
    /// <remarks>   Renamed from <c>device_trigger_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="lockTimeout">  The lock timeout, which determines how long a network instrument
    ///                             server will wait for a lock to be released. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceTrigger( DeviceLink link, DeviceOperationFlags flags, int lockTimeout, int ioTimeout )
    {
        DeviceGenericParms request = new() {
            Link = link,
            Flags = new DeviceFlags( flags ),
            LockTimeout = lockTimeout,
            IOTimeout = ioTimeout
        };
        return this.DeviceTrigger( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceTriggerProcedure"/>;
    /// Device executes a trigger.
    /// </summary>
    /// <remarks>   Renamed from <c>device_trigger_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceGenericParms"/> to send
    ///                         to  the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceTrigger( DeviceGenericParms request )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceTriggerProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceClearProcedure"/>;
    /// Device clears itself.
    /// </summary>
    /// <remarks>   Renamed from <c>device_clear_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="lockTimeout">  The lock timeout, which determines how long a network instrument
    ///                             server will wait for a lock to be released. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceClear( DeviceLink link, DeviceOperationFlags flags, int lockTimeout, int ioTimeout )
    {
        DeviceGenericParms request = new() {
            Link = link,
            Flags = new DeviceFlags( flags ),
            LockTimeout = lockTimeout,
            IOTimeout = ioTimeout
        };
        return DeviceClear( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceClearProcedure"/>;
    /// Device clears itself.
    /// </summary>
    /// <remarks>   Renamed from <c>device_clear_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceGenericParms"/> to send
    ///                         to the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceClear( DeviceGenericParms request )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceClearProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceRemoteProcedure"/>;
    /// Device disables its front panel.
    /// </summary>
    /// <remarks>   Renamed from <c>device_remote_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="lockTimeout">  The lock timeout, which determines how long a network instrument
    ///                             server will wait for a lock to be released. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceRemote( DeviceLink link, DeviceOperationFlags flags, int lockTimeout, int ioTimeout )
    {
        DeviceGenericParms request = new() {
            Link = link,
            Flags = new DeviceFlags( flags ),
            LockTimeout = lockTimeout,
            IOTimeout = ioTimeout
        };
        return DeviceRemote( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceRemoteProcedure"/>;
    /// Device disables its front panel.
    /// </summary>
    /// <remarks>   Renamed from <c>device_remote_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceGenericParms"/> to send
    ///                         to the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceRemote( DeviceGenericParms request )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceRemoteProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceLocalProcedure"/>;
    /// Device enables its front panel.
    /// </summary>
    /// <remarks>   Renamed from <c>device_local_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="lockTimeout">  The lock timeout, which determines how long a network instrument
    ///                             server will wait for a lock to be released. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceLocal( DeviceLink link, DeviceOperationFlags flags, int lockTimeout, int ioTimeout )
    {
        DeviceGenericParms request = new() {
            Link = link,
            Flags = new DeviceFlags( flags ),
            LockTimeout = lockTimeout,
            IOTimeout = ioTimeout
        };
        return DeviceRemote( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceLocalProcedure"/>;
    /// Device enables its front panel.
    /// </summary>
    /// <remarks>   Renamed from <c>device_local_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceGenericParms"/> to send
    ///                         to the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    ///
    public DeviceError DeviceLocal( DeviceGenericParms request )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceLocalProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceLockProcedure"/>;
    /// Device is locked.
    /// </summary>
    /// <remarks>   Renamed from <c>device_lock_1</c> </remarks>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="lockTimeout">  The lock timeout, which determines how long a network instrument
    ///                             server will wait for a lock to be released. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceLock( DeviceLink link, DeviceOperationFlags flags, int lockTimeout )
    {
        DeviceLockParms request = new() {
            Link = link,
            Flags = new DeviceFlags( flags ),
            LockTimeout = lockTimeout
        };
        return this.DeviceLock( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceLockProcedure"/>;
    /// Device is locked.
    /// </summary>
    /// <remarks>   Renamed from <c>device_lock_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceLockParms"/> to send to
    ///                         the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceLock( DeviceLockParms request )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceLockProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceUnlockProcedure"/>;
    /// Device is unlocked.
    /// </summary>
    /// <remarks>   Renamed from <c>device_unlock_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="link"> The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                     call, which forms the request of this RPC call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceUnlock( DeviceLink link )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceUnlockProcedure, Vxi11ProgramConstants.DeviceCoreVersion, link, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceEnableSrqProcedure"/>;
    /// Device enables/disables sending of service requests.
    /// </summary>
    /// <remarks>   Renamed from <c>device_enable_srq_1</c> </remarks>
    /// <param name="link">     The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                         call. </param>
    /// <param name="enable">   True to enable, false to disable service request interrupts. </param>
    /// <param name="handle">   The handle. Host specific data for handling the service request. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceEnableSrq( DeviceLink link, bool enable, byte[] handle )
    {
        DeviceEnableSrqParms request = new() {
            Link = link,
            Enable = enable
        };
        request.SetHandle( handle );
        return this.DeviceEnableSrq( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceEnableSrqProcedure"/>;
    /// Device enables/disables sending of service requests.
    /// </summary>
    /// <remarks>   Renamed from <c>device_enable_srq_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceEnableSrqParms"/> to send
    ///                         to the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceEnableSrq( DeviceEnableSrqParms request )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceEnableSrqProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceDoCommandProcedure"/>;
    /// Device executes a command.
    /// </summary>
    /// <remarks>   Renamed from <c>device_docmd_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="lockTimeout">  The lock timeout. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <param name="cmd">          The command; which command to execute. </param>
    /// <param name="dataSize">     Size of the data. </param>
    /// <param name="dataIn">       The data in. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceDoCmdResp"/>.
    /// </returns>
    public DeviceDoCmdResp DeviceDoCmd( DeviceLink link, DeviceFlags flags, int lockTimeout, int ioTimeout,
                                        int cmd, int dataSize, byte[] dataIn )
    {
        return this.DeviceDoCmd( link, flags, lockTimeout, ioTimeout, cmd, true, dataSize, dataIn );
    }


    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceDoCommandProcedure"/>;
    /// Device executes a command.
    /// </summary>
    /// <remarks>   Renamed from <c>device_docmd_1</c> </remarks>
    /// <param name="link">         The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                             call. </param>
    /// <param name="flags">        The <see cref="IXdrCodec"/> specifying the <see cref="DeviceOperationFlags"/>
    ///                             options. </param>
    /// <param name="lockTimeout">  The lock timeout. </param>
    /// <param name="ioTimeout">    The i/o timeout, which determines how long a network instrument
    ///                             server allows an I/O operation to take. </param>
    /// <param name="cmd">          The command; which command to execute. </param>
    /// <param name="netWorkOrder"> True to net work order; the client's byte order. Network order is
    ///                             defined by the Internet Protocol Suite; set <see langword="true"/>
    ///                             for big endian. </param>
    /// <param name="dataSize">     Size of the data. </param>
    /// <param name="dataIn">       The data in. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceDoCmdResp"/>.
    /// </returns>
    public DeviceDoCmdResp DeviceDoCmd( DeviceLink link, DeviceFlags flags, int lockTimeout, int ioTimeout,
                                        int cmd, bool netWorkOrder, int dataSize, byte[] dataIn )
    {
        DeviceDoCmdParms request = new() {
            Link = link,
            Flags = flags,
            IOTimeout = ioTimeout,
            LockTimeout = lockTimeout,
            Cmd = cmd,
            NetworkOrder = netWorkOrder,
            DataSize = dataSize
        };
        request.SetDataIn( dataIn );
        return this.DeviceDoCmd( request );
    }


    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceDoCommandProcedure"/>;
    /// Device executes a command.
    /// </summary>
    /// <remarks>   Renamed from <c>device_docmd_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request"> The request of type <see cref="Codecs.DeviceDoCmdParms"/> to send to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceDoCmdResp"/>.
    /// </returns>
    public DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms request )
    {
        DeviceDoCmdResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceDoCommandProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DestroyLinkProcedure"/>;
    /// Closes a link to a device.
    /// </summary>
    /// <remarks>   Renamed from <c>destroy_link_1</c> </remarks>
    /// <param name="link"> The <see cref="DeviceLink"/> link received from the <see cref="Vxi11Message.CreateLinkProcedure"/>
    ///                     call, which serves as the request for the RPC call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DestroyLink( DeviceLink link )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DestroyLinkProcedure, Vxi11ProgramConstants.DeviceCoreVersion, link, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.CreateInterruptChannelProcedure"/>;
    /// Device creates interrupt channel.
    /// </summary>
    /// <remarks>   Renamed from <c>create_intr_chan_1</c> </remarks>
    /// <param name="hostAddress">      The host address. </param>
    /// <param name="hostPort">         The host port. </param>
    /// <param name="programFamily">    (Optional) The <see cref="DeviceAddrFamily"/> program family [TCP]. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError CreateIntrChan( int hostAddress, int hostPort, DeviceAddrFamily programFamily = DeviceAddrFamily.DeviceTcpAddressFamily )
    {
        DeviceRemoteFunc request = new() {
            HostAddr = hostAddress,
            HostPort = hostPort,
            ProgNum = Vxi11ProgramConstants.DeviceInterruptProgram,
            ProgVers = Vxi11ProgramConstants.DeviceInterruptVersion,
            ProgFamily = programFamily
        };
        return this.CreateIntrChan( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.CreateInterruptChannelProcedure"/>;
    /// Device creates interrupt channel.
    /// </summary>
    /// <remarks>   Renamed from <c>create_intr_chan_1</c> </remarks>
    /// <param name="hostAddress">      The host address. </param>
    /// <param name="hostPort">         The host port. </param>
    /// <param name="programNumber">    The program number; should be <see cref="Vxi11ProgramConstants.DeviceInterruptProgram"/>. </param>
    /// <param name="programVersion">   The program version; should be <see cref="Vxi11ProgramConstants.DeviceInterruptVersion"/> </param>
    /// <param name="programFamily">    The <see cref="DeviceAddrFamily"/> program family. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError CreateIntrChan( int hostAddress, int hostPort, int programNumber, int programVersion, DeviceAddrFamily programFamily  )
    {
        DeviceRemoteFunc request = new() {
            HostAddr = hostAddress,
            HostPort = hostPort,
            ProgNum = programNumber,
            ProgVers = programVersion,
            ProgFamily = programFamily
        };
        return this.CreateIntrChan( request );
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.CreateInterruptChannelProcedure"/>;
    /// Device creates interrupt channel.
    /// </summary>
    /// <remarks>   Renamed from <c>create_intr_chan_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <param name="request">  The request of type <see cref="Codecs.DeviceRemoteFunc"/> to send to
    ///                         the remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError CreateIntrChan( DeviceRemoteFunc request )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.CreateInterruptChannelProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DestroyInterruptChannelProcedure"/>;
    /// Device destroys interrupt channel.
    /// </summary>
    /// <remarks>   Renamed from <c>destroy_intr_chan_1</c> </remarks>
    /// <exception cref="OncRpcException">  Thrown when an ONC/RPC error condition occurs. </exception>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DestroyIntrChan()
    {
        VoidXdrCodec request = VoidXdrCodec.VoidXdrCodecInstance;
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DestroyInterruptChannelProcedure, Vxi11ProgramConstants.DeviceCoreVersion, request, result );
        return result;
    }

    #endregion

}
