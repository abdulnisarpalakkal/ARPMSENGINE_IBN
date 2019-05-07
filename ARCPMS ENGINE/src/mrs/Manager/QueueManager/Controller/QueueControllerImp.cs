using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.DB;
using ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Model;
using ARCPMS_ENGINE.src.mrs.Global;
using System.IO;
using System.Xml;
using System.Threading;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Controller;
using System.Threading.Tasks;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Controller;
using ARCPMS_ENGINE.src.mrs.Manager.ClickTransferManager.Controller;
using ARCPMS_ENGINE.src.mrs.Global;
using ARCPMS_ENGINE.src.mrs.Config;
using OPC;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp;
using ARCPMS_ENGINE.src.mrs.OPCOperations;

using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Controller;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.VLC.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.DB;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;

namespace ARCPMS_ENGINE.src.mrs.Manager.QueueManager.Controller
{
    class QueueControllerImp: QueueControllerService
    {
        public static event EventHandler OnToDisplayMessage;

        static System.Timers.Timer entryKioskReader = new System.Timers.Timer();
        static System.Timers.Timer exitKioskReader = new System.Timers.Timer();
        static System.Timers.Timer queueEntryReader = new System.Timers.Timer();
        static System.Timers.Timer queueCancelReader = new System.Timers.Timer();
        static System.Timers.Timer clickTransferReader = new System.Timers.Timer();
        

        static object lockEntryKioskReader = new object();
        static object lockExitKioskReader = new object();
        static object lockQueueEntryReader = new object();
        static object lockqueueCancelReader = new object();
        static object lockClickTransferReader = new object();
       

        EESControllerService objEESControllerService = null;
        QueueDaoService objQueueDaoService = null;
        ParkingControllerService objParkingControllerService = null;
        ClickTransferService objClickTransferService = null;
      
        CMControllerService objCMControllerService = null;
        VLCControllerService objVLCControllerService = null;
        EESDaoService objEESDaoService = null;

        public void RequestListener()
        {
            //entryKioskReader.Start();
            //exitKioskReader.Start();
            queueEntryReader.Start();
            queueCancelReader.Start();
          

            //entryKioskReader.Interval = 2000;
            //exitKioskReader.Interval = 1000;
            queueEntryReader.Interval = 1000;
            queueCancelReader.Interval = 1000;
           



            ////Continues check for entry kiosk input data.
            //entryKioskReader.Elapsed += (s, e) =>
            //{
            //    lock (lockEntryKioskReader)
            //    {
            //        ReadEntryKioskData();
            //    }
            //};

            ////Continues check for exit kiosk data.
            //exitKioskReader.Elapsed += (s, e) =>
            //{
            //    lock (lockExitKioskReader)
            //    {
            //        ReadExitKioskData();
            //    }
            //};

           // Continues check L2_EES_QUEUE for new requests.
            queueEntryReader.Elapsed += (s, e) =>
            {
                lock (lockQueueEntryReader)
                {
                    ReadQueueEntryData();
                }
            };

           //  Continues check L2_EES_QUEUE for new cancel request .
            queueCancelReader.Elapsed += (s, e) =>
            {
                lock (lockqueueCancelReader)
                {
                    ReadQueueCancelData();
                }
            };


        }

       


