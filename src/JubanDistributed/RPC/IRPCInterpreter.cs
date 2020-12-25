using System;
using System.Collections.Generic;
using System.Text;

namespace Jubanlabs.JubanDistributed.RPC
{
    public interface IRPCInterpreter
    {
        byte[] Process(byte[] msg);
    }

}
