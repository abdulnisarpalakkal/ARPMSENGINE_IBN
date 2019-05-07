using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.DB
{
    interface EESDaoService
    {
        List<EESData> GetEESList();
        List<Model.EESData> GetEESListInRange(int minRange, int maxRange);
        EESData GetEESDetails(EESData objEESData);
        bool UpdateBoolValueUsingMachineCode(string machineCode, string tableField, bool dataValue);
        bool UpdateIntValueUsingMachineCode(string machineCode, string tableField, int dataValue);
        bool UpdateStringValueUsingMachineCode(string machineCode, string tableField, string dataValue);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool UpdateMachineBlockStatusForPMS(string machine_code, bool blockStatus);

        bool IsEESDisabled(string machineName);
        bool IsEESSwitchOff(string machineName);
        bool IsEESEntryInCurrentModeInDB(string machineCode);
        bool IsPSNotGettingFromEES(string machineCode);

        bool IsEESBlockedInDBForParking(string machineName);
        bool IsEESBlockedInDBForPMS(string machineName);
        void UpdatePhotoPathToCustomerTable(EESData objEESData, string southimg, string northimg);
        //void SetCarOutsideTime(string machineCode, DateTime outsideDate);
        //void SetEESReadyTime(string machineCode, DateTime outsideDate);
        //void SetCarAlignedTime(string machineCode, DateTime outsideDate);
        //void SetReadyToGetTime(string machineCode, DateTime outsideDate);
        EESData GetBlockedEESDetails(Int64 queueId);
    }
}
