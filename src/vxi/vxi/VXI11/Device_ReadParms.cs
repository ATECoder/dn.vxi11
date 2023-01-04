

namespace VXI11
{
    public class Device_ReadParms : IXdrCodec
    {
        public Device_Link lid;
        public int requestSize;
        public int io_timeout;
        public int lock_timeout;
        public Device_Flags flags;
        public byte termChar;

        public Device_ReadParms()
        {
        }

        public Device_ReadParms(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            lid.Encode(xdr);
            xdr.EncodeInt(requestSize);
            xdr.EncodeInt(io_timeout);
            xdr.EncodeInt(lock_timeout);
            flags.Encode(xdr);
            xdr.EncodeByte(termChar);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            lid = new Device_Link(xdr);
            requestSize = xdr.DecodeInt();
            io_timeout = xdr.DecodeInt();
            lock_timeout = xdr.DecodeInt();
            flags = new Device_Flags(xdr);
            termChar = xdr.DecodeByte();
        }

    }
}
