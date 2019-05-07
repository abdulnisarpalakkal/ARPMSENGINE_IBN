using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.DB
{
    interface PSTDaoService
    {
        List<Model.PSTData> GetPSTList();
        Model.PSTData GetPSTDetails(Model.PSTData objPSTData);
        Model.PSTData GetPSTDetailsInRange(int minAisle, int maxAisle);
        
        bool IsPSTBlockedInDB(string machineName);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool IsPSTDisabled(string machineName);
        bool IsPSTSwitchOff(string machineName);
     
    }
}
