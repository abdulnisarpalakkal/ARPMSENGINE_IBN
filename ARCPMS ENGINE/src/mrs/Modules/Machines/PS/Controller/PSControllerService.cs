using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Controller
{
    interface PSControllerService
    {
        List<Model.PSData> GetPSList();
        Model.PSData GetPSDetailsIncludeAisle(int aisle);
        bool PSMove(PSData objPSData);
        bool PSGetFromEES(PSData objPSData);
        bool PSGetFromPST(PSData objPSData);
        bool PSPutToEES(PSData objPSData);
        bool PSPutToPST(PSData objPSData);
       
        bool CheckPSCommandDone(PSData objPSData);
        bool ClearPathForPS(PSData objPSData);
        bool ClearNearestPS(PSData objPSData);
        bool CheckPSHealthy(PSData objPSData);

        bool UpdatePSIntData(string machineCode,string opcTag,int dataValue);
        bool UpdatePSBoolData(string machineCode, string opcTag, bool dataValue);
        void FindCommandTypeAndDoneTag(PSData objPSData,out int commandType,out string doneTag);
        bool DoTriggerAction(PSData objPSData, int commandType);

        bool IsPSBlockedInDB(string machineName);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool IsPSDisabled(string machineName);
        bool IsPSSwitchOff(string machineName);

        bool IsPalletPresentOnPS(PSData objPSData, out bool hasPSCommunication);
        int GetAisleOfPS(PSData objPSData);

       // bool AsynchReadSettingsForPS();
        void AsynchReadListenerForPS(object sender, RefreshEventArguments arg);
    }
}
