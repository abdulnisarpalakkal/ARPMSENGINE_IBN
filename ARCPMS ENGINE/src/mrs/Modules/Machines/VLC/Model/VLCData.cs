using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model
{
    [Serializable]
    class VLCData
    {
        public int vlcPkId { get; set; }
        public string vlcName { get; set; }
        public int row { get; set; }
        public int aisle { get; set; }
        public int floor { get; set; }
        public string machineCode { get; set; }
        public string vlcDeckCode { get; set; }
        public string machineChannel { get; set; }
        public int isBlocked { get; set; }
        public int status { get; set; }
        public bool isAutoMode { get; set; }
        public int queueId { get; set; }
        public bool isDone { get; set; } 

        public int destFloor { get; set; }
        public string command { get; set; }
    }
}
