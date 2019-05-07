using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Controller
{
    interface EESControllerService 
    {
        //commands
        void AsynchReadListenerForEES(object sender, RefreshEventArguments arg);

        List<Model.EESData> GetEESListInRange(int minRange, int maxRange);
        bool ChangeMode(EESData objEESData);
        bool ExcecuteEESGetCar(EESData objEESData);
        bool TakePhoto(EESData objEESData);
        bool ExcecutePaymentDone(EESData objEESData);

        bool IsEESReadyForChangeMode(EESData objEESData);
        bool IsEESReadyForParkingChangeMode(EESData objEESData);
        int GetEESMode(EESData objEESData);
        int CheckError(EESData objEESData);
        bool CheckEESClear(EESData objEESData);

        bool UpdateEESIntData(string machineCode, string opcTag, int dataValue);
        bool UpdateEESBoolData(string machineCode, string opcTag, bool dataValue);
        ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE GetCarType(EESData objEESData);
        //bool GetLowerHeightSensorStatus(EESData objEESData);
        //bool GetMidHeightSensorStatus(EESData objEESData);
        bool checkCarAtEES(EESData objEESData);
        List<EESData> GetEESList();
        EESData GetEESDetails(EESData objEESData);
        bool checkCarReadyAtEntry(EESData objEESData);

        
        bool IsDoneExcecuteEESGetCar(EESData objEESData);
        bool UpdateMachineBlockStatus(string machine_code, bool blockStatus);
        bool UpdateMachineBlockStatusForPMS(string machine_code, bool blockStatus);
        bool CheckEESHealthy(EESData objEESData);

        bool IsEESEntryInCurrentModeInDB(string machineCode);
        bool IsPalletPresentOnEES(Model.EESData objEESData);
        bool IsEESEntryInOPC(EESData objEESData);

        bool IsEESBlockedInDBForParking(string machineName);
        bool IsEESReadyForParkingChangeModeBack(EESData objEESData);
        bool IsEESBlockedInDBForPMS(string machineName);


        bool GetCountOfEntryAndExitRequests(List<Model.EESData> eesList,out int entryRequestCount,out int exitRequestCount);

        bool IsEESReadyForPS(Model.EESData objEESData);
        bool IsEESReadyForRem(Model.EESData objEESData);
        bool ConfirmEESReadyForREMLock(string machineCode);

        int GetEESState(EESData objEESData);
        bool IsEESDisabled(string machineName);
       
              
    }
}