        public void ReadEntryKioskData()
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            try
            {
                entryKioskReader.Stop();

               

                if (Directory.Exists(GlobalValues.GLOBAL_ENTRY_XML_PATH) == false)
                    return;

                string[] xmlfiles = Directory.GetFiles(GlobalValues.GLOBAL_ENTRY_XML_PATH + @"\", "*.xml");

                XmlDocument xml = new XmlDocument();
                string xmlFile = "";

                //if no input kiosk found return.
                if (xmlfiles == null || xmlfiles.Length < 1) return;

                for (int i = 0; i < xmlfiles.Length; i++)
                {
                    

                    xmlFile = xmlfiles[i];
                    FileInfo f = new FileInfo(xmlFile);

                    if (f.IsReadOnly)
                    {
                        
                        continue;
                    }

                    string newFileName = "Entry_" + System.DateTime.Now.ToFileTime().ToString();

                    if (Directory.Exists(GlobalValues.GLOBAL_BACKUP_XML_PATH + @"\"))
                    {
                        f.CopyTo(GlobalValues.GLOBAL_BACKUP_XML_PATH + @"\" + newFileName + ".xml");
                    }

                    xml.Load(xmlFile);

                    XmlNodeList xnList = xml.SelectNodes("/Patron");

                    //Iterate OPC XML file (come from the kiosk)
                    foreach (XmlNode xn in xnList)
                    {
                        QueueData objQueueData = null;
                        objQueueData = new QueueData();

                        objQueueData.customerId = Convert.ToString(xn["card_ID"].InnerText);
                        int eesNumber = 0;
                        int.TryParse(xn["EES"].InnerText.ToString(),out eesNumber );
                        objQueueData.eesNumber = eesNumber;
                        objQueueData.patronName = Convert.ToString(xn["Name"].InnerText);
                        objQueueData.plateNumber = Convert.ToString(xn["Car_ID"].InnerText);
                        objQueueData.needWash = Convert.ToString(xn["Carwash"].InnerText) == "True" ? true :false;

                       // objEESControllerService = new EESControllerImp();
                       // objQueueData.isHighCar = objEESControllerService.IsHighCar(objQueueData.eesNumber) ? 2 : 1;
                        objQueueData.isEntry = true;
                        objQueueData.requestType = objQueueData.isEntry ? 1:2;
                        
                        objQueueData.procStartTime = System.DateTime.Now;


                        objQueueData = objQueueDaoService.InsertQueue(objQueueData);
                        if (OnToDisplayMessage != null)
                            OnToDisplayMessage("Entry request received from " + objQueueData.eesNumber + " with card ID " + objQueueData.customerId, null);

                        if (File.Exists(xmlFile) == true) File.Delete(xmlFile);

                        //CancellationTokenSource tokenSource = new CancellationTokenSource();
                        //GlobalValues.threadsDictionary.Add(objQueueData.queuePkId, tokenSource);
                        //if (OnToDisplayMessage != null)
                        //    OnToDisplayMessage("Entry request recieved from EES" + objQueueData.eesNumber + " with card ID " + objQueueData.customerId, null);
                        //Task.Factory.StartNew(() => objParkingControllerService.EntryCarProcessing(objQueueData), GlobalValues.threadsDictionary[objQueueData.queuePkId].Token);
                        
                    }
                    // }

                    //if (File.Exists(filePath) == true)
                    //    File.Delete(filePath);
                }// for loop end

            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, ":--Exception 'ReadEntryKioskData ':: " + errMsg.Message);
                //Console.WriteLine(errMsg.Message);
              
            }
            finally
            {
                entryKioskReader.Interval = 2000;
                entryKioskReader.Start();
            }
        }

        public void ReadExitKioskData()
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            try
            {
                exitKioskReader.Stop();

                

                if (Directory.Exists(GlobalValues.GLOBAL_EXIT_XML_PATH) == false)
                    return;

                string[] xmlfiles = Directory.GetFiles(GlobalValues.GLOBAL_EXIT_XML_PATH + @"\", "*.xml");

                XmlDocument xml = new XmlDocument();
                string xmlFile = "";

                //if no input kiosk found return.
                if (xmlfiles == null || xmlfiles.Length < 1) return;

                for (int i = 0; i < xmlfiles.Length; i++)
                {
                    xmlFile = xmlfiles[i];
                    FileInfo f = new FileInfo(xmlFile);

                    if (f.IsReadOnly)
                    {

                        continue;
                    }

                    string newFileName = "Exit_" + System.DateTime.Now.ToFileTime().ToString();

                    if (Directory.Exists(GlobalValues.GLOBAL_BACKUP_XML_PATH + @"\"))
                    {
                        f.CopyTo(GlobalValues.GLOBAL_BACKUP_XML_PATH + @"\" + newFileName + ".xml");
                    }

                    xml.Load(xmlFile);

                    
                    XmlNodeList xnList = xml.SelectNodes("/OPCXMLData/OPCTagNode");

                    //Iterate OPC XML file (come from the kiosk)
                    foreach (XmlNode xn in xnList)
                    {
                        QueueData objQueueData = null;
                        objQueueData = new QueueData();

                        objQueueData.customerId = Convert.ToString(xn["Card_ID"].InnerText);
                       
                        objQueueData.isEntry = false;
                        objQueueData.requestType = objQueueData.isEntry ? 1 : 0;

                        if (xn["Location"] != null)
                            objQueueData.kioskId = Convert.ToString(xn["Location"].InnerText);
                        else
                        {
                            objQueueData.kioskId = "SMS";
                            objQueueData.retrievalType = 1;
                        }
                        objQueueData.procStartTime = System.DateTime.Now;



                        objQueueData = objQueueDaoService.InsertQueue(objQueueData);
                        if (OnToDisplayMessage != null)
                            OnToDisplayMessage("Exit request received  with card ID " + objQueueData.customerId, null);

                        if (File.Exists(xmlFile) == true) File.Delete(xmlFile);
                        //if (objQueueData.queuePkId != 0)
                        //{
                        //    CancellationTokenSource tokenSource = new CancellationTokenSource();
                        //    GlobalValues.threadsDictionary.Add(objQueueData.queuePkId, tokenSource);
                        //    if (OnToDisplayMessage != null)
                        //        OnToDisplayMessage("Exit request recieved  with card ID " + objQueueData.customerId, null);
                        //    Task.Factory.StartNew(() => objParkingControllerService.ExitCarProcessing(objQueueData), GlobalValues.threadsDictionary[objQueueData.queuePkId].Token);
                        //}
                    }
                   
                }// for loop end

            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, ":--Exception 'ReadExitKioskData ':: " + errMsg.Message);

            }
            finally
            {
                exitKioskReader.Interval = 2000;
                exitKioskReader.Start();
            }
        }
        public void ReadQueueEntryData()
        {
            
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            if (objParkingControllerService == null) objParkingControllerService = new ParkingControllerImp();
            if (objClickTransferService == null) objClickTransferService = new ClickTransferImp();
           
            int queueId=0;
            try
            {
                
                queueEntryReader.Stop();
                queueId = objQueueDaoService.GetPendingQueueDataForProcessing();
                if (queueId != 0)
                {
                    Model.QueueData objQueueData = new Model.QueueData();
                    objQueueData = objQueueDaoService.GetQueueData(queueId);

                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                   

                    if (objQueueData.requestType == 1 )
                    {
                        //GlobalValues.threadsDictionary.Add(-1 * queueId, tokenSource);
                        //if (OnToDisplayMessage != null)
                        //    OnToDisplayMessage("Entry request received from " + objQueueData.gate + " with card ID " + objQueueData.customerId, null);
                        Task.Factory.StartNew(() => objParkingControllerService.EntryCarProcessing(objQueueData), tokenSource.Token);
                    }
                    else if (objQueueData.requestType == 2)
                    {
                        //GlobalValues.threadsDictionary.Add(-1 * queueId, tokenSource);
                        //if (OnToDisplayMessage != null)
                        //    OnToDisplayMessage("Exit request received  with card ID " + objQueueData.customerId, null);
                        Task.Factory.StartNew(() => objParkingControllerService.ExitCarProcessing(objQueueData), tokenSource.Token);
                    }
                   
                    else if (objQueueData.requestType == 3 || objQueueData.requestType == 4
                        || objQueueData.requestType == 7 || objQueueData.requestType == 8)
                    {
                      //  GlobalValues.threadsDictionary.Add( queueId, tokenSource);
                        //if (OnToDisplayMessage != null)
                        //    OnToDisplayMessage("Transfer request received  with card ID " + objQueueData.customerId, null);
                        //Task.Factory.StartNew(() => objCarWashControllerService.ProcessWashPath(objQueueData), GlobalValues.threadsDictionary[queueId].Token);
                        objParkingControllerService.AddRequestIntoQueue(objQueueData);
                    }
                   
                    objQueueDaoService.SetQueueStatus(queueId,2);
                   
                }

            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, ":--Exception 'ReadQueueEntryData ':: " + errMsg.Message);

            }
            finally
            {
                removeFinishedThread();
                queueEntryReader.Interval = 3000;
                queueEntryReader.Start();
            }
        }
        public void ReadQueueCancelData()
        {
           // Model.QueueData objQueueData = new Model.QueueData();
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            List<int> abortedList=new List<int>();
            
            try
            {

                queueCancelReader.Stop();
                abortedList = objQueueDaoService.GetCancelledQueueId();
                foreach (int queueId in abortedList)
                {
                    if (queueId != 0)
                    {
                        GlobalValues.threadsDictionary[queueId].Cancel();
                     
                        //once thread will catch the cancellation request, then it will call the delete or complete transaction procedrue and remove 
                        //the entry from threadsDictionary
                    }
                }

            }
            catch (Exception errMsg)
            {
                //Console.WriteLine(errMsg.Message);

            }
            finally
            {
                queueCancelReader.Interval = 3000;
                queueCancelReader.Start();
            }
        }
        public void removeFinishedThread()
        {
            List<Model.QueueData> processingQList = new List<Model.QueueData>();
            processingQList = objQueueDaoService.GetAllProcessingQId();
            bool isFound = false;
            List<int> qRemoveList = new List<int>();

            foreach (KeyValuePair<int, CancellationTokenSource> pair in GlobalValues.threadsDictionary)
            {
                isFound = false;
                foreach (Model.QueueData objQueueData in processingQList)
                {
                    if (pair.Key == objQueueData.queuePkId)
                    {
                        isFound = true;
                        break;
                    }
               
                }
                if (!isFound)
                    qRemoveList.Add(pair.Key);

            }
            foreach (int removeQueue in qRemoveList)
            {
                GlobalValues.threadsDictionary.Remove(removeQueue);
            }
            
        }
        public bool UpdateAbortedStatus(int queueId)
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            return objQueueDaoService.UpdateAbortedStatus(queueId);
        }
        public void CancelIfRequested(int queueId)
        {

            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            try
            {
                if (GlobalValues.threadsDictionary[queueId].Token.IsCancellationRequested)
                {

                    GlobalValues.threadsDictionary[queueId].Token.ThrowIfCancellationRequested();

                }

                //hold & resume logic block
                if (objQueueDaoService.GetHoldRequestFlagStatus(queueId))
                {
                    objQueueDaoService.SetHoldFlagStatus(queueId, true);
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "CancelIfRequested(): Queue Id:" + queueId + ":--transaction Paused");
                    do
                    {
                        if (GlobalValues.threadsDictionary[queueId].Token.IsCancellationRequested)
                        {

                            GlobalValues.threadsDictionary[queueId].Token.ThrowIfCancellationRequested();

                        }
                        if (!objQueueDaoService.GetHoldRequestFlagStatus(queueId))
                            objQueueDaoService.SetHoldFlagStatus(queueId, false);
                        Thread.Sleep(1000);
                    } while (objQueueDaoService.GetHoldFlagStatus(queueId));
                    Logger.WriteLogger(GlobalValues.PARKING_LOG, "CancelIfRequested(): Queue Id:" + queueId + ":--transaction Resumed");
                }

            }
            catch (OperationCanceledException errMsg)
            {
                

                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " --TaskCanceledException 'CancelIfRequested':: " + errMsg.Message);
            
                objQueueDaoService.UpdateAbortedStatus(queueId); // if this called first before cancel the thread
                                                                 // there is a chance to remove this queue id from
                                                                 // threadsDictionary
                CreateDispalyXML();
                
                throw new OperationCanceledException();
            }
            catch (KeyNotFoundException ex)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG,  "Queue Id:" + queueId + "KeyNotFoundException CancelIfRequested: " + ex.Message);
            }
            catch(Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + queueId + " Exception CancelIfRequested: " + ex.Message);
            }
        }
        public bool UpdateEESCarData(int queueId, ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE carType)
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            return objQueueDaoService.UpdateEESCarData(queueId, carType);
        }
      
        public void CreateDispalyXML()
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Entering CreateDispalyXML");
            if (objQueueDaoService==null) objQueueDaoService = new QueueDaoImp();
            List<DisplayData> displayList = null;
           // string releasePath = "D:\\Nizar\\PROJECTS\\Ibn Battuta\\Code\\engine\\Live Engine\\engine on 12nov14\\ARCPMS ENGINE\\ARCPMS ENGINE\\bin\\Release\\ibm_config\\test.xml";
            try
            {
                displayList=objQueueDaoService.GetDisplayData();
                string pXMLDataFileName = GlobalValues.GLOBAL_DISPLAY_XML_PATH;


                using (FileStream fs = new FileStream(@pXMLDataFileName, FileMode.Create))
                {

                    using (XmlTextWriter w = new XmlTextWriter(fs, null))
                    {

                        w.Formatting = Formatting.Indented;
                        w.Indentation = 5;

                        w.WriteStartDocument();
                        w.WriteStartElement("ODBCXMLData");


                        foreach (DisplayData objDisplayData in displayList)
                        {
                           
                            w.WriteStartElement("ODBCTagRow");

                            w.WriteAttributeString("Car_ID", objDisplayData.cardId);
                            w.WriteAttributeString("Patron_Name", objDisplayData.patronName);
                            if (objDisplayData.gateNumber != 0)
                                w.WriteAttributeString("REES_Num", Convert.ToString(objDisplayData.gateNumber));
                            else
                                w.WriteAttributeString("REES_Num", "");
                            w.WriteAttributeString("EntryTime", objDisplayData.EntryTime.ToString("dd/MMM/yy hh:mm"));
                            w.WriteAttributeString("ExitTime", objDisplayData.ExitTime.ToString("dd/MMM/yy hh:mm"));
                           

                            w.WriteEndElement();//ODBCTagRow
                        }
                        w.WriteEndElement();//ODBCXMLData
                       // w.Close();

                    }
                }
                
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Error in  CreateDispalyXML: "+errMsg.Message);
            }
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Exitting CreateDispalyXML");
        }
      
        public Model.QueueData GetQueueData(int queueId)
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            return objQueueDaoService.GetQueueData(queueId);
        }
        public bool NeedToOptimizePath(int queueId)
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            return objQueueDaoService.NeedToOptimizePath(queueId);
        }
        public bool Dispose()
        {
            entryKioskReader.Stop();
            exitKioskReader.Stop();
            queueEntryReader.Stop();
            queueCancelReader.Stop();
           
            return true;
        }
        public void SetTransactionAbortStatus(int queueId, int abortStatus)
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            objQueueDaoService.SetTransactionAbortStatus(queueId, abortStatus);
        }
        /// <summary>
        /// setting flag for holding transactions
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="holdStatus"></param>
        public void SetHoldFlagStatus(int queueId, bool holdStatus)
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            objQueueDaoService.SetHoldFlagStatus(queueId,holdStatus);
        }
        /// <summary>
        /// get status of holding flag
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public bool GetHoldFlagStatus(int queueId)
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            return objQueueDaoService.GetHoldFlagStatus(queueId);
        }
        public bool SetReallocateData(decimal queueId, string machineCode, int reallocateFlag)
        {
            if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
            return objQueueDaoService.SetReallocateData(queueId, machineCode, reallocateFlag);
        }
        //public void DoResumeEngine()
        //{
        //    List<QueueData> processingQList = null;
        //    if (objQueueDaoService == null) objQueueDaoService = new QueueDaoImp();
        //    if (objCMControllerService == null) objCMControllerService = new CMControllerImp();
        //    if (objVLCControllerService == null) objVLCControllerService = new VLCControllerImp();

        //    //get all request in status 0 or 2
        //    processingQList = objQueueDaoService.GetAllProcessingQId();

        //    //take one request
        //    foreach (QueueData objQueueData in processingQList)
        //    {
        //        string machineCode = null;
        //        //get queue id
        //        //get all cms blocked by this queue id
        //        CMData objCMData=objCMControllerService.GetBlockedCMDetails(objQueueData.queuePkId);
        //        if(objCMData!=null)
        //        {
        //            //check pallet present of blocked machine
        //            GlobalValues.palletStatus palletStatus= objCMControllerService.GetPalletOnCMStatus(objCMData);
        //            if (palletStatus == GlobalValues.palletStatus.present)
        //                machineCode = objCMData.machineCode;

        //            //TODO: there is a chance to start transcation from ees even if ucm compl
        //        }
               
        //        if(string.IsNullOrEmpty(machineCode))
        //        {
        //            //get all vlcs blocked by this queue id
        //            VLCData objVLCData = objVLCControllerService.GetVLCDetails(objQueueData.queuePkId);
                
        //            //take blocked machine which has pallet present
        //            if (objVLCData != null)
        //                machineCode = objVLCData.machineCode;
        //        }


        //        //call SetReallocateData with pallet present machine
        //        if (!string.IsNullOrEmpty(machineCode))
        //        {
        //            SetReallocateData(objQueueData.queuePkId, machineCode, 3);
        //        }
        //        else
        //        {
        //            SetReallocateData(objQueueData.queuePkId, "SLOT", 3);// entry or exit which had no initial path 
        //        }
        //        UpdateAbortedStatus(objQueueData.queuePkId);

        //    }
        //}

        public void DoResumeEngine()
        {
            List<QueueData> processingQList = null;
            if (objQueueDaoService == null)
                objQueueDaoService = new QueueDaoImp();
            if (objCMControllerService == null) 
                objCMControllerService = new CMControllerImp();
            if (objVLCControllerService == null) 
                objVLCControllerService = new VLCControllerImp();
            if (objEESDaoService == null) 
                objEESDaoService = new EESDaoImp();
            //get all request in status 0 or 2
            processingQList = objQueueDaoService.GetAllProcessingQId();

            //take one request
            foreach (QueueData objQueueData in processingQList)
            {
                string carMachine = null;
              
                //get queue id
                //get all cms blocked by this queue id
                CMData objCMData = objCMControllerService.GetBlockedCMDetails(objQueueData.queuePkId);
               
                //get all vlcs blocked by this queue id
                VLCData objVLCData = objVLCControllerService.GetVLCDetails(objQueueData.queuePkId);

                EESData objEESData=objEESDaoService.GetBlockedEESDetails(objQueueData.queuePkId);

                if(objCMData!=null && objVLCData!=null)
                {
                    //check pallet present of blocked machine
                    GlobalValues.palletStatus palletStatus = objCMControllerService.GetPalletOnCMStatus(objCMData);
                    if (palletStatus == GlobalValues.palletStatus.present)
                        carMachine = objCMData.machineCode;
                    else
                        carMachine = objVLCData.machineCode;
                }
                else if(objCMData!=null)
                    carMachine = objCMData.machineCode;
                else if (objCMData != null)
                    carMachine = objVLCData.machineCode;
                else if (objEESData!=null)
                    carMachine = objEESData.machineCode;

                //call SetReallocateData with pallet present machine
                if (!string.IsNullOrEmpty(carMachine))
                {
                    SetReallocateData(objQueueData.queuePkId, carMachine, 3);
                }
                else
                {
                    SetReallocateData(objQueueData.queuePkId, "SLOT", 3);// entry or exit which had no initial path 
                }
                UpdateAbortedStatus(objQueueData.queuePkId);

            }
        }
    }
}
