using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Controller
{
    interface PSTControllerService
    {
        List<Model.PSTData> GetPSTList();
        Model.PSTData GetPSTDetails(Model.PSTData objPSTData);
        Model.PSTData GetPSTDetailsInRange(int minAisle,int maxAisle);

        int GetCountOfPallet(PSTData objPSTData);
        bool CheckPSTHealthy(PSTData objPSTData);

        bool IsPSTBlockedInDB(string machineName);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool IsPSTDisabled(string machineName);
        bool IsPSTSwitchOff(string machineName);
        
       // bool AsynchReadSettingsForPST();
        void AsynchReadListenerForPST(object sender, RefreshEventArguments arg);
       
    }
}
