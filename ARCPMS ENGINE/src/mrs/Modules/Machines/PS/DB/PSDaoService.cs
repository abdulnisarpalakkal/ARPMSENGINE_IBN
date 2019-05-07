using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.DB
{
    interface PSDaoService
    {
        List<Model.PSData> GetPSList();
        Model.PSData GetPSDetails(Model.PSData objPSData);
        Model.PSData GetPSDetailsIncludeAisle(int aisle);

        bool UpdateBoolValueUsingMachineCode(string machineCode, string tableField, bool dataValue);
        bool UpdateIntValueUsingMachineCode(string machineCode, string tableField, int dataValue);

        bool UpdateStringValueUsingMachineCode(string machineCode, string tableField, string dataValue);

        bool IsPSBlockedInDB(string machineName);

        int GetValidAisleForMoving(Model.PSData objPSData);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);

        bool IsPSDisabled(string machineName);
        bool IsPSSwitchOff(string machineName);
    }
}
