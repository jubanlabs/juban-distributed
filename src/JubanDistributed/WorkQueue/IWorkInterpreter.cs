namespace Jubanlabs.JubanDistributed.WorkQueue
{
    public interface IWorkInterpreter
    {
        void Process(byte[] msg);
    }

}
