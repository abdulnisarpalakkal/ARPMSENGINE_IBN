using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model
{
    [Serializable]
    class EESData
    {
        public int eesPkId  { get; set; }
        public string eesName  { get; set; }
        public int aisle  { get; set; }
        public int row { get; set; }
        public int startAisle { get; set; }
        public int endAisle { get; set; }
        public string machineCode { get; set; }
        public string machineChannel { get; set; }
        public int collapseStart { get; set; }
        public int collapseEnd { get; set; }
        public int isBlocked { get; set; }
        public int morningMode { get; set; }
        public int normalMode { get; set; }
        public int eveningMode { get; set; }
        public int status { get; set; }
        public int normalMixEES { get; set; }
        public int isInProcess { get; set; }
        public int isAutoMode { get; set; }
        public int carReady { get; set; }
        public int carAtEES { get; set; }
        public int eesMode { get; set; }
        public int queueId { get; set; }
        public string command { get; set; }
        public bool isDone { get; set; }

        public int customerPkId { get; set; }
        public bool isEntry { get; set; }
       

    }
}
