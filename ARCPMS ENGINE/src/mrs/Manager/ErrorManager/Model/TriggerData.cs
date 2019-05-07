using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model
{
    class TriggerData
    {
        public string MachineCode { get; set; }
        public enum triggerCategory { NA, ERROR, MANUAL, TRIGGER, DISABLE, WAITING };
        public triggerCategory category { get; set; }
        public Int32 ErrorCode { get; set; }
        public bool TriggerEnabled { get; set; }
    }
}
