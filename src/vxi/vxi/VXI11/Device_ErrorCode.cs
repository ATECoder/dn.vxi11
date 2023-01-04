
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_ErrorCode : IXdrCodec
    {
        public int value;

        public Device_ErrorCode()
        {
        }

        public Device_ErrorCode(int value)
        {
            this.value = value;
        }

        public Device_ErrorCode(XdrDecodingStreamBase xdr)
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
