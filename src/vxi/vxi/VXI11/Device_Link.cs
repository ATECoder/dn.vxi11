
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_Link : IXdrCodec
    {
        public int value;

        public Device_Link()
        {
        }

        public Device_Link(int value)
        {
            this.value = value;
        }

        public Device_Link(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            xdr.EncodeInt(value);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            value = xdr.DecodeInt();
        }
    }
}
