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
    public DeviceCoreServerStubBase( int port ) : this( null, port )
    {
    }

    /// <summary>   Constructor. </summary>
    /// <remarks>   2023-01-06. </remarks>
    /// <param name="bindAddr"> The bind address. </param>
    /// <param name="port">     The port. </param>
    public DeviceCoreServerStubBase( IPAddress bindAddr, int port )
    {
        OncRpcServerTransportRegistrationInfo[] info = new OncRpcServerTransportRegistrationInfo[] {
            new OncRpcServerTransportRegistrationInfo(Vxi11ProgramConstants.DeviceCoreProgram, Vxi11ProgramConstants.DeviceCoreVersion),
        };
        this.SetTransportRegistrationInfo( info );

        OncRpcServerTransportBase[] transports = new OncRpcServerTransportBase[] {
            // new OncRpcUdpServerTransport(this, bindAddr, port+2, info, 32768),
            new OncRpcTcpServerTransport(this, bindAddr, port, info, OncRpcServerTransportBase.DefaultBufferSize)
        };
        this.SetTransports( transports );

        this.CharacterEncoding = DeviceCoreClient.DefaultEncoding;
    }

    /// <summary>   Dispatch (handle) an ONC/RPC request from a client. </summary>
    /// <remarks>
    /// This interface has some fairly deep semantics, so please read the description above for how
    /// to use it properly. For background information about fairly deep semantics, please also refer
    /// to <i>Gigzales</i>, <i>J</i>.: Semantics considered harmful. Addison-Reilly, 1992, ISBN 0-542-
    /// 10815-X. <para>
    /// See the introduction to this class for examples of how to use this interface properly.</para>
    /// </remarks>
    /// <param name="call">         <see cref="T:cc.isr.ONC.RPC.Server.OncRpcCallInformation" />
    ///                             about the call to handle, like the caller's Internet address, the ONC/RPC
    ///                             call header, etc. </param>
    /// <param name="program">      Program number requested by client. </param>
    /// <param name="version">      Version number requested. </param>
    /// <param name="procedure">    Procedure number requested. </param>
    public void DispatchOncRpcCall( OncRpcCallInformation call, int program, int version, int procedure )
    {
        if ( version == Vxi11ProgramConstants.DeviceCoreVersion )
            switch ( ( Vxi11MessageConstants )  procedure )
            {
                case Vxi11MessageConstants.CreateLinkProcedure: 
                    {
                        CreateLinkParms args = new();
                        call.RetrieveCall( args );
                        CreateLinkResp result = this.CreateLink( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceWriteProcedure:
                    {
                        DeviceWriteParms args = new();
                        call.RetrieveCall( args );
                        DeviceWriteResp result = this.DeviceWrite( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceReadProcedure:
                    {
                        DeviceReadParms args = new();
                        call.RetrieveCall( args );
                        DeviceReadResp result = this.DeviceRead( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceReadStbProcedure:
                    {
                        DeviceGenericParms args = new();
                        call.RetrieveCall( args );
                        DeviceReadStbResp result = this.DeviceReadStb( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceTriggerProcedure:
                    {
                        DeviceGenericParms args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.DeviceTrigger( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceClearProcedure:
                    {
                        DeviceGenericParms args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.DeviceClear( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceRemoteProcedure:
                    {
                        DeviceGenericParms args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.DeviceRemote( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceLocalProcedure:
                    {
                        DeviceGenericParms args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.DeviceLocal( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceLockProcedure:
                    {
                        DeviceLockParms args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.DeviceLock( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceUnlockProcedure:
                    {
                        DeviceLink args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.DeviceUnlock( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceEnableSrqProcedure:
                    {
                        DeviceEnableSrqParms args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.DeviceEnableSrq( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DeviceDoCommandProcedure:
                    {
                        DeviceDoCmdParms args = new();
                        call.RetrieveCall( args );
                        DeviceDoCmdResp result = this.DeviceDoCmd( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DestroyLinkProcedure:
                    {
                        DeviceLink args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.DestroyLink( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.CreateInterruptChannelProcedure:
                    {
                        DeviceRemoteFunc args = new();
                        call.RetrieveCall( args );
                        DeviceError result = this.CreateIntrChan( args );
                        call.Reply( result );
                        break;
                    }
                case Vxi11MessageConstants.DestroyInterruptChannelProcedure:
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
