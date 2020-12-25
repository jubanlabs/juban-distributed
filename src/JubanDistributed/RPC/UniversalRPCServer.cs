using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Jubanlabs.JubanDistributed.RPC {
    public class UniversalRPCServer {
        public static void Start () {
            var interpreter = new UniversalRPCInterpreter ();
            EventBasedRPCServiceHost worker = new EventBasedRPCServiceHost (interpreter, "universalrpc");
        }

    }
}