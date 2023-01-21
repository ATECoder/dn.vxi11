using cc.isr.ONC.RPC.Server;
using cc.isr.VXI11.Codecs;

using System.Net;

namespace cc.isr.VXI11;

/// <summary>
/// The abstract VXI-11 <see cref="Vxi11ProgramConstants.DeviceCoreProgram"/> <see cref="DeviceCoreServerStubBase"/> class is the base class upon which
/// to build VXI-11 <see cref="Vxi11ProgramConstants.DeviceCoreProgram"/> TCP servers.
/// </summary>
public abstract class DeviceCoreServerStubBase : OncRpcServerStubBase, IOncRpcDispatchable
{

    /// <summary>   Default constructor. </summary>
    public DeviceCoreServerStubBase() : this( 0 )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="port"> The port. </param>
    public DeviceCoreServerStubBase( int port ) : this( IPAddress.Any, port )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <param name="bindAddr"> The bind address. </param>
    /// <param name="port">     The port. </param>
    public DeviceCoreServerStubBase( IPAddress bindAddr, int port )
    {
        OncRpcProgramInfo[] registeredPrograms = new OncRpcProgramInfo[] {
            new OncRpcProgramInfo(Vxi11ProgramConstants.DeviceCoreProgram, Vxi11ProgramConstants.DeviceCoreVersion),
        };
        this.SetRegisteredPrograms( registeredPrograms );

        OncRpcTransportBase[] transports = new OncRpcTransportBase[] {
            // new OncRpcUdpServerTransport(this, bindAddr, port+2, info, 32768),
            new OncRpcTcpTransport(this, bindAddr, port, registeredPrograms, OncRpcTransportBase.BufferSizeDefault)
        };
        this.SetTransports( transports );

        this.CharacterEncoding = DeviceCoreClient.EncodingDefault;
    }

    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <remarks>
    /// This interface has some fairly deep semantics, so please read the description above for how
    /// to use it properly. For background information about fairly deep semantics, please also refer
    /// to <i>Gigzales</i>, <i>J</i>.: Semantics considered harmful. Addison-Reilly, 1992, ISBN 0-542-
    /// 10815-X. <para>
    ///  
    /// See the introduction to this class for examples of how to use this interface properly.</para>
    /// </remarks>
    /// <param name="call">         <see cref="T:cc.isr.ONC.RPC.Server.OncRpcCallInformation" />
    ///                             about the call to handle, like the caller's Internet address, the ONC/RPC
    ///                             call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    public void DispatchOncRpcCall( OncRpcCallHandler call, int program, int version, int procedure )
    {
        if ( version == Vxi11ProgramConstants.DeviceCoreVersion )
            switch ( ( Vxi11Message ) procedure )
            {
                case Vxi11Message.CreateLinkProcedure:
                    {
                        CreateLinkParms request = new();
                        call.RetrieveCall( request );
                        CreateLinkResp result = this.CreateLink( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceWriteProcedure:
                    {
                        DeviceWriteParms request = new();
                        call.RetrieveCall( request );
                        DeviceWriteResp result = this.DeviceWrite( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceReadProcedure:
                    {
                        DeviceReadParms request = new();
                        call.RetrieveCall( request );
                        DeviceReadResp result = this.DeviceRead( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceReadStbProcedure:
                    {
                        DeviceGenericParms request = new();
                        call.RetrieveCall( request );
                        DeviceReadStbResp result = this.DeviceReadStb( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceTriggerProcedure:
                    {
                        DeviceGenericParms request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.DeviceTrigger( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceClearProcedure:
                    {
                        DeviceGenericParms request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.DeviceClear( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceRemoteProcedure:
                    {
                        DeviceGenericParms request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.DeviceRemote( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceLocalProcedure:
                    {
                        DeviceGenericParms request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.DeviceLocal( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceLockProcedure:
                    {
                        DeviceLockParms request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.DeviceLock( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceUnlockProcedure:
                    {
                        DeviceLink request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.DeviceUnlock( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceEnableSrqProcedure:
                    {
                        DeviceEnableSrqParms request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.DeviceEnableSrq( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DeviceDoCommandProcedure:
                    {
                        DeviceDoCmdParms request = new();
                        call.RetrieveCall( request );
                        DeviceDoCmdResp result = this.DeviceDoCmd( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DestroyLinkProcedure:
                    {
                        DeviceLink request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.DestroyLink( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.CreateInterruptChannelProcedure:
                    {
                        DeviceRemoteFunc request = new();
                        call.RetrieveCall( request );
                        DeviceError result = this.CreateIntrChan( request );
                        call.Reply( result );
                        break;
                    }
                case Vxi11Message.DestroyInterruptChannelProcedure:
                    {
                        call.RetrieveCall( VoidXdrCodec.VoidXdrCodecInstance );
                        DeviceError result = this.DestroyIntrChan();
                        call.Reply( result );
                        break;
                    }
                default:
                    call.ReplyProcedureNotAvailable();
                    break;
            }
        else
            call.ReplyProgramNotAvailable();
    }

    /// <summary>  Opens a link to a device. </summary>
    /// <remarks> Renamed from <c>create_link_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.CreateLinkParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract CreateLinkResp CreateLink( CreateLinkParms arg1 );

    /// <summary>  Device receives a message. </summary>
    /// <remarks> Renamed from <c>device_write_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceWriteParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceWriteResp"/>. </returns>
    public abstract DeviceWriteResp DeviceWrite( DeviceWriteParms arg1 );

    /// <summary>  Device returns a result. </summary>
    /// <remarks> Renamed from <c>device_read_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceReadParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceReadResp"/>. </returns>
    public abstract DeviceReadResp DeviceRead( DeviceReadParms arg1 );

    /// <summary>  Device returns its status byte. </summary>
    /// <remarks> Renamed from <c>device_readstb_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceReadStbResp"/>. </returns>
    public abstract DeviceReadStbResp DeviceReadStb( DeviceGenericParms arg1 );

    /// <summary>  Device executes a trigger. </summary>
    /// <remarks> Renamed from <c>device_trigger_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DeviceTrigger( DeviceGenericParms arg1 );

    /// <summary>  Device clears itself. </summary>
    /// <remarks> Renamed from <c>device_clear_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DeviceClear( DeviceGenericParms arg1 );

    /// <summary>  Device disables its front panel. </summary>
    /// <remarks> Renamed from <c>device_remote_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DeviceRemote( DeviceGenericParms arg1 );

    /// <summary>  Device enables its front panel. </summary>
    /// <remarks> Renamed from <c>device_local_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceGenericParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DeviceLocal( DeviceGenericParms arg1 );

    /// <summary>  Device is locked. </summary>
    /// <remarks> Renamed from <c>device_lock_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceLockParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DeviceLock( DeviceLockParms arg1 );

    /// <summary>  Device is unlocked. </summary>
    /// <remarks> Renamed from <c>device_unlock_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceLink"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DeviceUnlock( DeviceLink arg1 );

    /// <summary>  Device enables/disables sending of service requests. </summary>
    /// <remarks> Renamed from <c>device_enable_srq_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceEnableSrqParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DeviceEnableSrq( DeviceEnableSrqParms arg1 );

    /// <summary>  Device executes a command. </summary>
    /// <remarks> Renamed from <c>device_docmd_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceDoCmdParms"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceDoCmdResp"/>. </returns>
    public abstract DeviceDoCmdResp DeviceDoCmd( DeviceDoCmdParms arg1 );

    /// <summary>  Closes a link to a device. </summary>
    /// <remarks> Renamed from <c>destroy_link_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceLink"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DestroyLink( DeviceLink arg1 );

    /// <summary>  Device creates interrupt channel. </summary>
    /// <remarks> Renamed from <c>create_intr_chan_1</c> </remarks>
    /// <param name="arg1"> The parameter (of type <see cref="Codecs.DeviceRemoteFunc"/>) to the remote procedure call.. </param>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError CreateIntrChan( DeviceRemoteFunc arg1 );

    /// <summary>  Device destroys interrupt channel. </summary>
    /// <remarks> Renamed from <c>destroy_intr_chan_1</c> </remarks>
    /// <returns>   A Result from remote procedure call of type <see cref="Codecs.DeviceError"/>. </returns>
    public abstract DeviceError DestroyIntrChan();

}
