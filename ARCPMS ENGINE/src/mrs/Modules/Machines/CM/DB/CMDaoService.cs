using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.DB
{
    interface CMDaoService
    {
        List<CMData> GetCMList();
        List<CMData> GetCMDetails(CMData objCMData);

        bool GetNearbyLCMlist(CMData objCMData,out CMData leftCMData,out CMData rightCMData);
        bool UpdateBoolValueUsingMachineCode(string machineCode,string tableField,bool dataValue);
        bool UpdateIntValueUsingMachineCode(string machineCode, string tableField, int dataValue);

        bool UpdateStringValueUsingMachineCode(string machineCode, string tableField, string dataValue);
        bool UpdateBoolValueUsingRemCode(string machineCode, string tableField, bool dataValue);
        
        bool IsCMBlockedInDB(string machineName);
       
        CMData GetMovingSideCM(CMData objCMData);

        bool GetValidAisleForMoving(CMData objCMData, out int moveAisle, out int moveRow);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool UpdateMachineBlockStatusForHome(string machine_code, bool blockStatus);

        bool IsCMDisabled(string machineCode);
        bool IsCMSwitchOff(string machineCode);

        bool UpdateCMClearRequest(string machineCode,string clearMachine, int clearFlag);
        bool UpdateCMClearPermission(string machineCode);
        bool GetCMClearRequest(string machineCode,out string clearingMachine,out int clearFlag);
        CMData NeedToPushNearestCM(CMData objCMData);
        int GetCMAisleFromDB(string machineName);
        /// <summary>
        /// get cm details blocked by queueid
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        CMData GetBlockedCMDetails(Int64 queueId);
        bool GetCMRotationEnabledStatus(string machineCode);
    }
}
