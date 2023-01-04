
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Create_LinkParms : IXdrCodec
    {
        public int clientId;
        public bool lockDevice;
        public int lock_timeout;
        public String device;

        public Create_LinkParms()
        {
        }

        public Create_LinkParms(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            xdr.EncodeInt(clientId);
            xdr.EcodeBoolean(lockDevice);
            xdr.EncodeInt(lock_timeout);
            xdr.EncodeString(device);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            clientId = xdr.DecodeInt();
            lockDevice = xdr.DecodeBoolean();
            lock_timeout = xdr.DecodeInt();
            device = xdr.DecodeString();
        }

    }
}
