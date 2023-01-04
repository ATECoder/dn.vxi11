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
    /// The abstract VXI-11 <see cref="vxi11.DEVICE_ASYNC"/> <see cref="DeviceAsyncServerStubBase"/> class is the base class upon which
    /// to build VXI-11 <see cref="vxi11.DEVICE_ASYNC"/> TCP and UDP servers.
    /// </summary>
    public abstract class DeviceAsyncServerStubBase : OncRpcServerStubBase, IOncRpcDispatchable
    {

        public DeviceAsyncServerStubBase():this(0)
        {
        }

        public DeviceAsyncServerStubBase(int port):this(null, port)
        {
        }

        public DeviceAsyncServerStubBase(IPAddress bindAddr, int port)
        {
            OncRpcServerTransportRegistrationInfo[] info = new OncRpcServerTransportRegistrationInfo[] {
                new OncRpcServerTransportRegistrationInfo(vxi11.DEVICE_ASYNC, vxi11.DEVICE_ASYNC_VERSION),
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
                    case 1:
                        {
                            Device_Link args = new Device_Link();
                            call.RetrieveCall(args);
                            Device_Error result = device_abort_1(args);
                            call.Reply(result);
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

        public abstract Device_Error device_abort_1(Device_Link arg1);

    }
}
