using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.OPCOperations
{
    interface OpcThreadService
    {
        bool Request(string req, object val);
        void Stop();
        

    }
}
