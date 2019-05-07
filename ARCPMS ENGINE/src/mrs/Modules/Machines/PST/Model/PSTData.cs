using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Model
{
    [Serializable]
    class PSTData
    {

        public int pstPkId { get; set; }
        public string pstName { get; set; }
        public int aisle { get; set; }
        public int row { get; set; }
        public int quantity { get; set; }
        public string machineChannel { get; set; }
        public string machineCode { get; set; }
        public bool isBlocked { get; set; }
        public int status { get; set; }
        public int pvlPkId { get; set; }
        public bool isSwitchOff { get; set; }

    }
}
