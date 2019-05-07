using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.PalletManager.DB;
using System.Threading;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.PvlManager.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model;
using ARCPMS_ENGINE.src.mrs.OPCOperations;
using OPC;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp;
using System.Threading.Tasks;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ErrorManager.Model;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.DB;




namespace ARCPMS_ENGINE.src.mrs.Manager.PalletManager.Controller
{
    class PalletManagerImp:PalletManagerService
    {
        private volatile bool _shouldStop;

        PalletDaoService objPalletDaoService = null;
        EESControllerService objEESControllerService = null;
        PSControllerService objPSControllerService = null;
        PSTControllerService objPSTControllerService = null;
        PVLControllerService objPVLControllerService = null;
        ErrorControllerService objErrorControllerService = null;
        PVLManagerService objPVLManagerService = null;
        PVLDaoService objPVLDaoService = null;

        List<EESData> eesList = null;
        List<PSData> psList = null;
        List<PSTData> pstList = null;
        List<PVLData> pvlList = null;
        static int PMS_MODE = 3;
        private Object psLock = new Object();
        private Object pstLock = new Object();
        private Object pvlLock = new Object();
        private Object eesLock = new Object();


        public PalletManagerImp()
        {
            objPalletDaoService = new PalletDaoImp();
            objEESControllerService = new EESControllerImp();
            objPSControllerService = new PSControllerImp();
            objPSTControllerService = new PSTControllerImp();
            objPVLControllerService = new PVLControllerImp();

            eesList = objEESControllerService.GetEESList();
            psList = objPSControllerService.GetPSList();
            pstList = objPSTControllerService.GetPSTList();
            pvlList = objPVLControllerService.GetPVLList();

        }

        public void StartModeScanning()
        {
            bool isHealthy = false;
            if (objPalletDaoService==null)  objPalletDaoService = new PalletDaoImp();
            if (objEESControllerService==null) objEESControllerService = new EESControllerImp();
            bool eesLocked = false;



            while (!_shouldStop)
            {
                try
                {


                    if (!objPalletDaoService.IsPMSInL2())
                    {
                        Thread.Sleep(1500);
                        continue;
                    }

                    PMS_MODE = objPalletDaoService.GetCurrentPMSMode();

                    // Get EES name one by one.
                    foreach (EESData objEESData in eesList)
                    {
                        try
                        {
                            //check status of ees.
                            eesLocked = false;
                            isHealthy =  objEESControllerService.IsEESReadyForChangeMode(objEESData);

                            if (!isHealthy ) continue;

                            //check setting and current mode are same
                            objEESData.eesMode = objEESControllerService.IsEESEntryInCurrentModeInDB(objEESData.machineCode) ? GlobalValues.EESEntry : GlobalValues.EESExit;
                            if (objEESControllerService.GetEESMode(objEESData) == objEESData.eesMode) 
                                continue;

                            if (objEESControllerService.UpdateMachineBlockStatusForPMS(objEESData.machineCode,true))
                            {
                                eesLocked = true;
                                objEESControllerService.ChangeMode(objEESData);
                                
                            }

                        }
                        catch (Exception errMsg)
                        {

                            Console.WriteLine(errMsg.Message);
                        }
                        finally
                        {
                            if(eesLocked)
                                objEESControllerService.UpdateMachineBlockStatusForPMS(objEESData.machineCode, false);
                        }
                    }
                }
                catch (Exception errMsg)
                {
                    Console.WriteLine(errMsg.Message);
                }
                Thread.Sleep(1500);
            }
        }
        public void StartPMSProcessing()
        {
            if(objPalletDaoService==null) objPalletDaoService=new PalletDaoImp();
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            
            
            List<EESData> eesList1=null;
            List<EESData> eesList2=null;
            int halfCount = 0;

            try
            {
                halfCount = eesList.Count / 2;
                eesList1=new List<EESData>(eesList);
                eesList2=new List<EESData>(eesList);
                eesList1.RemoveRange(halfCount, eesList.Count - halfCount);
                eesList2.RemoveRange(0, halfCount);





                Task.Factory.StartNew(() => StartEESScanning(eesList1));
                Task.Factory.StartNew(() => StartEESScanning(eesList2));
                Task.Factory.StartNew(() => StartPSScanning());
                Task.Factory.StartNew(() => StartPSTScanning());
                Task.Factory.StartNew(() => StartPVLScanning());

                   
               
                
            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "StartPMSProcessing:"+ " Exception=" + ex.Message);
            }
        }

        public void StartEESScanning(List<EESData> scanningEESList)
        {

            if (objPalletDaoService == null) objPalletDaoService = new PalletDaoImp();
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            bool isEntry=false;
            bool isPalletPresent=false;
            bool needToFeedPallet = false;
            bool needToRemovePallet = false;
            bool isEESHealthy = false;
            bool hasCarAtEES=false;
           
            
            while (!_shouldStop)
            {
                try
                {

                    foreach (EESData objEESData in scanningEESList)
                    {
                        PSData objPSData = new PSData();
                     
                        EESData objRefEESData = new EESData();;
                        isEESHealthy = objEESControllerService.CheckEESHealthy(objEESData);
                        hasCarAtEES = objEESControllerService.checkCarAtEES(objEESData);
                        if (!objPalletDaoService.IsPMSInL2() || !isEESHealthy ||  hasCarAtEES)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        isEntry = objEESControllerService.IsEESEntryInOPC(objEESData);
                        isPalletPresent = objEESControllerService.IsPalletPresentOnEES(objEESData);


                        needToFeedPallet = isEntry && !isPalletPresent;
                        if (!needToFeedPallet)
                            needToRemovePallet = !isEntry && isPalletPresent;
                        if (!(needToRemovePallet || needToFeedPallet))
                            continue;


                        objRefEESData = BasicConfig.Clone<EESData>(objEESData);
                     
                        if (PMS_MODE == GlobalValues.MORNING_MODE)
                        {

                            if (needToFeedPallet)
                                Task.Factory.StartNew(() => PutEESByPS(objPSData, objRefEESData));
                            else if (needToRemovePallet)
                                Task.Factory.StartNew(() => GetEESByPS(objPSData, objRefEESData));
                        }
                        else if (PMS_MODE == GlobalValues.EVENING_MODE)
                        {
                            if (needToFeedPallet)
                                Task.Factory.StartNew(() => PutEESByPS(objPSData, objRefEESData));
                            else if (needToRemovePallet)
                                Task.Factory.StartNew(() => GetEESByPS(objPSData, objRefEESData));
                        }
                        else
                        {
                            if (needToFeedPallet)
                                Task.Factory.StartNew(() => PutEESByPS(objPSData, objRefEESData));
                            else if (needToRemovePallet)
                                Task.Factory.StartNew(() => GetEESByPS(objPSData, objRefEESData));
                        }

                    }
                }
                catch (Exception ex)
                {

                    Logger.WriteLogger(GlobalValues.PMS_LOG, "EES scanning:" + " Exception=" + ex.Message);
                }
                Thread.Sleep(500);
            }
        
        }



