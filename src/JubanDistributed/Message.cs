using ProtoBuf;

namespace Jubanlabs.JubanDistributed
{
    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public string type;
        [ProtoMember(2)]
        public string method;
        [ProtoMember(3)]
        public string[] types;
        [ProtoMember(4)]
        public byte[][] values;

        [ProtoMember(5)]
        public bool delayed;

        [ProtoMember(6)]
        public string id;
    }
}