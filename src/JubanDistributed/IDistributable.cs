namespace Jubanlabs.JubanDistributed
{
    public interface IDistributable{

    }

    public interface IWorkerService :IDistributable
    {

    }

    public interface IRPCService :IDistributable
    {

    }

    public interface IKickoff 
    {
        void FreshStart();
        void Resume();
    }
}
