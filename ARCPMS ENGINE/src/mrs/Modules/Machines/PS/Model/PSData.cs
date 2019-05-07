using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Model
{
    [Serializable]
    class PSData
    {
        public int psPkId { get; set; }
        public string psName { get; set; }
        public string machineCode { get; set; }
        public string machineChannel { get; set; }
        public bool isBlocked { get; set; }
        public int actualMin { get; set; }
        public int actualMax { get; set; }
        public int actualHome { get; set; }
        public int dynamicMin { get; set; }
        public int dynamicMax { get; set; }
        public int dynamicHome { get; set; }
        public int status { get; set; }
        public bool isTrigger { get; set; }
        public bool isHomeMove { get; set; }
        public bool inDemoMode { get; set; }

        public int destFloor { get; set; }
        public int destAisle { get; set; }
        public int destRow { get; set; }
        public string command { get; set; }

        public bool isSwitchOff { get; set; }

    }
}
