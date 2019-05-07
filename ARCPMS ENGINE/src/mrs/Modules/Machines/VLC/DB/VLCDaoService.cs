using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model;
using ARCPMS_ENGINE.src.mrs.Global;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.DB
{
    interface VLCDaoService
    {
        List<VLCData> GetVLCList();
        VLCData GetVLCDetails(VLCData objVLCData);
        bool UpdateBoolValueUsingMachineCode(string machineCode, string tableField, bool dataValue);
        bool UpdateIntValueUsingMachineCode(string machineCode, string tableField, int dataValue);
        bool UpdateStringValueUsingMachineCode(string machineCode, string tableField, string dataValue);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);

        bool IsVLCDisabled(string machineName);
        bool IsVLCSwitchOff(string machineName);
        bool IsVLCBlockedInDB(string machineName);
        /// <summary>
        /// Get VLC blocked by queueId
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        Model.VLCData GetVLCDetails(Int64 queueId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="machineName"></param>
        /// <returns></returns>
        GlobalValues.vlcMode GetVLCMode(string machineName);
    }
}
