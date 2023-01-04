
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXI11
{
    public class Device_RemoteFunc : IXdrCodec
    {
        public int hostAddr;
        public int hostPort;
        public int progNum;
        public int progVers;
        public int progFamily;

        public Device_RemoteFunc()
        {
        }

        public Device_RemoteFunc(XdrDecodingStreamBase xdr)
        {
            Decode(xdr);
        }

        public void Encode(XdrEncodingStreamBase xdr)
        {
            xdr.EncodeInt(hostAddr);
            xdr.EncodeInt(hostPort);
            xdr.EncodeInt(progNum);
            xdr.EncodeInt(progVers);
            xdr.EncodeInt(progFamily);
        }

        public void Decode(XdrDecodingStreamBase xdr)
        {
            hostAddr = xdr.DecodeInt();
            hostPort = xdr.DecodeInt();
            progNum = xdr.DecodeInt();
            progVers = xdr.DecodeInt();
            progFamily = xdr.DecodeInt();
        }

    }
}
