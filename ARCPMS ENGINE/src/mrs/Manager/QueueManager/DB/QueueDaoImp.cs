using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using ARCPMS_ENGINE.src.mrs.DBCon;
using System.Data;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Global;
//using System.Data.Oracle

namespace ARCPMS_ENGINE.src.mrs.Manager.QueueManager.DB
{
    class QueueDaoImp:QueueDaoService
    {
        private object lockInserQueue = new object();
        public Model.QueueData InsertQueue(Model.QueueData objQueueData)
        {
            var queueId = 0;
            try
            {
                lock (lockInserQueue)
                {
                    using (OracleConnection con = new DBConnection().getDBConnection()) 
                    {

                        using (OracleCommand command = con.CreateCommand())
                        {

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "INSERT_QUEUE";
                            command.Parameters.Add("V_QUEUE_ID", OracleDbType.Int64, queueId, ParameterDirection.Output);
                            command.Parameters.Add("cust_id", OracleDbType.Varchar2, 50, objQueueData.customerId, ParameterDirection.Input);
                            command.Parameters.Add("ARG_EES_ID", OracleDbType.Int64, objQueueData.eesNumber, ParameterDirection.Input);
                            command.Parameters.Add("KIOSK_ID", OracleDbType.Varchar2, objQueueData.kioskId, ParameterDirection.Input);
                            command.Parameters.Add("CAR_ID", OracleDbType.Varchar2, objQueueData.plateNumber, ParameterDirection.Input);
                            command.Parameters.Add("ARG_REQUEST_TYPE", OracleDbType.Int64, objQueueData.requestType, ParameterDirection.Input);
                            command.Parameters.Add("PRIORITY", OracleDbType.Int64, 1, ParameterDirection.Input); 
                            command.Parameters.Add("high_car", OracleDbType.Int64, objQueueData.carType, ParameterDirection.Input);
                            command.Parameters.Add("STAT", OracleDbType.Int64, 0, ParameterDirection.Input);
                            command.Parameters.Add("PATRON_NAME", OracleDbType.NVarchar2, 500, objQueueData.patronName, ParameterDirection.Input);
                            command.Parameters.Add("arg_need_wash", OracleDbType.Int64, (objQueueData.needWash ? 1 : 0), ParameterDirection.Input);
                            command.Parameters.Add("RETRIEVAL_TYPE", OracleDbType.Int64, objQueueData.retrievalType, ParameterDirection.Input);
                            command.Parameters.Add("arg_rot_staus", OracleDbType.Char, 0, ParameterDirection.Input);


                            command.ExecuteNonQuery();
                            Int32.TryParse(Convert.ToString(command.Parameters["V_QUEUE_ID"].Value), out queueId);
                            objQueueData.queuePkId = queueId;
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
               
            }
            return objQueueData;
         
        }


        public bool UpdateEESCarData(int queueId,ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE carType)
        {
            bool bOk = false;
            
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "UPDATE_EES_DETAILS_IN_QUEUE";
                        command.Parameters.Add("V_QUEUE_ID", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.Parameters.Add("ARG_CAR_TYPE", OracleDbType.Int32, carType, ParameterDirection.Input);
                        command.ExecuteNonQuery();
                        bOk = true;
                    }
                }
                
            }
            catch (Exception errMsg)
            {
                //Console.WriteLine(errMsg.Message);

            }
           
         
            return bOk;
        }

        public int  GetPendingQueueDataForProcessing()
        {
            int queueId = 0;

            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "GET_PENDING_REQUEST_IN_QUEUE";
                        command.Parameters.Add("V_QUEUE_ID", OracleDbType.Int64, queueId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        Int32.TryParse(Convert.ToString(command.Parameters["V_QUEUE_ID"].Value), out queueId);
                    }
                }

            }
            catch (Exception errMsg)
            {
                //Console.WriteLine(errMsg.Message);
            }

            return queueId;
        }
      
