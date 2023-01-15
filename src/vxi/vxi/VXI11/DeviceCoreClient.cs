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
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="host">     The Internet address of host where to contact the remote program.. </param>
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC calls. </param>
    public DeviceCoreClient( IPAddress host, OncRpcProtocols protocol ) : base( host, Vxi11ProgramConstants.DeviceCoreProgram,
                                                                                Vxi11ProgramConstants.DeviceCoreVersion, 0, protocol )
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
    public DeviceCoreClient( IPAddress host, int port, OncRpcProtocols protocol ) : base( host, Vxi11ProgramConstants.DeviceCoreProgram,
                                                                                          Vxi11ProgramConstants.DeviceCoreVersion, port, protocol )
    {
    }

    /// <summary>
    /// Constructs a <see cref="DeviceCoreClient"/> client stub proxy object from which the <see cref="Vxi11ProgramConstants.DeviceCoreProgram"/>
    /// remote program can be accessed.
    /// </summary>
    /// <exception cref="DeviceException">  Thrown when an VXI-11 error condition occurs. </exception>
    /// <param name="client">   The ONC/RPC client connection object implementing a particular protocol. </param>
    public DeviceCoreClient( OncRpcClientBase client ) : base( client )
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
    /// <param name="protocol"> The <see cref="OncRpcProtocols"/> protocol to be used for ONC/RPC
    ///                         calls. </param>
    public DeviceCoreClient( IPAddress host, int program, int version, OncRpcProtocols protocol ) : base( host, program, version, 0, protocol )
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
    public DeviceCoreClient( IPAddress host, int program, int version, int port, OncRpcProtocols protocol ) : base( host, program, version, port, protocol )
    {
    }

    #endregion

    #region " members "

    /// <summary>   Gets or sets the default encoding. </summary>
    /// <remarks>
    /// The default encoding for VXI-11 is <see cref="Encoding.ASCII"/>, which is a subset of <see cref="Encoding.UTF8"/>
    /// </remarks>
    /// <value> The default encoding. </value>
    public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

    #endregion

    #region " remote procedure calls "

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.CreateLinkProcedure"/>; Opens a link
    /// to a device.
    /// </summary>
    /// <remarks> Renamed from <c>create_link_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.CreateLinkParms"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public CreateLinkResp CreateLink( CreateLinkParms arg1 )
    {
        CreateLinkResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.CreateLinkProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>  
    /// Calls remote procedure <see cref="Vxi11Message.DeviceWriteProcedure"/>; 
    /// Device receives a message. </summary>
    /// <remarks> Renamed from <c>device_write_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceWriteParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceWriteResp"/>. </returns>
    public DeviceWriteResp DeviceWrite( DeviceWriteParms arg1 )
    {
        DeviceWriteResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceWriteProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary> Calls remote procedure <see cref="Vxi11Message.DeviceReadProcedure"/>;
    /// Device returns a result. </summary>
    /// <remarks> Renamed from <c>device_read_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceReadParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceReadResp"/>. </returns>
    public DeviceReadResp DeviceRead( DeviceReadParms arg1 )
    {
        DeviceReadResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceReadProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>  Calls remote procedure <see cref="Vxi11Message.DeviceReadStbProcedure"/>;
    /// Device returns its status byte. </summary>
    /// <remarks> Renamed from <c>device_readstb_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceReadStbResp"/>. </returns>
    public DeviceReadStbResp DeviceReadStb( DeviceGenericParms arg1 )
    {
        DeviceReadStbResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceReadStbProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceTriggerProcedure"/>;
    /// Device executes a trigger.
    /// </summary>
    /// <remarks>   Renamed from <c>device_trigger_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceTrigger( DeviceGenericParms arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceTriggerProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceClearProcedure"/>;
    /// Device clears itself.
    /// </summary>
    /// <remarks>   Renamed from <c>device_clear_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceClear( DeviceGenericParms arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceClearProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceRemoteProcedure"/>;
    /// Device disables its front panel.
    /// </summary>
    /// <remarks>   Renamed from <c>device_remote_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceRemote( DeviceGenericParms arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceRemoteProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceLocalProcedure"/>;
    /// Device enables its front panel.
    /// </summary>
    /// <remarks>   Renamed from <c>device_local_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceLocal( DeviceGenericParms arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceLocalProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceLockProcedure"/>;
    /// Device is locked.
    /// </summary>
    /// <remarks>   Renamed from <c>device_lock_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceLockParms"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceLock( DeviceLockParms arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceLockProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceUnlockProcedure"/>;
    /// Device is unlocked.
    /// </summary>
    /// <remarks>   Renamed from <c>device_unlock_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceLink"/>) to the remote
    ///                     procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceUnlock( DeviceLink arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceUnlockProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceEnableSrqProcedure"/>;
    /// Device enables/disables sending of service requests.
    /// </summary>
    /// <remarks>   Renamed from <c>device_enable_srq_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceEnableSrqParms"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DeviceEnableSrq( DeviceEnableSrqParms arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceEnableSrqProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DeviceDoCommandProcedure"/>;
    /// Device executes a command.
    /// </summary>
    /// <remarks>   Renamed from <c>device_docmd_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceDoCmdParms"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceDoCmdResp"/>.
    /// </returns>
    public DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms arg1 )
    {
        DeviceDoCmdResp result = new();
        this.Client?.Call( ( int ) Vxi11Message.DeviceDoCommandProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DestroyLinkProcedure"/>;
    /// Closes a link to a device.
    /// </summary>
    /// <remarks>   Renamed from <c>destroy_link_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceLink"/>) to the remote
    ///                     procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DestroyLink( DeviceLink arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DestroyLinkProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.CreateInterruptChannelProcedure"/>;
    /// Device creates interrupt channel.
    /// </summary>
    /// <remarks>   Renamed from <c>create_intr_chan_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceRemoteFunc"/>) to the
    ///                     remote procedure call. </param>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError CreateIntrChan( DeviceRemoteFunc arg1 )
    {
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.CreateInterruptChannelProcedure, Vxi11ProgramConstants.DeviceCoreVersion, arg1, result );
        return result;
    }

    /// <summary>
    /// Calls remote procedure <see cref="Vxi11Message.DestroyInterruptChannelProcedure"/>;
    /// Device destroys interrupt channel.
    /// </summary>
    /// <remarks>   Renamed from <c>destroy_intr_chan_1</c> </remarks>
    /// <returns>
    /// A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>.
    /// </returns>
    public DeviceError DestroyIntrChan()
    {
        VoidXdrCodec args = VoidXdrCodec.VoidXdrCodecInstance;
        DeviceError result = new();
        this.Client?.Call( ( int ) Vxi11Message.DestroyInterruptChannelProcedure, Vxi11ProgramConstants.DeviceCoreVersion, args, result );
        return result;
    }

    #endregion

}
