using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.DB
{
    interface ParkingDaoService
    {
        List<Model.PathDetailsData> FindAndGetInitialPath(int queueId);
        List<Model.PathDetailsData> FindAndGetInitialEntryPath(int queueId);
        List<Model.PathDetailsData> FindAndGetInitialExitPath(int queueId);
        List<Model.PathDetailsData> FindAndGetDynamicPath(int queueId);
        List<Model.PathDetailsData> FindAndGetDynamicEntryPath(int queueId);
        List<Model.PathDetailsData> FindAndGetDynamicExitPath(int queueId);
        List<Model.PathDetailsData> FindAndGetVLCDynamicPath(int queueId); //created on 31MAR2015
        List<Model.PathDetailsData> FindAndGetAllocationPath(int queueId);
        List<Model.PathDetailsData> GetPathDetails(int queueId, int pathId = 0);
        bool UpdateSlotAfterGetCarFromSlot(int queueId);
        bool CallProcedureAfterProcessCar(int queueId);
        void ResetProcedureCall();
        bool IsRotated(int queueId);
        bool UpdateRotaion(int queueId, bool rotateStatus);
        bool IsSlotFromEESLevel(int queueId);
        void UpdateCarReachedAtEESTime(int exitQueueId);
        string GetCardIdUsingQueueId(int queueId);
        Model.PathDetailsData GetEachPathDetailsRecord(int queueId, int pathId = 0);
        bool IsPathDynamicVLC(int queueId);//created on 31MAR2015
        void UpdateSearchingPathStatus(int queueId, bool searchStatus); //created on 11APR2015
        ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE IsExitCarHigh(int queueId);
        bool FindOptimizedPath(int queueId);
        int GetMachineStage(int queueId);
        bool GetIterationStatus(int queueId);
        int GetPathStartFlag(int queueId);
        void ResumeProcedureCall();
        int GetMachineSearchFlag(int queueId);
    }
}