        public void StartPSScanning()
        {
            if (objPalletDaoService == null) objPalletDaoService = new PalletDaoImp();
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            if (objPSTControllerService == null) objPSTControllerService = new PSTControllerImp();

            OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
            bool isPalletOnPS = false;
            int entryEESWithoutPallet = 0;
            int exitEESWithPallet = 0;
            
           
            bool hasPSCommunication = false;


           // initial window setting 
            foreach (PSData objPSData in psList)
            {
                if (opcd == null) opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());

                opcd.WriteTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Min_Window_Limit, objPSData.dynamicMin);
                opcd.WriteTag<int>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Max_Window_Limit, objPSData.dynamicMax);
 

            }

            Logger.WriteLogger(GlobalValues.PMS_LOG, "PS scanning started.....");
            while (!_shouldStop)
            {
                if (opcd == null) opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                try
                {
                    
                    foreach (PSData objPSData in psList)
                    {
                        PSData objRefPSData = new PSData();
                        PSTData objPSTData = new PSTData();
                        entryEESWithoutPallet = 0;
                        exitEESWithPallet = 0;
                        
                        if (!objPalletDaoService.IsPMSInL2() || !objPSControllerService.CheckPSHealthy(objPSData))
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        objPSData.dynamicMin = opcd.ReadTag<Int32>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Min_Window_Limit);
                        objPSData.dynamicMax = opcd.ReadTag<Int32>(objPSData.machineChannel, objPSData.machineCode, OpcTags.PS_L2_Max_Window_Limit);

                        if (objPSData.dynamicMin == 0 || objPSData.dynamicMax == 0) continue;

                        List<EESData> eesList = objEESControllerService.GetEESListInRange(objPSData.dynamicMin, objPSData.dynamicMax);  //Finding the count of entry request EESs 
                        if (eesList != null && eesList.Count > 0)
                        {                                                                                                              //and exit request EESs in PS zone
                            objEESControllerService.GetCountOfEntryAndExitRequests(eesList, out  entryEESWithoutPallet, out exitEESWithPallet);
                        }
                        isPalletOnPS = objPSControllerService.IsPalletPresentOnPS(objPSData, out hasPSCommunication);
                        if (!hasPSCommunication) continue;


                        objRefPSData = BasicConfig.Clone<PSData>(objPSData);
                      
                        if (PMS_MODE == 3)//Normal Mode
                        {

                           
                            if (isPalletOnPS)
                            {
                                if (entryEESWithoutPallet > 0 ) continue;
                                else if (exitEESWithPallet > 0)
                                {
                                    objPSTData = objPSTControllerService.GetPSTDetailsInRange(objRefPSData.dynamicMin, objRefPSData.dynamicMax);
                                    if (objPSTControllerService.CheckPSTHealthy(objPSTData) && objPSTControllerService.GetCountOfPallet(objPSTData) < 4)
                                    {

                                        Task.Factory.StartNew(() => PutPSTByPS(objRefPSData, objPSTData));
                                    }
                                  
                                }
                            }
                            else
                            {
                                if (exitEESWithPallet > 0) continue;
                                else if (entryEESWithoutPallet > 0)
                                {
                                    objPSTData = objPSTControllerService.GetPSTDetailsInRange(objRefPSData.dynamicMin, objRefPSData.dynamicMax);
                                    if (objPSTControllerService.CheckPSTHealthy(objPSTData) && objPSTControllerService.GetCountOfPallet(objPSTData) > 0)
                                    {
                                        Task.Factory.StartNew(() => GetPSTByPS(objRefPSData, objPSTData));
                                    }
                                    
                                }
                            }
                        }
                        else if (PMS_MODE == 1)
                        {
                            if (isPalletOnPS)
                            {
                                if (entryEESWithoutPallet > 0) continue;
                                else if (exitEESWithPallet > 0)
                                {
                                    objPSTData = objPSTControllerService.GetPSTDetailsInRange(objRefPSData.dynamicMin, objRefPSData.dynamicMax);
                                    if (objPSTControllerService.CheckPSTHealthy(objPSTData) && objPSTControllerService.GetCountOfPallet(objPSTData) < 4)
                                    {

                                        Task.Factory.StartNew(() => PutPSTByPS(objRefPSData, objPSTData));
                                    }

                                }
                            }
                            else
                            {
                                if (exitEESWithPallet > 0) continue;
                                else 
                                {
                                    objPSTData = objPSTControllerService.GetPSTDetailsInRange(objRefPSData.dynamicMin, objRefPSData.dynamicMax);
                                    if (objPSTControllerService.CheckPSTHealthy(objPSTData) && objPSTControllerService.GetCountOfPallet(objPSTData) > 0)
                                    {
                                        Task.Factory.StartNew(() => GetPSTByPS(objRefPSData, objPSTData));
                                    }

                                }
                            }
                        }
                        else if (PMS_MODE == 2)
                        {
                            if (isPalletOnPS)
                            {
                                if (entryEESWithoutPallet > 0) continue;
                                else 
                                {
                                    objPSTData = objPSTControllerService.GetPSTDetailsInRange(objRefPSData.dynamicMin, objRefPSData.dynamicMax);
                                    if (objPSTControllerService.CheckPSTHealthy(objPSTData) && objPSTControllerService.GetCountOfPallet(objPSTData) < 4)
                                    {

                                        Task.Factory.StartNew(() => PutPSTByPS(objRefPSData, objPSTData));
                                    }

                                }
                            }
                            else
                            {
                                if (exitEESWithPallet > 0) continue;
                                else if (entryEESWithoutPallet > 0)
                                {
                                    objPSTData = objPSTControllerService.GetPSTDetailsInRange(objRefPSData.dynamicMin, objRefPSData.dynamicMax);
                                    if (objPSTControllerService.CheckPSTHealthy(objPSTData) && objPSTControllerService.GetCountOfPallet(objPSTData) > 0)
                                    {
                                        Task.Factory.StartNew(() => GetPSTByPS(objRefPSData, objPSTData));
                                    }

                                }
                            }
                        }
                      
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogger(GlobalValues.PMS_LOG, "StartPSScanning:" + " Exception=" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
        }
        public void StartPSTScanning()
        {

            if (objPalletDaoService == null) objPalletDaoService = new PalletDaoImp();
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            if (objPSTControllerService == null) objPSTControllerService = new PSTControllerImp();
            if (objPVLControllerService == null) objPVLControllerService = new PVLControllerImp();

            OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
            bool hasPalletOnPS = false;
            bool isPSTFull = false;
            bool isPSTEmpty = false;
            int entryEESWithoutPallet = 0;
            int exitEESWithPallet = 0;
            int countOfPST;
            bool hasPalletOnPVL = false;
            bool isPVLHealthy = false;



            
            
            bool hasPSCommunication = false;

            Logger.WriteLogger(GlobalValues.PMS_LOG, "PST scanning started.....");
            while (!_shouldStop)
            {
                if (opcd == null) opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                try
                {
                   
                    foreach (PSTData objPSTData in pstList)
                    {
                        PSTData objRefPSTData = new PSTData();
                        PSData objPSData = null;
                        PVLData objPVLData = null;
                        objPSData = new PSData();
                        entryEESWithoutPallet = 0;
                        exitEESWithPallet = 0;
                        
                        if (!objPalletDaoService.IsPMSInL2() || !objPSTControllerService.CheckPSTHealthy(objPSTData))
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        objPSData=objPSControllerService.GetPSDetailsIncludeAisle(objPSTData.aisle);
                        hasPalletOnPS = objPSControllerService.IsPalletPresentOnPS(objPSData, out hasPSCommunication);
                        if (!hasPSCommunication) 
                            continue;
                        
                        //Finding the count of entry request EESs and exit request EESs in PS zone
                        List<EESData> eesList = objEESControllerService.GetEESListInRange(objPSData.dynamicMin, objPSData.dynamicMax);
                        if (eesList != null && eesList.Count > 0)
                        {
                            objEESControllerService.GetCountOfEntryAndExitRequests(eesList, out  entryEESWithoutPallet, out exitEESWithPallet);
                        }
                        countOfPST=objPSTControllerService.GetCountOfPallet(objPSTData);
                        if(countOfPST<0) continue;
                        isPSTFull =  countOfPST>= 4;
                        isPSTEmpty = countOfPST == 0;

                        objRefPSTData = BasicConfig.Clone<PSTData>(objPSTData);

                      

                        if (PMS_MODE == 3)//Normal Mode
                        {


                            if (isPSTFull)
                            {
                                if (entryEESWithoutPallet >= exitEESWithPallet ) continue;
                                else if ((exitEESWithPallet > 0 && hasPalletOnPS) || (exitEESWithPallet > 1 && !hasPalletOnPS))
                                {
                                   
                                        objPVLData = new PVLData();
                                        objPVLData.pvlPkId = objPSTData.pvlPkId;
                                        objPVLData = objPVLControllerService.GetPVLDetails(objPVLData);
                                        hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out isPVLHealthy);
                                        if (objPVLControllerService.CheckPVLHealthy(objPVLData) && !hasPalletOnPVL && isPVLHealthy)
                                        {
                                            // PSTData objPSTData2 = objPSTData;
                                            Task.Factory.StartNew(() => GetPSTByPVL(objPVLData, objRefPSTData, objPSData));
                                        }
                                  

                                }
                            }
                            else if (isPSTEmpty)
                            {
                                if (exitEESWithPallet >= entryEESWithoutPallet) continue;
                                else if ((entryEESWithoutPallet > 0 && !hasPalletOnPS) || (entryEESWithoutPallet > 1 && hasPalletOnPS))
                                {
                                    objPVLData = new PVLData();
                                        objPVLData.pvlPkId = objPSTData.pvlPkId;
                                        objPVLData = objPVLControllerService.GetPVLDetails(objPVLData);
                                        hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out isPVLHealthy);
                                        if (objPVLControllerService.CheckPVLHealthy(objPVLData) && hasPalletOnPVL && isPVLHealthy)
                                        {

                                            Task.Factory.StartNew(() => PutPSTByPVL(objPVLData, objRefPSTData, objPSData));
                                        }
                                   

                                }
                            }
                        }
                        else if (PMS_MODE == 1)
                        {
                            if (isPSTFull)
                            {
                                if (entryEESWithoutPallet >= exitEESWithPallet && (exitEESWithPallet != 0 || entryEESWithoutPallet != 0)) continue;
                                else if ((exitEESWithPallet > 0 && hasPalletOnPS) || (exitEESWithPallet > 1 && !hasPalletOnPS))
                                {
                                  
                                        objPVLData = new PVLData();
                                        objPVLData.pvlPkId = objPSTData.pvlPkId;
                                        objPVLData = objPVLControllerService.GetPVLDetails(objPVLData);
                                        hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out isPVLHealthy);
                                        if (objPVLControllerService.CheckPVLHealthy(objPVLData) && !hasPalletOnPVL && isPVLHealthy)
                                        {
                                            // PSTData objPSTData2 = objPSTData;
                                            Task.Factory.StartNew(() => GetPSTByPVL(objPVLData, objRefPSTData, objPSData));
                                        }
                                  

                                }
                            }
                            else if (isPSTEmpty)
                            {
                                if (exitEESWithPallet >= entryEESWithoutPallet && (exitEESWithPallet != 0 || entryEESWithoutPallet != 0)) continue;
                                else 
                                {
                                   
                                        objPVLData = new PVLData();
                                        objPVLData.pvlPkId = objPSTData.pvlPkId;
                                        objPVLData = objPVLControllerService.GetPVLDetails(objPVLData);
                                        hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out isPVLHealthy);
                                        if (objPVLControllerService.CheckPVLHealthy(objPVLData) && hasPalletOnPVL && isPVLHealthy)
                                        {

                                            Task.Factory.StartNew(() => PutPSTByPVL(objPVLData, objRefPSTData, objPSData));
                                        }
                                 

                                }
                            }
                        }
                        else if (PMS_MODE == 2)
                        {
                            if (isPSTFull)
                            {
                                if (entryEESWithoutPallet >= exitEESWithPallet && (exitEESWithPallet != 0 || entryEESWithoutPallet != 0)) continue;
                                else 
                                {
                                   
                                        objPVLData = new PVLData();
                                        objPVLData.pvlPkId = objPSTData.pvlPkId;
                                        objPVLData = objPVLControllerService.GetPVLDetails(objPVLData);
                                        hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out isPVLHealthy);
                                        if (objPVLControllerService.CheckPVLHealthy(objPVLData) && !hasPalletOnPVL && isPVLHealthy)
                                        {
                                            // PSTData objPSTData2 = objPSTData;
                                            Task.Factory.StartNew(() => GetPSTByPVL(objPVLData, objRefPSTData, objPSData));
                                        }
                                  
                                }
                            }
                            else if (isPSTEmpty)
                            {
                                if (exitEESWithPallet >= entryEESWithoutPallet && (exitEESWithPallet != 0 || entryEESWithoutPallet != 0)) continue;
                                else if ((entryEESWithoutPallet > 0 && !hasPalletOnPS) || (entryEESWithoutPallet > 1 && hasPalletOnPS))
                                {
                                    
                                        objPVLData = new PVLData();
                                        objPVLData.pvlPkId = objPSTData.pvlPkId;
                                        objPVLData = objPVLControllerService.GetPVLDetails(objPVLData);
                                        hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out isPVLHealthy);
                                        if (objPVLControllerService.CheckPVLHealthy(objPVLData) && hasPalletOnPVL && isPVLHealthy)
                                        {

                                            Task.Factory.StartNew(() => PutPSTByPVL(objPVLData, objRefPSTData, objPSData));
                                        }
                                   
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogger(GlobalValues.PMS_LOG, "StartPSTScanning:" + " Exception=" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
            

        }
        public void StartPVLScanning()
        {
            if (objPalletDaoService == null) objPalletDaoService = new PalletDaoImp();
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            if (objPSTControllerService == null) objPSTControllerService = new PSTControllerImp();
            if (objPVLControllerService == null) objPVLControllerService = new PVLControllerImp();

            OpcOperationsService opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
            bool hasPalletOnPS = false;
            bool isPSTFull = false;
            bool isPSTEmpty = false;
            bool hasPalletOnPVL = false;
            int entryEESWithoutPallet = 0;
            int exitEESWithPallet = 0;
            int countOfPST;
            int countSpaceInPSAndPST = 0;
            int countPalletInPSAndPST = 0;
            bool hasPVLCommunication = false;
            bool hasPSCommunication = false;


            

            Logger.WriteLogger(GlobalValues.PMS_LOG, "PVL scanning started.....");
            while (!_shouldStop)
            {
                if (opcd == null) opcd = new OpcOperationsImp(OpcConnection.GetOPCServerConnection());
                try
                {
                    
                    foreach (PVLData objPVLData in pvlList)
                    {
                        Logger.WriteLogger(GlobalValues.PMS_LOG, "PVL scanning:  take " + objPVLData.machineCode + "for scanning");
                        entryEESWithoutPallet = 0;
                        exitEESWithPallet = 0;
                        countSpaceInPSAndPST = 0;
                        countPalletInPSAndPST = 0;
                        PVLData objRefPVLData = null;
                        PSTData objPSTData = null;
                        PSData objPSData = null;

                        objRefPVLData = new PVLData();
                        if (!objPalletDaoService.IsPMSInL2() || !objPVLControllerService.CheckPVLHealthy(objPVLData))
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        objPSTData = new PSTData();
                        objPSTData.pvlPkId = objPVLData.pvlPkId;
                        objPSTData=objPSTControllerService.GetPSTDetails(objPSTData);
                        objPSData = objPSControllerService.GetPSDetailsIncludeAisle(objPSTData.aisle);

                        hasPalletOnPS = objPSControllerService.IsPalletPresentOnPS(objPSData, out hasPSCommunication);
                        if (!hasPSCommunication) continue;

                        //Finding the count of entry request EESs and exit request EESs in PS zone
                        List<EESData> eesList = new List<EESData>();
                        eesList = objEESControllerService.GetEESListInRange(objPSData.dynamicMin, objPSData.dynamicMax);
                        if (eesList!=null && eesList.Count > 0)
                        {
                            objEESControllerService.GetCountOfEntryAndExitRequests(eesList, out  entryEESWithoutPallet, out exitEESWithPallet);
                        }
                        countOfPST = objPSTControllerService.GetCountOfPallet(objPSTData);
                        if (countOfPST < 0) continue;
                        isPSTFull = countOfPST >= 4;
                        isPSTEmpty = countOfPST == 0;
                        
                        hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData,out hasPVLCommunication);

                        if (!hasPVLCommunication) continue;

                        objRefPVLData = BasicConfig.Clone<PVLData>(objPVLData);
                      
                        if (PMS_MODE == 3)//Normal Mode
                        {
                            if (hasPalletOnPVL)
                            {
                                countSpaceInPSAndPST = 5-(countOfPST + (hasPalletOnPS ? 1 : 0));
                                if (entryEESWithoutPallet >= exitEESWithPallet 
                                    && (entryEESWithoutPallet != 0 || exitEESWithPallet!=0)) continue;
                                else if (exitEESWithPallet > countSpaceInPSAndPST || countOfPST>=3)
                                {


                                    Task.Factory.StartNew(() => GetPVLByUCM(objRefPVLData));
                                   
                                }
                            }
                            else if (!hasPalletOnPVL)
                            {
                                hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out hasPVLCommunication);
                                if (hasPalletOnPVL || !hasPVLCommunication) continue; //revalidation

                                countPalletInPSAndPST = countOfPST + (hasPalletOnPS ? 1 : 0);
                                if (exitEESWithPallet >= entryEESWithoutPallet 
                                    && (entryEESWithoutPallet != 0 || exitEESWithPallet!=0)) continue;
                                else if (entryEESWithoutPallet > countPalletInPSAndPST || countOfPST<=1)
                                {

                                    Task.Factory.StartNew(() => PutPVLByUCM(objRefPVLData));

                                }
                            }
                        }
                        else if (PMS_MODE == 1)//morning mode
                        {
                            if (hasPalletOnPVL)
                            {
                                countSpaceInPSAndPST = 5 - (countOfPST + (hasPalletOnPS ? 1 : 0));
                                if (entryEESWithoutPallet >= exitEESWithPallet
                                    && (entryEESWithoutPallet != 0 || exitEESWithPallet != 0)) continue;
                                else if (exitEESWithPallet > countSpaceInPSAndPST || isPSTFull)
                                {


                                    Task.Factory.StartNew(() => GetPVLByUCM(objRefPVLData));

                                }
                            }
                            else if (!hasPalletOnPVL)
                            {
                                hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out hasPVLCommunication);
                                if (hasPalletOnPVL || !hasPVLCommunication) continue; //revalidation

                                countPalletInPSAndPST = countOfPST + (hasPalletOnPS ? 1 : 0);
                                if (exitEESWithPallet >= entryEESWithoutPallet
                                    && (entryEESWithoutPallet != 0 || exitEESWithPallet != 0)) continue;
                                else if (countOfPST <= 2)
                                {

                                    Task.Factory.StartNew(() => PutPVLByUCM(objRefPVLData));

                                }
                            }
                        }
                        else if (PMS_MODE == 2)//evening mode
                        {
                            if (hasPalletOnPVL)
                            {
                                countSpaceInPSAndPST = 5 - (countOfPST + (hasPalletOnPS ? 1 : 0));
                                if (entryEESWithoutPallet >= exitEESWithPallet
                                    && (entryEESWithoutPallet != 0 || exitEESWithPallet != 0)) continue;
                                else if (countOfPST >=2)
                                {


                                    Task.Factory.StartNew(() => GetPVLByUCM(objRefPVLData));

                                }
                            }
                            else if (!hasPalletOnPVL)
                            {
                                hasPalletOnPVL = objPVLControllerService.IsPalletOnPVL(objPVLData, out hasPVLCommunication);
                                if (hasPalletOnPVL || !hasPVLCommunication) continue; //revalidation

                                countPalletInPSAndPST = countOfPST + (hasPalletOnPS ? 1 : 0);
                                if (exitEESWithPallet >= entryEESWithoutPallet
                                    && (entryEESWithoutPallet != 0 || exitEESWithPallet != 0)) continue;
                                else if (entryEESWithoutPallet > countPalletInPSAndPST || isPSTEmpty)
                                {

                                    Task.Factory.StartNew(() => PutPVLByUCM(objRefPVLData));

                                }
                            }
                        }
                        Logger.WriteLogger(GlobalValues.PMS_LOG, "PVL scanning:" + objPVLData.machineCode + ":scanning finished");
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogger(GlobalValues.PMS_LOG, "StartPVLScanning:" + " Exception=" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(500);
                }

            }
        }

        public bool PutEESByPS(PSData objPSData, EESData objEESData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entered PutEESByPS : EES=" + objEESData.machineCode);
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            ErrorData objErrorData = null;
            bool hasPalletOnPs = false;
            bool success = false;
            bool psBlocked = false;
            bool eesBlocked = false;
            bool isReadyForPS = false;
            bool hasPSCommunication = false;
            try
            {

                objPSData = objPSControllerService.GetPSDetailsIncludeAisle(objEESData.aisle);
                hasPalletOnPs = objPSControllerService.IsPalletPresentOnPS(objPSData, out hasPSCommunication);
                isReadyForPS = objEESControllerService.IsEESReadyForPS(objEESData);
                
                if (objPSData.isBlocked 
                    || objPSData.status != 2 
                    || objPSData.isSwitchOff 
                    || !hasPalletOnPs
                    || !isReadyForPS
                    || !hasPSCommunication)
                    return false;

                psBlocked = CriticalSectionForPSLocking(objPSData.machineCode);
                eesBlocked = CriticalSectionForEESLocking(objEESData.machineCode);
                if (psBlocked && eesBlocked)
                {

                    objPSData.destAisle = objEESData.aisle;
                    objPSData.command = OpcTags.PS_L2_EES_PutCmd;
                    objErrorData = ConvertPSDataToErrorData(objPSData);
                    objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                    success = objPSControllerService.PSPutToEES(objPSData);
                    success = success && objPSControllerService.CheckPSCommandDone(objPSData);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exception PutEESByPS : EES=" + objEESData.machineCode + ", error=" + ex.Message);
            }
            finally
            {
                if (psBlocked)
                    objPSControllerService.UpdateMachineBlockStatus(objPSData.machineCode, false);
                if (eesBlocked)
                    objEESControllerService.UpdateMachineBlockStatusForPMS(objEESData.machineCode, false);
                if(psBlocked && eesBlocked)
                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting PutEESByPS : EES=" + objEESData.machineCode 
                    + ", PS=" + objPSData.machineCode +", psBlocked=" + psBlocked + ", eesBlocked=" + eesBlocked);
            }
            return success;
        }

        public bool GetEESByPS(PSData objPSData, EESData objEESData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entered GetEESByPS : EES=" + objEESData.machineCode);
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            ErrorData objErrorData = null;
            bool hasPalletOnPs = false;
            bool success = false;
            bool psBlocked = false;
            bool eesBlocked = false;
            bool isReadyForPS = false;
            bool hasPSCommunication = false;
            try
            {

                objPSData = objPSControllerService.GetPSDetailsIncludeAisle(objEESData.aisle);
                hasPalletOnPs = objPSControllerService.IsPalletPresentOnPS(objPSData, out hasPSCommunication);
                isReadyForPS = objEESControllerService.IsEESReadyForPS(objEESData);
                if (objPSData.isBlocked 
                    || objPSData.status != 2 
                    || objPSData.isSwitchOff 
                    || hasPalletOnPs
                    ||!isReadyForPS
                    || !hasPSCommunication)
                    return false;
                psBlocked = CriticalSectionForPSLocking(objPSData.machineCode);
                eesBlocked = CriticalSectionForEESLocking(objEESData.machineCode);
                if (psBlocked && eesBlocked)
                {
                    objPSData.destAisle = objEESData.aisle;
                    objPSData.command = OpcTags.PS_L2_EES_GetCmd;
                    objErrorData = ConvertPSDataToErrorData(objPSData);
                    objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                    success = objPSControllerService.PSGetFromEES(objPSData);
                    success = success && objPSControllerService.CheckPSCommandDone(objPSData);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exception GetEESByPS : EES=" + objEESData.machineCode + ", error=" + ex.Message);
            }
            finally
            {
                if (psBlocked)
                    objPSControllerService.UpdateMachineBlockStatus(objPSData.machineCode, false);
                if (eesBlocked)
                    objEESControllerService.UpdateMachineBlockStatusForPMS(objEESData.machineCode, false);
                if (psBlocked && eesBlocked)
                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting GetEESByPS : EES=" + objEESData.machineCode 
                    +" PS=" + objPSData.machineCode +", psBlocked=" + psBlocked + ", eesBlocked=" + eesBlocked);
            }
            return success;
        }

        public bool PutPSTByPS(PSData objPSData, PSTData objPSTData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entered PutPSTByPS : PS=" + objPSData.machineCode);
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objPSTControllerService == null) objPSTControllerService = new PSTControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            ErrorData objErrorData = null;
            bool success = false;
            bool psBlocked = false;
            bool pstBlocked = false;
            try
            {
                psBlocked=CriticalSectionForPSLocking(objPSData.machineCode);
                pstBlocked=CriticalSectionForPSTLocking(objPSTData.machineCode);
                if (psBlocked && pstBlocked)
                {
                    objPSData.destAisle = objPSTData.aisle;
                    objPSData.command = OpcTags.PS_L2_PST_PutCmd;
                    objErrorData = ConvertPSDataToErrorData(objPSData);
                    objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                    success = objPSControllerService.PSPutToPST(objPSData);
                    success = success && objPSControllerService.CheckPSCommandDone(objPSData);
                }



            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exception PutPSTByPS : PS=" + objPSData.machineCode + ", error=" + ex.Message);
            }
            finally
            {
                if (psBlocked)
                    objPSControllerService.UpdateMachineBlockStatus(objPSData.machineCode, false);
                if (pstBlocked)
                    objPSTControllerService.UpdateMachineBlockStatus(objPSTData.machineCode, false);
                if (psBlocked && pstBlocked)
                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting PutPSTByPS : PS=" + objPSData.machineCode +
                    " PST=" + objPSTData.machineCode +" , psBlocked=" + psBlocked + ", pstBlocked=" + pstBlocked);
            }
            return success;
        }

        public bool GetPSTByPS(PSData objPSData, PSTData objPSTData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entered GetPSTByPS : PS=" + objPSData.machineCode);
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objPSTControllerService == null) objPSTControllerService = new PSTControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            ErrorData objErrorData = null;
            bool success = false;
            bool psBlocked = false;
            bool pstBlocked = false;
            try
            {
                psBlocked = CriticalSectionForPSLocking(objPSData.machineCode);
                pstBlocked = CriticalSectionForPSTLocking(objPSTData.machineCode);
                if (psBlocked && pstBlocked)
                {
                    objPSData.destAisle = objPSTData.aisle;
                    objPSData.command = OpcTags.PS_L2_PST_GetCmd;
                    objErrorData = ConvertPSDataToErrorData(objPSData);
                    objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                    success = objPSControllerService.PSPutToPST(objPSData);
                    success = success && objPSControllerService.CheckPSCommandDone(objPSData);
                }



            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exception GetPSTByPS : PS=" + objPSData.machineCode + ", error=" + ex.Message);
            }
            finally
            {
                if (psBlocked)
                    objPSControllerService.UpdateMachineBlockStatus(objPSData.machineCode, false);
                if (pstBlocked)
                    objPSTControllerService.UpdateMachineBlockStatus(objPSTData.machineCode, false);
                if (psBlocked && pstBlocked)
                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting GetPSTByPS : PS=" + objPSData.machineCode +
                    " PST=" + objPSTData.machineCode + " , psBlocked=" + psBlocked + ", pstBlocked=" + pstBlocked);
            }
            return success;
        }

        public bool PutPSTByPVL(PVLData objPVLData, PSTData objPSTData, PSData objPSData)
        {
            if (objPVLControllerService == null) objPVLControllerService = new PVLControllerImp();
            if (objPSTControllerService == null) objPSTControllerService = new PSTControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            ErrorData objErrorData = null;
            bool success = false;
            bool pvlBlocked = false;
            bool pstBlocked = false;
            int checkCount = 0;
            int checkCountLimit=5;
            int psAisle = 0;
           // bool psNotCleared = false;
            
            try
            {
                pvlBlocked = CriticalSectionForPVLLocking(objPVLData.machineCode);
                pstBlocked = CriticalSectionForPSTLocking(objPSTData.machineCode);
                if (pvlBlocked && pstBlocked)
                {
                    psAisle = objPSControllerService.GetAisleOfPS(objPSData);
                    while (psAisle == objPSTData.aisle)
                    {
                        
                        if (checkCount > checkCountLimit) break;
                        ClearPSFromUnderPST(objPSData);
                        Thread.Sleep(500);
                        checkCount++;
                        psAisle = objPSControllerService.GetAisleOfPS(objPSData);
                    } 
                    if (!(checkCount > checkCountLimit) && psAisle!=0)
                    {
                        objPVLData.command = OpcTags.PVL_Put_PB;
                        objErrorData = ConvertPVLDataToErrorData(objPVLData);
                        objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                        success = objPVLControllerService.PVLPut(objPVLData);
                        success = success && objPVLControllerService.CheckPVLCommandDone(objPVLData);
                    }
                   
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "PutPSTByPVL:" + " Exception=" + ex.Message);
            }
            finally
            {
                if (pvlBlocked)
                    objPVLControllerService.UpdateMachineBlockStatus(objPVLData.machineCode, false);
                if (pstBlocked)
                    objPSTControllerService.UpdateMachineBlockStatus(objPSTData.machineCode, false);
                if (pvlBlocked && pstBlocked && objErrorData != null)
                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
            }
            return success;
        }

        public bool GetPSTByPVL(PVLData objPVLData, PSTData objPSTData, PSData objPSData)
        {
            if (objPVLControllerService == null) objPVLControllerService = new PVLControllerImp();
            if (objPSTControllerService == null) objPSTControllerService = new PSTControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            ErrorData objErrorData = null;
            bool success = false;
            bool pvlBlocked = false;
            bool pstBlocked = false;
            int checkCount = 0;
            int checkCountLimit = 5;
            int psAisle = 0;

            try
            {
                pvlBlocked = CriticalSectionForPVLLocking(objPVLData.machineCode);
                pstBlocked = CriticalSectionForPSTLocking(objPSTData.machineCode);
                if (pvlBlocked && pstBlocked)
                {

                    psAisle = objPSControllerService.GetAisleOfPS(objPSData);
                    while (psAisle == objPSTData.aisle)
                    {

                        if (checkCount > checkCountLimit) break;
                        ClearPSFromUnderPST(objPSData);
                        Thread.Sleep(500);
                        checkCount++;
                        psAisle = objPSControllerService.GetAisleOfPS(objPSData);
                    } 
                    if (!(checkCount > checkCountLimit) && psAisle!=0)
                    {
                        objPVLData.command = OpcTags.PVL_Get_PB;
                        objErrorData = ConvertPVLDataToErrorData(objPVLData);
                        objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                        success = objPVLControllerService.PVLGet(objPVLData);
                        success = success && objPVLControllerService.CheckPVLCommandDone(objPVLData);
                    }

                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "GetPSTByPVL:" + " Exception=" + ex.Message);
            }
            finally
            {
                if (pvlBlocked)
                    objPVLControllerService.UpdateMachineBlockStatus(objPVLData.machineCode, false);
                if (pstBlocked)
                    objPSTControllerService.UpdateMachineBlockStatus(objPSTData.machineCode, false);
                if (pvlBlocked && pstBlocked && objErrorData!=null)
                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
            }
            return success;
        }
        public bool PutPVLByUCM(PVLData objPVLData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entered PutPVLByUCM : PVL=" + objPVLData.machineCode);
            if (objPVLControllerService == null) 
                objPVLControllerService = new PVLControllerImp();
            
            if (objPVLManagerService == null)
                objPVLManagerService = new PVLManagerImp();
            if (objPVLDaoService == null)
                objPVLDaoService = new PVLDaoImp();
            objPVLData.isStore = false;
           
            bool success = false;
            bool pvlBlocked = false;
           
            try
            {
                pvlBlocked = CriticalSectionForPVLLocking(objPVLData.machineCode);
               
                if (pvlBlocked)
                {
                    //success = objPVLManagerService.ProcessPVLRequest(objPVLData);
                    success = objPVLDaoService.InsertQueueForPVL((int)GlobalValues.REQUEST_TYPE.SlotToPVL, objPVLData.machineCode);
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exception PutPVLByUCM : PVL=" + objPVLData.machineCode + " , error= " + ex.Message);
            }
            finally
            {
                if (!success && pvlBlocked)
                    objPVLControllerService.UpdateMachineBlockStatus(objPVLData.machineCode, false);
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting PutPVLByUCM : PVL=" + objPVLData.machineCode + " , success= " + success + ", pvlBlocked " + pvlBlocked);
            }
            
            return success;
        }
        public bool GetPVLByUCM(PVLData objPVLData)
        {
            Logger.WriteLogger(GlobalValues.PMS_LOG, "Entered GetPVLByUCM : PVL=" + objPVLData.machineCode );
            if (objPVLControllerService == null) 
                objPVLControllerService = new PVLControllerImp();

            if (objPVLManagerService == null) 
                objPVLManagerService = new PVLManagerImp();
            if (objPVLDaoService == null)
                objPVLDaoService = new PVLDaoImp();

            objPVLData.isStore = true;

            bool success = false;
            bool pvlBlocked = false;

            try
            {
                pvlBlocked = CriticalSectionForPVLLocking(objPVLData.machineCode);

                if (pvlBlocked)
                {
                    //success=objPVLManagerService.ProcessPVLRequest(objPVLData);
                    success = objPVLDaoService.InsertQueueForPVL((int)GlobalValues.REQUEST_TYPE.PVLToSlot, objPVLData.machineCode);
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exception GetPVLByUCM : PVL=" + objPVLData.machineCode + " , error= " + ex.Message);
            }
            finally
            {
                if(!success && pvlBlocked)
                    objPVLControllerService.UpdateMachineBlockStatus(objPVLData.machineCode, false);
                Logger.WriteLogger(GlobalValues.PMS_LOG, "Exitting GetPVLByUCM : PVL=" + objPVLData.machineCode + " , success= " + success + ", pvlBlocked " + pvlBlocked);
            }
            
            return success;
        }
       
        
        public bool ClearPSFromUnderPST(PSData objPSData)
        {
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            if (objErrorControllerService == null) objErrorControllerService = new ErrorControllerImp();
            ErrorData objErrorData = null;
            bool success = false;
            bool psBlocked = false;
            try
            {
                psBlocked = CriticalSectionForPSLocking(objPSData.machineCode);
                if (psBlocked )
                {
                    objPSData.destAisle = objPSData.actualHome;
                    objPSData.command = OpcTags.PS_L2_MoveCmd;
                    objErrorData = ConvertPSDataToErrorData(objPSData);
                    objErrorControllerService.UpdateLiveCommandOfMachine(objErrorData);
                    success = objPSControllerService.PSMove(objPSData);
                    success = success && objPSControllerService.CheckPSCommandDone(objPSData);
                }
                


            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PMS_LOG, "ClearPSFromUnderPST:" + " Exception=" + ex.Message);
            }
            finally
            {
                if (psBlocked)
                {
                    objPSControllerService.UpdateMachineBlockStatus(objPSData.machineCode, false);

                    objErrorControllerService.UpdateLiveCommandStatusOfMachine(objErrorData.machine, true);
                }
            }
            return success;
        }

        public bool CriticalSectionForPSLocking(string machineCode)
        {
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            bool isPSLocked = true;
            bool success = false;
            lock (psLock)
            {
                isPSLocked=objPSControllerService.IsPSBlockedInDB(machineCode);
                if (!isPSLocked)
                    success=objPSControllerService.UpdateMachineBlockStatus(machineCode, true);
            }
            return success;
        }

        public bool CriticalSectionForPSTLocking(string machineCode)
        {
            if (objPSTControllerService == null) objPSTControllerService = new PSTControllerImp();
            bool isPSTLocked = true;
            bool success = false;
            lock (psLock)
            {
                isPSTLocked = objPSTControllerService.IsPSTBlockedInDB(machineCode);
                if (!isPSTLocked)
                    success = objPSTControllerService.UpdateMachineBlockStatus(machineCode, true);
            }
            return success;
        }

        public bool CriticalSectionForEESLocking(string machineCode)
        {
            if (objEESControllerService == null) objEESControllerService = new EESControllerImp();
            bool isEESLocked = true;
            bool success = false;
            lock (psLock)
            {
                isEESLocked = objEESControllerService.IsEESBlockedInDBForPMS(machineCode);
                if (!isEESLocked)
                    success = objEESControllerService.UpdateMachineBlockStatusForPMS(machineCode, true);
            }
            return success;
        }

        public bool CriticalSectionForPVLLocking(string machineCode)
        {
            if (objPVLControllerService == null) objPVLControllerService = new PVLControllerImp();
            bool isPVLLocked = true;
            bool success = false;
            lock (psLock)
            {
                isPVLLocked = objPVLControllerService.IsPVLBlockedInDB(machineCode);
                if (!isPVLLocked)
                    success = objPVLControllerService.UpdateMachineBlockStatus(machineCode, true);
            }
            return success;
        }
        public ErrorData ConvertPSDataToErrorData(PSData objPSData)
        {
            ErrorData convertErrorData = new ErrorData();
            convertErrorData.machine = objPSData.machineCode;
            convertErrorData.command = objPSData.command;
            convertErrorData.aisle = objPSData.destAisle;

            convertErrorData.done = false;
            convertErrorData.queueId = 0;
            convertErrorData.seqId = 0;

            return convertErrorData;
        }
        public ErrorData ConvertPVLDataToErrorData(PVLData objPVLData)
        {
            ErrorData convertErrorData = new ErrorData();
            convertErrorData.machine = objPVLData.machineCode;
            convertErrorData.command = objPVLData.command;
            convertErrorData.floor = objPVLData.destFloor;
            convertErrorData.queueId = objPVLData.queueId;
            convertErrorData.done = false;
            return convertErrorData;
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }


        public int GetCurrentPMSMode()
        {
            if (objPalletDaoService == null) objPalletDaoService = new PalletDaoImp();
            return objPalletDaoService.GetCurrentPMSMode();
        }
        public void HomePositionMoveTrigger()
        {
            //get all cms
            if (objPSControllerService == null) objPSControllerService = new PSControllerImp();
            List<PSData> psList = objPSControllerService.GetPSList();
            bool isCMHealthy = false;
            bool isCMIdle = false;
            if (objErrorControllerService == null) new ErrorControllerImp();

            //while (!_shouldStop)
            //{
            //}
        }
    }
}
