using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Slot.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Slot.Controller
{
    interface SlotControllerService
    {
        bool IsSlotValid(SlotData objSlotData);
        bool updateSlotAfterCarProcessing(SlotData objSlotData);
        bool updateSlotAfterGetCarFromSlot(SlotData objSlotData);

    }
}