        public Model.QueueData GetQueueData(int queueId)
        {

            Model.QueueData objQueueData = new Model.QueueData();

            try
            {
                string sql = "SELECT Q.ID,Q.EES_NAME,Q.KIOSK_ID,Q.CUSTOMER_ID,Q.REQUEST_TYPE,Q.PRIORITY,Q.STATUS,Q.PROC_START_TIME,Q.PROC_END_TIME"
                                + " ,Q.ASSIGNED_THREAD_ID,Q.IS_HIGH_CAR,Q.GATE,cust.card_id,QUEUE_HANDLING_PACKAGE.get_machine_search_flag(Q.ID) machine_search_flag "
                                + " FROM L2_EES_QUEUE Q LEFT JOIN  L2_CUSTOMERS CUST ON q.customer_id=cust.customer_id   WHERE  ID=" + queueId;


                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    using (OracleCommand command = con.CreateCommand())
                    {
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {

                                while (reader.Read())
                                {
                                    objQueueData.queuePkId = Int32.Parse(reader["ID"].ToString());
                                    objQueueData.eesCode = reader["EES_NAME"].ToString();
                                    objQueueData.kioskId = reader["KIOSK_ID"].ToString();
                                    int temp = 0;
                                    if (int.TryParse(reader["CUSTOMER_ID"].ToString(),out temp))
                                         objQueueData.customerPkId =temp;
                                    objQueueData.customerId = reader["card_id"].ToString();
                                    objQueueData.requestType = Int32.Parse(reader["REQUEST_TYPE"].ToString());
                                    objQueueData.priority = Int32.Parse(reader["PRIORITY"].ToString());

                                    objQueueData.status = Int32.Parse(reader["STATUS"].ToString());

                                    objQueueData.carType = (ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE)Int32.Parse(reader["IS_HIGH_CAR"].ToString());
                                    objQueueData.gate = reader["GATE"].ToString();
                                    objQueueData.isEntry = objQueueData.requestType == 1 ? true : false;
                                    objQueueData.MachineSearchFlag = int.Parse(reader["machine_search_flag"].ToString());

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg);
            }
            return objQueueData;
        }
       

        public List<Model.QueueData> GetAllProcessingQId()
        {

            List<Model.QueueData> processingQList = new List<Model.QueueData>();

            try
            {
                string sql = "SELECT Q.ID FROM L2_EES_QUEUE Q    WHERE  STATUS =2" ;


                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    using (OracleCommand command = con.CreateCommand())
                    {
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {

                                while (reader.Read())
                                {
                                    Model.QueueData objQueueData=new Model.QueueData();
                                    objQueueData.queuePkId = Int32.Parse(reader["ID"].ToString());
                                    
                                    processingQList.Add(objQueueData);

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg);
            }
            return processingQList;
        }
        public List<int> GetCancelledQueueId()
        {
            List<int> abortedList = new List<int>();
            try
            {
                
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    
                    string sql = "SELECT id FROM L2_EES_QUEUE WHERE CANCELREQTYPE !=0 and IS_ABORTED=0 and status!=1";
                    
                    using (OracleCommand command = con.CreateCommand())
                    {
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {

                                while (reader.Read())
                                {
                                   
                                    abortedList.Add(Int32.Parse(reader["ID"].ToString()));

                                }
                            }
                        }
                    }
                   
                }
            }
            catch (Exception errMsg)
            {
               
            }
            finally
            {
            }

            return abortedList;
        }
        public int GetCancelledStatus(int queueId)
        {
            int status = 0;


            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "select CANCELREQTYPE from   l2_ees_queue  where id = " + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    status = Convert.ToInt32(command.ExecuteScalar());
                }

            }
            finally
            {

            }
            return status;
        }


