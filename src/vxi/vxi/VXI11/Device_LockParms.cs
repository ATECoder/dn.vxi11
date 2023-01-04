

namespace VXI11
{
    public class Device_LockParms : IXdrCodec
    {
        public Device_Link lid;
        public Device_Flags flags;
        public int lock_timeout;

        public Device_LockParms()
        {
        }

        public Device_LockParms(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            lid.Encode(xdr);
            flags.Encode(xdr);
            xdr.EncodeInt(lock_timeout);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            lid = new Device_Link(xdr);
            flags = new Device_Flags(xdr);
            lock_timeout = xdr.DecodeInt();
        }

    }
}
