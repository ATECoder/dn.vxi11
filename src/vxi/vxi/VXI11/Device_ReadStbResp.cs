
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_ReadStbResp : IXdrCodec
    {
        public Device_ErrorCode error;
        public byte stb;

        public Device_ReadStbResp()
        {
        }

        public Device_ReadStbResp(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            error.Encode(xdr);
            xdr.EncodeByte(stb);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            error = new Device_ErrorCode(xdr);
            stb = xdr.DecodeByte();
        }

    }
}
