
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_ReadResp : IXdrCodec
    {
        public Device_ErrorCode error;
        public int reason;
        public byte[] data;

        public Device_ReadResp()
        {
        }

        public Device_ReadResp(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            error.Encode(xdr);
            xdr.EncodeInt(reason);
            xdr.EncodeDynamicOpaque(data);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            error = new Device_ErrorCode(xdr);
            reason = xdr.DecodeInt();
            data = xdr.DecodeDynamicOpaque();
        }

    }
}
