using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Oracle.DataAccess.Client;
using ARCPMS_ENGINE.src.mrs.Modules.Machines;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.PalletManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Controller;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.OPCConnection;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.DBCon;
using Oracle.DataAccess.Client;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace ARCPMS_ENGINE.src.mrs
{
    public class InitializeEngine
    {
        OPCDA.NET.RefreshGroup uGrp;
        int DAUpdateRate = 1;

        private volatile bool _shouldStop;
        public static event EventHandler OnToDisplayMessage;

        Thread MoveIdealCMToHomePosition = null;
        Thread MoveIdealVLCToHomePosition = null;
        Thread triggerESSModeChange = null;
        Thread triggerGetPalletForESS = null;
        
       // Thread triggerClickTransfer = null;
       // Thread triggerGeneratorMode = null;

 

        static object lockEntryKioskReader = new object();
        static object lockExitKioskReader = new object();

        static object lockExecution = new object();
        static object lockBeforeAddingToKioskQueue = new object();
        static object lockEESExecution = new object();

        static object lockKioskReader = new object();

       // public static event EventHandler TriggerOnCancellation;
      


        static List<string> lstEntryKioskXML = new List<string>();
        static List<string> lstExitKioskXML = new List<string>();

        QueueControllerService objQueueControllerService = null;
        ParkingControllerService objParkingControllerService = null;
        PalletManagerService objPalletManagerService = null;
       


        public void DoInitializeEngine(GlobalValues.engineStartMode startMode)
        {
            if (OnToDisplayMessage != null)  OnToDisplayMessage("Initialization.....", null);
           
          
            /**
             * 1. set all global variables(including path of display xml)
             * 2. update machine values to DB
             * 3. update status of car at EES to DB: this included in EES machine value updation
             * 4. call engine reset procedure
             * 5. initiate notification from opc
             * 6. initiate home position 
             * 7. initiate mode changing of ees
             * 8. initiate pallet management
             * 9. initiate click transfer listener
             * 10.initiate generator mode listener
             * 11.start machine values updation trigger
             * 12.call queue manager
             */

            try
            {

                //1. set all global variables(including path of display xml)
                SetAllGlobalVariables();

                if (objParkingControllerService == null)
                    objParkingControllerService = new ParkingControllerImp();
                if (objPalletManagerService == null)
                    objPalletManagerService = new PalletManagerImp();

                #region OPC and Oracle Initialization
                //checking opc server connection established or not
                if (OnToDisplayMessage != null)  OnToDisplayMessage("Initialization.....", null);
                OpcServer opc=null;
                while (!OpcConnection.IsOpcServerConnectionAvailable())
                {
                        if (OnToDisplayMessage != null) OnToDisplayMessage("OPC Initialization failed....." , null);
                        Thread.Sleep(1000);

                }
                if (OnToDisplayMessage != null) OnToDisplayMessage("OPC Initialization success.....", null);



                //checking database connection established or not
                OracleConnection con = null;
                do{
                    con = new DBConnection().getDBConnection();
                    if(con.State==ConnectionState.Closed)
                        if (OnToDisplayMessage != null) OnToDisplayMessage("Oracle Initialization failed.....", null);
                    Thread.Sleep(1000);
                }while(con.State==ConnectionState.Closed);
                if (OnToDisplayMessage != null) OnToDisplayMessage("Oracle Initialization success.....", null);
                #endregion

              


                //2. update machine values to DB //11.start machine values updation trigger
                #region Synchind OPC data and reset or resume engine
                if (OnToDisplayMessage != null) OnToDisplayMessage("Synching data.....", null);
                UpdateMachineValues();
                if (OnToDisplayMessage != null) OnToDisplayMessage("Synching data finished.....", null);
                Thread threadUpdateMachineStatus = new Thread(delegate()
                {
                    updateMachineValuesTimer();

                });
                threadUpdateMachineStatus.IsBackground = true;
                threadUpdateMachineStatus.Start();

                //4. call engine reset procedure
               
                if (startMode==GlobalValues.engineStartMode.restart)
                {
                    objParkingControllerService.CallResetProcedure();
                }
                else
                {
                    objParkingControllerService.CallResumeProcedure();
                }
                #endregion


                //5. initiate notification from opc 
                #region OPC Notification
                CommonServicesForMachines objCommonService;

                objCommonService = new CMControllerImp();
                objCommonService.AsynchReadSettings();



                objCommonService = new EESControllerImp();
                objCommonService.AsynchReadSettings();

                objCommonService = new PSControllerImp();
                objCommonService.AsynchReadSettings();

                ////objCommonService = new PSTControllerImp();
                ////objCommonService.AsynchReadSettings();

                ////objCommonService = new PVLControllerImp();
                ////objCommonService.AsynchReadSettings();

                objCommonService = new VLCControllerImp();
                objCommonService.AsynchReadSettings();
                if (OnToDisplayMessage != null) OnToDisplayMessage("initialized OPC notifications.....", null);
                #endregion
               
                //6. initiate home position 
                if (GlobalValues.PARKING_ENABLED)
                {
                    #region Home Position
                    MoveIdealCMToHomePosition = new Thread(delegate()
                    {
                        objParkingControllerService.HomePositionMoveTrigger();
                    });
                    MoveIdealCMToHomePosition.IsBackground = true;
                    MoveIdealCMToHomePosition.Start();

                    MoveIdealVLCToHomePosition = new Thread(delegate()
                    {
                        objParkingControllerService.VLCHomePositionMoveTrigger();
                    });
                    MoveIdealVLCToHomePosition.IsBackground = true;
                    MoveIdealVLCToHomePosition.Start();
                    #endregion
                }
                
                //  * 7. initiate mode changing of ees //* 8. initiate pallet management
                if (GlobalValues.PMS_ENABLED)
                {
                    #region PMS



                    triggerESSModeChange = new Thread(delegate()
                    {
                        objPalletManagerService.StartModeScanning();
                    });
                    triggerESSModeChange.IsBackground = true;
                    triggerESSModeChange.Start();


                    triggerGetPalletForESS = new Thread(delegate()
                    {
                        objPalletManagerService.StartPMSProcessing();
                    });
                    triggerGetPalletForESS.IsBackground = true;
                    triggerGetPalletForESS.Start();
                    #endregion
                }
                // * 9. initiate click transfer listener
                // * 10.initiate generator mode listener

                #region Listening new parking request
                // * 12.call queue manager

                objQueueControllerService = new QueueControllerImp();
                if (startMode == GlobalValues.engineStartMode.resume)
                {
                    objQueueControllerService.DoResumeEngine();
                }

                objQueueControllerService.CreateDispalyXML();
                objQueueControllerService.RequestListener();
                #endregion

                //13. iterate queueEntries
                #region PROCESS PARKING
                if (GlobalValues.PARKING_ENABLED)
                {
                   
                    InitializeParkingQueue();

                }
                #endregion
                //Delete all old images 
                #region Delete all old images


                Thread threadForDeleteImages = new Thread(delegate()
                {

                    DeleteOldRecords();


                });
                threadForDeleteImages.IsBackground = true;
                threadForDeleteImages.Name = "DeleteImages";
                threadForDeleteImages.Start();
                #endregion
                if (OnToDisplayMessage != null) OnToDisplayMessage("initialization completed succesfully.....", null);

            }
            catch (Exception errMsg)
            {
                if (OnToDisplayMessage != null) OnToDisplayMessage("Error....." + errMsg.Message, null);
                MessageBox.Show(errMsg.Message);
            }
            finally
            {
                
            }
            
             
        }
        public void AsynchReadSettings()
        {
            //initiate notification from opc 

            CommonServicesForMachines objCommonService;

            objCommonService = new CMControllerImp();
            objCommonService.AsynchReadSettings();



            objCommonService = new EESControllerImp();
            objCommonService.AsynchReadSettings();

            objCommonService = new PSControllerImp();
            objCommonService.AsynchReadSettings();

            //objCommonService = new PSTControllerImp();
            //objCommonService.AsynchReadSettings();

            //objCommonService = new PVLControllerImp();
            //objCommonService.AsynchReadSettings();

            objCommonService = new VLCControllerImp();
            objCommonService.AsynchReadSettings();

        }
        public bool SetAllGlobalVariables()
        {
            bool success = false;
            try
            {
                GlobalValues.GLOBAL_DB_CON_STRING = BasicConfig.GetXmlTextOfTag("connectionString");
                GlobalValues.GLOBAL_ENTRY_XML_PATH = BasicConfig.GetXmlTextOfTag("EntryKioskXmlPath");
                GlobalValues.GLOBAL_EXIT_XML_PATH = BasicConfig.GetXmlTextOfTag("ExitKioskXmlPath");
                GlobalValues.GLOBAL_BACKUP_XML_PATH = BasicConfig.GetXmlTextOfTag("BackupkXmlPath");

                GlobalValues.GLOBAL_DISPLAY_XML_PATH = BasicConfig.GetXmlTextOfTag("DisplayXmlPath");

                GlobalValues.REPORT_PATH = BasicConfig.GetXmlTextOfTag("ReportPath");
                GlobalValues.OPC_MACHINE_HOST = BasicConfig.GetXmlTextOfTag("opcMachineHost");
                GlobalValues.OPC_SERVER_NAME = BasicConfig.GetXmlTextOfTag("opcServerName");

                GlobalValues.CAM_HOST_SERVER = BasicConfig.GetXmlTextOfTag("CamHostServer");
                GlobalValues.CAM_OPC_SERVER_NAME = BasicConfig.GetXmlTextOfTag("CamOpcServerName");
                GlobalValues.LOG_DIR = BasicConfig.GetXmlTextOfTag("LogDir");
                GlobalValues.IMAGE_PATH = BasicConfig.GetXmlTextOfTag("ImagePath");
                GlobalValues.AUTO_REFRESH = (BasicConfig.GetXmlTextOfTag("AutoRefresh")=="true");
                
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return success;
        }
       
        public void updateMachineValuesTimer()
        {
            while (!_shouldStop)
            {
                UpdateMachineValues();
                Thread.Sleep(7000);
            }
        }
        public void UpdateMachineValues()
        {
            CommonServicesForMachines objCommonService;

            objCommonService = new CMControllerImp();
            objCommonService.UpdateMachineValues();

            objCommonService = new EESControllerImp();
            objCommonService.UpdateMachineValues();


            objCommonService = new PSControllerImp();
            objCommonService.UpdateMachineValues();

            //objCommonService = new PSTControllerImp();
            //objCommonService.UpdateMachineValues();

            //objCommonService = new PVLControllerImp();
            //objCommonService.UpdateMachineValues();

            objCommonService = new VLCControllerImp();
            objCommonService.UpdateMachineValues();

        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
        public void Dispose()
        {
            try
            {

                RequestStop();

                if (objQueueControllerService == null) objQueueControllerService = new QueueControllerImp();
                PalletManagerService objPalletManagerService = new PalletManagerImp();
                
                objQueueControllerService.Dispose();

                //if (MoveIdealCMToHomePosition != null)
                //{
                //    OPCNotification.isApplicationExitReq = true;
                //    opcnotification.Dispose();
                //    MoveIdealCMToHomePosition.Abort();
                //}
                objPalletManagerService.RequestStop();




                //if (triggerClickTransfer != null)
                //{
                //    ClickTransferManager.isApplicationExitReq = true;
                //    triggerClickTransfer.Abort();
                //}

                //if (triggerGeneratorMode != null)
                //{
                //    GeneratorMode.startGeneraterModeScan.Stop();
                //    triggerGeneratorMode.Abort();
                //}
                System.Threading.Thread.Sleep(5000);
                OpcConnection.StopOPCServer();

            }
            finally { }
        }
        public void ReinitializeOpc()
        {
            //OpcConnection.StopOPCServer();
            AsynchReadSettings();
        }
        public bool testAsynchReadSettings()
        {
            OPCDA.NET.RefreshEventHandler dch = new OPCDA.NET.RefreshEventHandler(AsynchReadListenerForCM);
            OpcServer opcServer = OpcConnection.GetOPCServerConnection();
            uGrp = new OPCDA.NET.RefreshGroup(opcServer, DAUpdateRate, dch);
            return true;
        }
        public void AsynchReadListenerForCM(object sender, OPCDA.NET.RefreshEventArguments arg)
        {
        }
        public void DeleteOldRecords()
        {
            try
            {
                int i = 0;
                //string[] dirs = Directory.GetDirectories(GlobalValues.IMAGE_PATH);
                DirectoryInfo di = new DirectoryInfo(GlobalValues.IMAGE_PATH);
                var files = di.GetFiles();
                foreach (var file in files)
                {
                    i++;


                    if (file.LastAccessTime < DateTime.Now.AddMonths(-12))
                    {
                        file.Delete();
                        if (i > 2000)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void InitializeParkingQueue()
        {
            #region PROCESS PARKING
            Thread threadProcessQueue = new Thread(delegate()
            {

                objParkingControllerService.ProcessQueue();


            });
            threadProcessQueue.IsBackground = true;
            threadProcessQueue.Name = "processQueue";
            threadProcessQueue.Start();

            //ProcessQueueDefault
            Thread threadProcessQueueDefault = new Thread(delegate()
            {

                objParkingControllerService.ProcessQueueDefault();


            });
            threadProcessQueueDefault.IsBackground = true;
            threadProcessQueueDefault.Name = "ProcessQueueDefault";
            threadProcessQueueDefault.Start();

            //ProcessQueueForEES
            Thread threadProcessQueueForEES = new Thread(delegate()
            {

                objParkingControllerService.ProcessQueueForEES();


            });
            threadProcessQueueForEES.IsBackground = true;
            threadProcessQueueForEES.Name = "ProcessQueueForEES";
            threadProcessQueueForEES.Start();

            //ProcessQueueForLCM
            Thread threadProcessQueueForLCM = new Thread(delegate()
            {

                objParkingControllerService.ProcessQueueForLCM();


            });
            threadProcessQueueForLCM.IsBackground = true;
            threadProcessQueueForLCM.Name = "ProcessQueueForLCM";
            threadProcessQueueForLCM.Start();

            //ProcessQueueForUCM
            Thread threadProcessQueueForUCM = new Thread(delegate()
            {

                objParkingControllerService.ProcessQueueForUCM();


            });
            threadProcessQueueForUCM.IsBackground = true;
            threadProcessQueueForUCM.Name = "ProcessQueueForUCM";
            threadProcessQueueForUCM.Start();

            //ProcessQueueForVLC
            Thread threadProcessQueueForVLC = new Thread(delegate()
            {

                objParkingControllerService.ProcessQueueForVLC();


            });
            threadProcessQueueForVLC.IsBackground = true;
            threadProcessQueueForVLC.Name = "ProcessQueueForVLC";
            threadProcessQueueForVLC.Start();

            #endregion
        }
    }
}
