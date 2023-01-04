
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_SrqParms : IXdrCodec
    {
        public byte[] handle;

        public Device_SrqParms()
        {
        }

        public Device_SrqParms(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            xdr.EncodeDynamicOpaque(handle);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            handle = xdr.DecodeDynamicOpaque();
        }

    }
}
