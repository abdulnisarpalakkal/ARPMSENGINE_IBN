using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Controller
{
    interface PVLControllerService
    {
        List<Model.PVLData> GetPVLList();
        Model.PVLData GetPVLDetails(Model.PVLData objPVLData);

        bool PVLMove(PVLData objPVLData);
        bool PVLPut(PVLData objPVLData);
        bool PVLGet(PVLData objPVLData);
        bool IsPalletOnPVL(PVLData objPVLData,out bool isHealthy);
        bool CheckPVLCommandDone(PVLData objPVLData);
        void FindCommandTypeAndDoneTag(Model.PVLData objPVLData, out int commandType, out string doneTag);
        bool DoTriggerAction(Model.PVLData objPVLData, int commandType);
        bool CheckPVLHealthy(PVLData objPVLData);
        bool FindSlotAndPathForPVL(PVLData objPVLData);
        bool TaskAfterPvlProcess(PVLData objPVLData);
        

        //bool AsynchReadSettingsForPVL();
        void AsynchReadListenerForPVL(object sender, RefreshEventArguments arg);
        bool IsPVLBlockedInDB(string machineName);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool IsPVLDisabled(string machineName);
        bool IsPVLSwitchOff(string machineName);

        int FindPalletGettingSlotAndPath(Model.PVLData objPVLData);
        int FindPalletStoringSlotAndPath(Model.PVLData objPVLData);
        void UpdateAfterPVLTask(Model.PVLData objPVLData);
        void TaskAfterGetPalletBundle(int pathID);

        bool ConfirmPVLReadyPUTByCM(string machineCode);
        bool IsPVLReadyForPUTByCM(Model.PVLData objPVLData);
        string GetDeckCodeOfPVL(string pvlCode);
    }
}
