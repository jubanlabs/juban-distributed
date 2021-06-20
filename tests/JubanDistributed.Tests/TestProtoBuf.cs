
using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Jubanlabs.JubanDistributed;
using Jubanlabs.JubanDistributed.Tests;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

namespace Jubanlabs.JubanDistributed.Common.Test
{
    [TestClass]
    public class TestProtoBuf :JubanTestBase{

private static ILogger<TestProtoBuf> Logger =  JubanLogger.GetLogger<TestProtoBuf>();
        public void testPB()
        {
            var sub =new DummySubClass();
            sub.Prop1=123;
            string type="Common.Test.DummyClass";
            string method="DummyCall";
            string[] types=new string[]{"Common.Test.DummySubClass","System.Int32"};
            byte[][] values = new byte[2][];
            values[0]=Utils.Serialize(sub);
            values[1]=Utils.Serialize(4);

            var message=new Message();
            message.type=type;
            message.method=method;
            message.types=types;
            message.values=values;

            var mesg=Utils.Serialize(message);
            Logger.LogTrace(mesg.Length.ToString());
            var mesgObj = (Message)Utils.Deserialize(mesg,typeof(Message));

            Logger.LogTrace(mesgObj.type);
            object[] valuesArray=new object[2];
            valuesArray[0]=Utils.Deserialize(mesgObj.values[0],Type.GetType(mesgObj.types[0]));
            Logger.LogTrace(((DummySubClass)valuesArray[0]).Prop1.ToString());

            valuesArray[1]=Utils.Deserialize(mesgObj.values[1],Type.GetType(mesgObj.types[1]));
            Logger.LogTrace(valuesArray[1].ToString());

             var dummyClass= Type.GetType(mesgObj.type);
            Logger.LogTrace(dummyClass.ToString());

            var typesArray=new Type[2];
            typesArray[0]=Type.GetType(mesgObj.types[0]);

            typesArray[1]=Type.GetType(mesgObj.types[1]);
           var methodInfo= dummyClass.GetMethod(mesgObj.method,typesArray);



           var obj =Activator.CreateInstance(dummyClass);
            var invokerSub=FastInvoke.GetMethodInvoker(methodInfo);
          Logger.LogTrace("1");
          var ooo = invokerSub(obj,valuesArray);
          Logger.LogTrace((ooo==null).ToString());
             Logger.LogTrace(ooo.ToString());
             Logger.LogTrace("2");
        }

    }

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
    }

}