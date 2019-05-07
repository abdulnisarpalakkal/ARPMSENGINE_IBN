using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model
{
    [Serializable]
    class PVLData
    {
        public int pvlPkId { get; set; }
        public string pvlName { get; set; }
        public string machineChannel { get; set; }
        public string machineCode { get; set; }
        public int floor { get; set; }
        public int aisle { get; set; }
        public int row { get; set; }
        public bool isBlocked { get; set; }
        public int status { get; set; }
        public int autoMode { get; set; }
        public int startAisle { get; set; }
        public int endAisle { get; set; }

        public string command { get; set; }
        public int destFloor { get; set; }
        public bool isSwitchOff { get; set; }

        public bool isStore { get; set; }
        public int queueId { get; set; }
        public bool isDone { get; set; }
        



    }
}
