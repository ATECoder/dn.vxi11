
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_WriteResp : IXdrCodec
    {
        public Device_ErrorCode error;
        public int size;

        public Device_WriteResp()
        {
        }

        public Device_WriteResp(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            error.Encode(xdr);
            xdr.EncodeInt(size);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            error = new Device_ErrorCode(xdr);
            size = xdr.DecodeInt();
        }

    }
}
