using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model
{
     [Serializable]
    class PathDetailsData
    {
        public int pathPkId { get; set; }
        public int sequencePkId { get; set; }
        public int queueId { get; set; }
        public string machine { get; set; }
        public int aisle { get; set; }
        public int floor { get; set; }
        public int row { get; set; }
        public string command { get; set; }
        public int cmd_val_in_number { get; set; }
        public string channel { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public int beforeCMTask { get; set; }
        public string unblockMachine { get; set; }
        public int callBackRehandle { get; set; }
        public bool parallelMove { get; set; }
        public string Description { get; set; }
        public bool dynamicCall { get; set; }
        public bool done { get; set; }
        public string machineName { get; set; }
        public bool callUpadate { get; set; }
        public string interactMachineCode { get; set; }
        public bool confirmTrigger { get; set; }

        public int requestType { get; set; }

    }
}
