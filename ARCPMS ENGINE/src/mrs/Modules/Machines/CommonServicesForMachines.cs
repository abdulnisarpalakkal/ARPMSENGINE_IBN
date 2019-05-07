using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines
{
    interface CommonServicesForMachines
    {
        bool UpdateMachineValues();
        bool AsynchReadSettings();

        
        bool UpdateMachineTagValueToDBFromListener(string machineCode, string machineTag, Object dataValue);
        void GetDataTypeAndFieldOfTag(string opcTag, out int dataType, out string tableField, out bool isRem);
    }
}
