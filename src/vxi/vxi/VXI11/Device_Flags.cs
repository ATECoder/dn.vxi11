
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_Flags : IXdrCodec
    {

        public int value;

        public Device_Flags()
        {
        }

        public Device_Flags(int value)
        {
            this.value = value;
        }

        public Device_Flags(XdrDecodingStreamBase xdr)
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
