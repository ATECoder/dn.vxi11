
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_EnableSrqParms : IXdrCodec
    {
        public Device_Link lid;
        public bool enable;
        public byte[] handle;

        public Device_EnableSrqParms()
        {
        }

        public Device_EnableSrqParms(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            lid.Encode(xdr);
            xdr.EcodeBoolean(enable);
            xdr.EncodeDynamicOpaque(handle);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            lid = new Device_Link(xdr);
            enable = xdr.DecodeBoolean();
            handle = xdr.DecodeDynamicOpaque();
        }

    }
}
