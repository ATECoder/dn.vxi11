
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_DocmdParms : IXdrCodec
    {
        public Device_Link lid;
        public Device_Flags flags;
        public int io_timeout;
        public int lock_timeout;
        public int cmd;
        public bool network_order;
        public int datasize;
        public byte[] data_in;

        public Device_DocmdParms()
        {
        }

        public Device_DocmdParms(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            lid.Encode(xdr);
            flags.Encode(xdr);
            xdr.EncodeInt(io_timeout);
            xdr.EncodeInt(lock_timeout);
            xdr.EncodeInt(cmd);
            xdr.EcodeBoolean(network_order);
            xdr.EncodeInt(datasize);
            xdr.EncodeDynamicOpaque(data_in);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            lid = new Device_Link(xdr);
            flags = new Device_Flags(xdr);
            io_timeout = xdr.DecodeInt();
            lock_timeout = xdr.DecodeInt();
            cmd = xdr.DecodeInt();
            network_order = xdr.DecodeBoolean();
            datasize = xdr.DecodeInt();
            data_in = xdr.DecodeDynamicOpaque();
        }

    }
}
