using System;
using System.IO;
using Jubanlabs.JubanShared.Common;
using ProtoBuf;

namespace Jubanlabs.JubanDistributed
{
    public class Utils
    {
        public static byte[] Serialize(object data)
        {
            byte[] body=null;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, data);
                body =  CommonHelper.CompressSmallString(stream.ToArray()); //GetBuffer was giving me a Protobuf.ProtoException of "Invalid field in source data: 0" when deserializing
            }
            return body;
        }


        public static object Deserialize(byte[] data,Type type)
        {
             return Serializer.Deserialize(type,new MemoryStream( CommonHelper.UncompressSmallStringToByteArray(data)));
               
        }

        public static T Deserialize<T>(byte[] data)
        {
            return Serializer.Deserialize<T>(new MemoryStream(CommonHelper.UncompressSmallStringToByteArray(data)));

        }
    }
}