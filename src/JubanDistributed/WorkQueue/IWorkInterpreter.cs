using System;
using System.Collections.Generic;
using System.Text;

namespace Jubanlabs.JubanDistributed.WorkQueue
{
    public interface IWorkInterpreter
    {
        void Process(byte[] msg);
    }

}
