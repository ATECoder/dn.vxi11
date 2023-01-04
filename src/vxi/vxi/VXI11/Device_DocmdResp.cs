
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_DocmdResp : IXdrCodec
    {
        public Device_ErrorCode error;
        public byte[] data_out;

        public Device_DocmdResp()
        {
        }

        public Device_DocmdResp(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            error.Encode(xdr);
            xdr.EncodeDynamicOpaque(data_out);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            error = new Device_ErrorCode(xdr);
            data_out = xdr.DecodeDynamicOpaque();
        }

    }
}
