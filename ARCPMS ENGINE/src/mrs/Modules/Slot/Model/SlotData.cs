using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Slot.Model
{
    class SlotData
    {

        public int slotPkId { get; set; }
        public int level { get; set; }
        public int aisle { get; set; }
        public int row { get; set; }
        public int slotType { get; set; }
        public int valueInNumber { get; set; }
        public string valueInString { get; set; }
        public int valueType { get; set; }
        public int slotStatus { get; set; }
        public DateTime updatedTime { get; set; }
        public string updatedBy { get; set; }
        public int parkBlock { get; set; }
        public int palletBlock { get; set; }
        public int queueId { get; set; }

    }
}
