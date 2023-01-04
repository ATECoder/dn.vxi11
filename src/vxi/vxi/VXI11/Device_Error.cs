
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_Error : IXdrCodec
    {
        public Device_ErrorCode error;

        public Device_Error()
        {
        }

        public Device_Error(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            error.Encode(xdr);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            error = new Device_ErrorCode(xdr);
        }
    }
}
