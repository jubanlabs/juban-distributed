using ProtoBuf;

namespace Jubanlabs.JubanDistributed
{
    [ProtoContract]
    public class ReturnMessage
    {
        [ProtoMember(1)]
        public string type;
        [ProtoMember(2)]
        public byte[] value;
    }
}