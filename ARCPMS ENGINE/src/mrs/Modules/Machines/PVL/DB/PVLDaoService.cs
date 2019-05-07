using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.DB
{
    interface PVLDaoService
    {
        List<Model.PVLData> GetPVLList();
        Model.PVLData GetPVLDetails(Model.PVLData objPVLData);

        bool IsPVLBlockedInDB(string machineName);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool IsPVLDisabled(string machineName);
        bool IsPVLSwitchOff(string machineName);
        int FindPalletGettingSlotAndPath(Model.PVLData objPVLData);
        int FindPalletStoringSlotAndPath(Model.PVLData objPVLData);
        void UpdateAfterPVLTask(Model.PVLData objPVLData);
        void TaskAfterGetPalletBundle(int pathID);
        bool InsertQueueForPVL(int requestType, string pvlCode);
    }
}
