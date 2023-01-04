
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Create_LinkResp : IXdrCodec
    {
        public Device_ErrorCode error;
        public Device_Link lid;
        public short abortPort;
        public int maxRecvSize;

        public Create_LinkResp()
        {
        }

        public Create_LinkResp(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            error.Encode(xdr);
            lid.Encode(xdr);
            xdr.EncodeShort(abortPort);
            xdr.EncodeInt(maxRecvSize);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            error = new Device_ErrorCode(xdr);
            lid = new Device_Link(xdr);
            abortPort = xdr.DecodeShort();
            maxRecvSize = xdr.DecodeInt();
        }
    }
}
