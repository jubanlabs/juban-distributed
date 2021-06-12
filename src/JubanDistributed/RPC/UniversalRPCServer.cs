namespace Jubanlabs.JubanDistributed.RPC {
    public class UniversalRPCServer {
        public static void Start () {
            var interpreter = new UniversalRPCInterpreter ();
            EventBasedRPCServiceHost worker = new EventBasedRPCServiceHost (interpreter, "universalrpc");
        }

    }
}