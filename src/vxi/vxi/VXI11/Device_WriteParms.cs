
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_WriteParms : IXdrCodec
    {
        public Device_Link lid;
        public int io_timeout;
        public int lock_timeout;
        public Device_Flags flags;
        public byte[] data;

        public Device_WriteParms()
        {
        }

        public Device_WriteParms(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            lid.Encode(xdr);
            xdr.EncodeInt(io_timeout);
            xdr.EncodeInt(lock_timeout);
            flags.Encode(xdr);
            xdr.EncodeDynamicOpaque(data);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            lid = new Device_Link(xdr);
            io_timeout = xdr.DecodeInt();
            lock_timeout = xdr.DecodeInt();
            flags = new Device_Flags(xdr);
            data = xdr.DecodeDynamicOpaque();
        }

    }
}
