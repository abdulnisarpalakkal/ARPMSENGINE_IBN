using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model
{
    class NotificationData
    {
        public int Id { get; set; }
        public string MachineCode { get; set; }

        public enum errorCategory { NA, ERROR, MANUAL, TRIGGER, DISABLE, WAITING };
        public errorCategory category { get; set; }
        public Int32 ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public bool IsCleared { get; set; }
        public DateTime NotifyTime { get; set; }
    }
}
