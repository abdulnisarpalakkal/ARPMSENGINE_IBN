using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;


namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Controller
{
    interface VLCControllerService
    {

        bool VLCMove(VLCData objVLCData);
        bool CheckVLCCommandDone(VLCData objVLCData);
        int CheckError(VLCData objVLCData);
        bool CheckVLCHealthy(VLCData objVLCData);

        bool UpdateVLCIntData(string machineCode, string opcTag, int dataValue);
        bool UpdateVLCBoolData(string machineCode, string opcTag, bool dataValue);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);

        void AsynchReadListenerForVLC(object sender, RefreshEventArguments arg);
        bool ConfirmReachedAtFloor(VLCData objVLCData);
        int VLCAtFloor(VLCData objVLCData);
        VLCData GetVLCDetails(VLCData objVLCData);
        bool IsVLCDisabled(string machineName);
        TriggerData NeedToShowTrigger(VLCData objVLCData);
        /// <summary>
        /// Get VLC blocked by queueId
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        Model.VLCData GetVLCDetails(Int64 queueId);
        /// <summary>
        /// for moving VLC to home position
        /// </summary>
        /// <param name="objVLCData"></param>
        /// <returns></returns>
        bool VLCHomeMove(Model.VLCData objVLCData);
        
    }
}
