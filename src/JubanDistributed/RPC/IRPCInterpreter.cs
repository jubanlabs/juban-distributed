namespace Jubanlabs.JubanDistributed.RPC
{
    public interface IRPCInterpreter
    {
        byte[] Process(byte[] msg);
    }

}
