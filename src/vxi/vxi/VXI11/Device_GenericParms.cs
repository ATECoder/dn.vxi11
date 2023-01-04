
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_GenericParms : IXdrCodec
    {
        public Device_Link lid;
        public Device_Flags flags;
        public int lock_timeout;
        public int io_timeout;

        public Device_GenericParms()
        {
        }

        public Device_GenericParms(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            lid.Encode(xdr);
            flags.Encode(xdr);
            xdr.EncodeInt(lock_timeout);
            xdr.EncodeInt(io_timeout);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            lid = new Device_Link(xdr);
            flags = new Device_Flags(xdr);
            lock_timeout = xdr.DecodeInt();
            io_timeout = xdr.DecodeInt();
        }

    }
}
