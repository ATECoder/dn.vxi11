using cc.isr.ONC.RPC.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{

    /// <summary>
    /// The abstract VXI-11 <see cref="vxi11.DEVICE_INTR"/> <see cref="DeviceIntrServerStubBase"/> class is the base class upon which
    /// to build VXI-11 <see cref="vxi11.DEVICE_INTR"/> TCP and UDP servers.
    /// </summary>
    public abstract class DeviceIntrServerStubBase : OncRpcServerStubBase, IOncRpcDispatchable
    {

        public DeviceIntrServerStubBase():this(0)
        {
        }

        public DeviceIntrServerStubBase(int port):this(null, port)
        {
        }

        public DeviceIntrServerStubBase(IPAddress bindAddr, int port)
        {
            OncRpcServerTransportRegistrationInfo[] info = new OncRpcServerTransportRegistrationInfo[] {
                new OncRpcServerTransportRegistrationInfo(vxi11.DEVICE_INTR, vxi11.DEVICE_INTR_VERSION),
            };
            this.SetTransportRegistrationInfo(info);

            OncRpcServerTransportBase[]  transports = new OncRpcServerTransportBase[] {
                new OncRpcUdpServerTransport(this, bindAddr, port, info, OncRpcServerTransportBase.DefaultBufferSize),
                new OncRpcTcpServerTransport(this, bindAddr, port, info, OncRpcServerTransportBase.DefaultBufferSize)
            };
            this.SetTransports(transports);
        }

        public void DispatchOncRpcCall(OncRpcCallInformation call, int program, int version, int procedure)
        {
            if (version == 1)
            {
                switch (procedure)
                {
                    case 30:
                        {
                            Device_SrqParms args = new Device_SrqParms();
                            call.RetrieveCall(args);
                            device_intr_srq_1(args);
                            call.Reply(VoidXdrCodec.VoidXdrCodecInstance);
                            break;
                        }
                    default:
                        call.ReplyProcedureNotAvailable();
                        break;
                }
            }
            else
            {
                call.ReplyProgramNotAvailable();
            }
        }

        public abstract void device_intr_srq_1(Device_SrqParms arg1);

    }
}