        public bool UpdateAbortedStatus(int queueId)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "UpdateAbortedStatus(): Queue Id:" + queueId + ":--Entered");
            bool success=false;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "CONFIG_PACKAGE.abort_transaction";
                        command.Parameters.Add("V_QUEUE_ID", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.ExecuteNonQuery();
                        success = true;
                    }
                }

            }
            catch (Exception errMsg)
            {

                Logger.WriteLogger(GlobalValues.PARKING_LOG, "UpdateAbortedStatus(): Queue Id:" + queueId + ":--Exception : " + errMsg.Message);

            }
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "UpdateAbortedStatus(): Queue Id:" + queueId + ":--Exitting");
            return success;
        }


        public List<Model.DisplayData> GetDisplayData()
        {
            List<Model.DisplayData> displayDataList = new List<Model.DisplayData>();
            DateTime tempTime = System.DateTime.Now;

            try
            {
                string sql = "SELECT * FROM DISPLAYBOARD_VIEW";


                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    using (OracleCommand command = con.CreateCommand())
                    {
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {

                                while (reader.Read())
                                {
                                    Model.DisplayData objDisplayData = new Model.DisplayData();
                                    objDisplayData.queuePkId = Int32.Parse(reader["TRANS ID"].ToString());
                                    objDisplayData.patronName = reader["PATRON_NAME"].ToString();
                                    objDisplayData.gateNumber = Int32.Parse(reader["GATE"].ToString());
                                    objDisplayData.cardId = reader["CARD_ID"].ToString();

                                    DateTime.TryParse(Convert.ToString(reader["ENTRY_TIME"]), out tempTime);
                                    objDisplayData.EntryTime = tempTime;
                                    DateTime.TryParse(Convert.ToString(reader["EXIT_TIME"]), out tempTime);
                                    objDisplayData.ExitTime = tempTime;

                                    displayDataList.Add(objDisplayData);


                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg);
            }
            return displayDataList;
        }



        public bool NeedToOptimizePath(int queueId)
        {
            bool status = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select count(need_optimize_path) from l2_ees_queue"
                        + " where (need_optimize_path=1 or NEED_OPTIMIZE_SLOT=1) and  id=" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    status = Convert.ToInt16(command.ExecuteScalar())>0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                status = false;
            }
            finally
            {

            }
            return status;
        }
        public bool SetQueueStatus(int queueId, int processStatus)
        {
            bool bOk = false;


            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update l2_ees_queue set status =" + processStatus + " where id = " + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bOk = command.ExecuteNonQuery() > 0;
                }

            }
            finally
            {

            }
            return bOk;
        }
        public int GetQueueStatus(int queueId)
        {
            int  status = 0;


            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "select status from   l2_ees_queue  where id = " + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    
                    status = Convert.ToInt32(command.ExecuteScalar());
                }

            }
            finally
            {

            }
            return status;
        }
        public void SetTransactionAbortStatus(int queueId, int abortStatus)
        {
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_EES_QUEUE set CANCELREQTYPE=" + abortStatus + "  where id=" + queueId + " and CANCELREQTYPE !=" + abortStatus;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                }
            }
            finally
            {

            }
        }
        /// <summary>
        /// setting flag for holding transactions
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="holdStatus"></param>
        public void SetHoldFlagStatus(int queueId, bool holdStatus)
        {
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_EES_QUEUE set HOLD_FLAG=" + (holdStatus ? 1 : 0) + "  where id=" + queueId + " and HOLD_FLAG !=" + (holdStatus ? 1 : 0);
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                }
            }
            catch (Exception errMsg)
            {
               
            }
            finally
            {

            }
        }
        /// <summary>
        /// get status of holding flag
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public bool GetHoldFlagStatus(int queueId)
        {
            bool holdStatus = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT HOLD_FLAG FROM L2_EES_QUEUE WHERE id =" + queueId ;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    holdStatus = bResult == 1;
                }
            }
            catch (Exception errMsg)
            {
            }
            finally
            {
            }
            return holdStatus;
        }

        public bool SetReallocateData(decimal queueId, string machineCode, int reallocateFlag)
        {
            bool bOk = false;


            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                  
                    OracleCommand command = con.CreateCommand();
                    string sql = "update l2_ees_queue set CANCELREQTYPE = '" + reallocateFlag
                        + "',PATH_START_MACHINE='" + machineCode
                        + "' where ID =" + queueId;

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bOk = command.ExecuteNonQuery() > 0;
                }

            }
            finally
            {

            }
            return bOk;
        }
        public bool GetHoldRequestFlagStatus(int queueId)
        {
            bool holdStatus = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT HOLD_REQ_FLAG FROM L2_EES_QUEUE WHERE id =" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    holdStatus = bResult == 1;
                }
            }
            catch (Exception errMsg)
            {
            }
            finally
            {
            }
            return holdStatus;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="holdStatus"></param>
        public void SetHoldReqFlagStatus(int queueId, bool holdReqStatus)
        {
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_EES_QUEUE set HOLD_REQ_FLAG=" + (holdReqStatus ? 1 : 0) + "  where id=" + queueId + " and HOLD_REQ_FLAG !=" + (holdReqStatus ? 1 : 0);
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                }
            }
            catch (Exception errMsg)
            {

            }
            finally
            {

            }
        }
        public int GetQueueAvailableStatus(int queueId)
        {
            int status = 0;


            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "select count(status) from   l2_ees_queue  where id = " + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    status = Convert.ToInt32(command.ExecuteScalar());
                }

            }
            finally
            {

            }
            return status;
        }
    }
}
