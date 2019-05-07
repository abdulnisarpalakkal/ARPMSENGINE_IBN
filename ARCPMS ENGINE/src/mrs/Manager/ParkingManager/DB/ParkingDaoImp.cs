using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using ARCPMS_ENGINE.src.mrs.DBCon;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Global;

namespace ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.DB
{
    class ParkingDaoImp:ParkingDaoService
    {
        public List<Model.PathDetailsData> FindAndGetInitialPath(int queueId)
        {
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = new OracleCommand())
                    {

                        //Allocate slot.
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PATH_SELECT_PACKAGE_NEW.initial_path_find_proc";
                        command.Parameters.Add("queue_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.ExecuteNonQuery();
                    }
                }


            }
            catch (Exception errMsg)
            {

            }
            return GetPathDetails(queueId);
        }
        public List<Model.PathDetailsData> FindAndGetInitialEntryPath(int queueId)
        {
            
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                   
                    using (OracleCommand command = new OracleCommand())
                    {
                        
                        //Allocate slot.
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "SLOT_ALLOCATION_PROC_NEW";
                        command.Parameters.Add("trans_queue_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.ExecuteNonQuery();
                    }
                }


            }
            catch (Exception errMsg)
            {

            }
            return GetPathDetails(queueId);
        }
        public List<Model.PathDetailsData> FindAndGetInitialExitPath(int queueId)
        {
        
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) 
                {
                    using (OracleCommand command = con.CreateCommand())
                    {
                       
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "FIND_EXIT_PATH ";
                        command.Parameters.Add("queueid", OracleDbType.Int32, queueId, ParameterDirection.InputOutput);
                        command.ExecuteNonQuery();
                        queueId = Convert.ToInt32(command.Parameters["queueid"].Value.ToString());
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return GetPathDetails(queueId);

        }
        public List<Model.PathDetailsData> FindAndGetDynamicPath(int queueId)
        {
            List<Model.PathDetailsData> lstPathDetails = null; // new List<PathDetails>();
            int pathId = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    
                    using (OracleCommand command = new OracleCommand())
                    {
                       
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PATH_SELECT_PACKAGE_NEW.dynamic_path_find_proc";
                        command.Parameters.Add("trans_queue_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.Parameters.Add("cm_path_id", OracleDbType.Int64, pathId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["cm_path_id"].Value.ToString(), out pathId);
                    }
                }

              
                if (pathId > 0)
                {
                    lstPathDetails = GetPathDetails(queueId, pathId);
                }
            }
            catch (Exception errMsg)
            {
            }
            return lstPathDetails;
        }
        

        public List<Model.PathDetailsData> FindAndGetDynamicEntryPath(int queueId)
        {
            List<Model.PathDetailsData> lstPathDetails = null; // new List<PathDetails>();
            int pathId = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    
                    using (OracleCommand command = new OracleCommand())
                    {
                       
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PATH_SELECT_PACKAGE_NEW.ucm_path_find_proc";
                        command.Parameters.Add("queue_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.Parameters.Add("ucm_path_id", OracleDbType.Int64, pathId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["ucm_path_id"].Value.ToString(), out pathId);
                    }
                }

              
                if (pathId > 0)
                {
                    lstPathDetails = GetPathDetails(queueId, pathId);
                }
            }
            catch (Exception errMsg)
            {
            }
            return lstPathDetails;
        }

        public List<Model.PathDetailsData> FindAndGetDynamicExitPath(int queueId)
        {
            List<Model.PathDetailsData> lstPathDetails = null; 
            int pathId = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    
                    using (OracleCommand command = new OracleCommand())
                    {
                        
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PATH_SELECT_PACKAGE_NEW.lcm_path_find_proc_for_exit";
                        command.Parameters.Add("queue_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.Parameters.Add("lcm_path_id", OracleDbType.Int64, pathId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["lcm_path_id"].Value.ToString(), out pathId);
                    }
                }

               
                if (pathId > 0)
                {
                    lstPathDetails = GetPathDetails(queueId, pathId);
                }
            }
            catch (Exception errMsg)
            {
            }
            return lstPathDetails;
        }
        public List<Model.PathDetailsData> FindAndGetVLCDynamicPath(int queueId)
        {
            List<Model.PathDetailsData> lstPathDetails = null; // new List<PathDetails>();
            int pathId = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = new OracleCommand())
                    {

                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PATH_SELECT_PACKAGE_NEW.dynamic_vlc_path_find_proc";
                        command.Parameters.Add("trans_queue_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.Parameters.Add("cm_path_id", OracleDbType.Int64, pathId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["cm_path_id"].Value.ToString(), out pathId);
                    }
                }


                if (pathId > 0)
                {
                    lstPathDetails = GetPathDetails(queueId, pathId);
                }
            }
            catch (Exception errMsg)
            {
            }
            return lstPathDetails;
        }
        public List<Model.PathDetailsData> FindAndGetAllocationPath(int queueId)
        {
            //Logger.WriteLogger(GlobalValues.PARKING_LOG, "FindAndGetAllocationPath(): Queue Id:" + queueId + ":--Entered");
            List<Model.PathDetailsData> lstPathDetails = null; // new List<PathDetails>();
            int pathId = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = new OracleCommand())
                    {

                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PATH_SELECT_PACKAGE_NEW.reallocate_path_proc";
                        command.Parameters.Add("trans_queue_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.Parameters.Add("cm_path_id", OracleDbType.Int64, pathId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["cm_path_id"].Value.ToString(), out pathId);
                    }
                }

               
                    lstPathDetails = GetPathDetails(queueId, pathId);
               
            }
            catch (Exception errMsg)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "FindAndGetAllocationPath(): Queue Id:" + queueId + ":--Exception-->" + errMsg.Message);
            }
            //Logger.WriteLogger(GlobalValues.PARKING_LOG, "FindAndGetAllocationPath(): Queue Id:" + queueId + ":--Exitting");
            return lstPathDetails;
        }
        
        public List<Model.PathDetailsData> GetPathDetails(int queueId,int pathId=0)
        {
            List<Model.PathDetailsData> lstPathDetails = null;

            Model.PathDetailsData pathDetails = null;

            try
            {
                string sql = "SELECT * FROM L2_PATH_DETAILS WHERE done=0 and   QUEUE_ID=" + queueId;
                if (pathId != 0)
                {
                    sql += " and PATH_ID = " + pathId;
                }
                sql += " order by sequence_id";
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    using (OracleCommand command = con.CreateCommand())
                    {
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstPathDetails = new List<Model.PathDetailsData>();

                                while (reader.Read())
                                {
                                    pathDetails = new Model.PathDetailsData();

                                    pathDetails.pathPkId = Int32.Parse(reader["PATH_ID"].ToString());
                                    pathDetails.aisle = Int32.Parse(reader["AISLE"].ToString());
                                    pathDetails.command = reader["COMMAND"].ToString();
                                    pathDetails.floor = Int32.Parse(reader["FLOOR"].ToString());
                                    pathDetails.machine = reader["MACHINE"].ToString();
                                    pathDetails.machineName = reader["MACHINE_NAME"].ToString();
                                  
                                    pathDetails.queueId = Int32.Parse(reader["QUEUE_ID"].ToString());
                                    pathDetails.row = Int32.Parse(reader["F_ROW"].ToString());
                                    pathDetails.sequencePkId = Int32.Parse(reader["SEQUENCE_ID"].ToString());
                                    pathDetails.done = Int32.Parse(reader["DONE"].ToString())==1?true:false;
                                    pathDetails.channel = reader["CHANNEL"].ToString();
                                    pathDetails.unblockMachine = Convert.ToString(reader["UNBLOCK_MACHINE"]);
                                    pathDetails.callBackRehandle = Convert.ToInt32(reader["CALL_BACK_REHANDLE"]);
                                    pathDetails.callUpadate = Convert.ToInt32(reader["CALL_UPDATE"]) == 1 ? true : false;
                                    pathDetails.parallelMove = Convert.ToInt32(reader["PARALLEL_MOVE"]) == 1 ? true : false;
                                    pathDetails.dynamicCall = Convert.ToInt32(reader["DYNAMIC_CALL"])==1?true:false;
                                    pathDetails.interactMachineCode = reader["INTERACT_MACHINE_CODE"].ToString();
                                    pathDetails.cmd_val_in_number = Int32.Parse(reader["CMD_VAL_IN_NUMBER"].ToString());
                                    pathDetails.confirmTrigger = Convert.ToInt32(reader["CONFIRM_TRIGGER"]) == 1 ? true : false;
                                    lstPathDetails.Add(pathDetails);
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
            return lstPathDetails;

        }
        public bool UpdateSlotAfterGetCarFromSlot(int queueId)
        {
            bool success = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) 
                {
                    using (OracleCommand command = con.CreateCommand())
                    {
                        if (con.State == ConnectionState.Closed) con.Open();
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "UPDATE_AFTER_RETRIEVAL ";
                        command.Parameters.Add("park_queue_Id", OracleDbType.Int32, queueId, ParameterDirection.Input);
                        command.ExecuteNonQuery();
                        success = true;
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return success;
        }

        public bool CallProcedureAfterProcessCar(int queueId)
        {
            bool success = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                  
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "UDPATE_AFTER_CAR_PROCESSING";
                        command.CommandText = sql;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("QUEUE_ID", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.ExecuteNonQuery();
                        success = true;
                    }
                    
                }
                
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            return success;
        }

        public void ResetProcedureCall()
        {
            using (OracleConnection con = new DBConnection().getDBConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                OracleCommand command = con.CreateCommand();
                string sql = "RESET_PROCEDURE_NEW";
                command.CommandText = sql;
                command.CommandType = CommandType.StoredProcedure;
                command.ExecuteNonQuery();
            }
        }



        public bool IsRotated(int queueId)
        {
            bool status = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select ct.is_rotate from L2_CUSTOMERS ct inner join l2_ees_queue q on ct.CUSTOMER_ID=q.customer_id where  q.id=" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    status = Convert.ToInt16(command.ExecuteScalar()) == 1 ? true : false;
                }
            }
            finally
            {

            }
            return status;
        }

        public bool UpdateRotaion(int queueId,bool rotateStatus)
        {
            bool bOk = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_CUSTOMERS ct set ct.is_rotate=" + (rotateStatus?1:0) + " where ct.CUSTOMER_ID=(select q.customer_id from l2_ees_queue q where q.id=" + queueId + ")";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    bOk = true;
                }
            }
            finally
            {

            }
            return bOk;
        }
        public bool IsSlotFromEESLevel(int queueId)
        {
            bool isEESFloor = false;
            int eesFloor = 4;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select SLOT_FLOOR from l2_slot_path where  QUEUE_ID=" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    isEESFloor = Convert.ToInt16(command.ExecuteScalar()) == eesFloor ? true : false;
                }
            }
            finally
            {

            }
            return isEESFloor;
        }
        public void UpdateCarReachedAtEESTime(int exitQueueId)
        {
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        if (con.State == ConnectionState.Closed) con.Open();

                        command.CommandType = CommandType.Text;
                        command.CommandText = "update l2_customers set exit_time=sysdate where EXIT_QUEUE_ID = " + exitQueueId + " and is_retrieved=0";
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
        }
        public string GetCardIdUsingQueueId(int queueId)
        {
            string cardId = null;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select CARD_ID from l2_customers where customer_id = (select customer_id from l2_ees_queue where id= " + queueId + ")";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    cardId = Convert.ToString(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {

            }
            return cardId;
        }

        public Model.PathDetailsData GetEachPathDetailsRecord(int queueId, int pathId = 0)
        {
            //List<Model.PathDetailsData> lstPathDetails = null;

            Model.PathDetailsData pathDetails = new Model.PathDetailsData();

            try
            {
                string sql = "select * from (SELECT * FROM L2_PATH_DETAILS WHERE  QUEUE_ID=" + queueId;
                if (pathId != 0)
                {
                    sql += " and PATH_ID = " + pathId;
                }
                sql += " and done=0 order by sequence_id) where  rownum=1";
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
                                    pathDetails = new Model.PathDetailsData();

                                    pathDetails.pathPkId = Int32.Parse(reader["PATH_ID"].ToString());
                                    pathDetails.aisle = Int32.Parse(reader["AISLE"].ToString());
                                    pathDetails.command = reader["COMMAND"].ToString();
                                    pathDetails.floor = Int32.Parse(reader["FLOOR"].ToString());
                                    pathDetails.machine = reader["MACHINE"].ToString();
                                    pathDetails.machineName = reader["MACHINE_NAME"].ToString();

                                    pathDetails.queueId = Int32.Parse(reader["QUEUE_ID"].ToString());
                                    pathDetails.row = Int32.Parse(reader["F_ROW"].ToString());
                                    pathDetails.sequencePkId = Int32.Parse(reader["SEQUENCE_ID"].ToString());
                                    pathDetails.done = Int32.Parse(reader["DONE"].ToString()) == 1 ? true : false;
                                    pathDetails.channel = reader["CHANNEL"].ToString();
                                    pathDetails.unblockMachine = Convert.ToString(reader["UNBLOCK_MACHINE"]);
                                    pathDetails.callBackRehandle = Convert.ToInt32(reader["CALL_BACK_REHANDLE"]);
                                    pathDetails.callUpadate = Convert.ToInt32(reader["CALL_UPDATE"]) == 1 ? true : false;
                                    pathDetails.parallelMove = Convert.ToInt32(reader["PARALLEL_MOVE"]) == 1 ? true : false;
                                    pathDetails.dynamicCall = Convert.ToInt32(reader["DYNAMIC_CALL"]) == 1 ? true : false;
                                    pathDetails.interactMachineCode = reader["INTERACT_MACHINE_CODE"].ToString();
                                    pathDetails.cmd_val_in_number = Int32.Parse(reader["CMD_VAL_IN_NUMBER"].ToString());
                                    pathDetails.confirmTrigger = Convert.ToInt32(reader["CONFIRM_TRIGGER"]) == 1 ? true : false;
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
            return pathDetails;
        }

        public bool IsPathDynamicVLC(int queueId)
        {
            bool isDynamic = false;
           
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select PATH_TYPE from l2_slot_path where  QUEUE_ID=" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    isDynamic = Convert.ToInt16(command.ExecuteScalar()) == 1 ? true : false;
                }
            }
            finally
            {

            }
            return isDynamic;
        }
        public bool IsPathDynamicVLCForExit(int queueId)
        {
            bool isDynamic = false;
           
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select PATH_TYPE from l2_slot_path where  QUEUE_ID=" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    isDynamic = Convert.ToInt16(command.ExecuteScalar()) == 1 ? true : false;
                }
            }
            finally
            {

            }
            return isDynamic;
        }
        
        public void UpdateSearchingPathStatus(int queueId,bool searchStatus) 
        {
            string query = null;
            string transStatus = null; 
            
            try
            {
               // transStatus = searchStatus ? "Searching Path" : "";
                query = "update L2_EES_QUEUE set SEARCH_FOR_PATH=" + (searchStatus ? 1 : 0)
                    + " where ID = " + queueId + " and SEARCH_FOR_PATH!=" + (searchStatus ? 1 : 0);
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        if (con.State == ConnectionState.Closed) con.Open();

                        command.CommandType = CommandType.Text;
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
        }
        public ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE IsExitCarHigh(int queueId)
        {
            ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE carType = ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE.high;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select IS_HIGH_CAR from L2_CUSTOMERS where  EXIT_QUEUE_ID=" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    carType = (ARCPMS_ENGINE.src.mrs.Global.GlobalValues.CAR_TYPE)Convert.ToInt16(command.ExecuteScalar());
                }
            }
            finally
            {

            }
            return carType;
        }
        public bool FindOptimizedPath(int queueId)
        {
            int pathId = 0;
            bool status = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = new OracleCommand())
                    {

                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "ADVANCED_PATH_SELECT_PACKAGE.dynamic_optimize";
                        command.Parameters.Add("arg_queue_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.ExecuteNonQuery();
                    }
                }


              
            }
            catch (Exception errMsg)
            {
                status = false;
            }
            return status;
        }
        public int GetMachineStage(int queueId)
        {
            int machineStage = 0;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select machine_stage from l2_slot_path where  QUEUE_ID=" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    machineStage = Convert.ToInt16(command.ExecuteScalar()) ;
                }
            }
            finally
            {

            }
            return machineStage;
        }
        public bool GetIterationStatus(int queueId)
        {
            bool iterationStatus = false;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select NEED_ITERATION from l2_slot_path where  QUEUE_ID=" + queueId;
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    iterationStatus = Convert.ToInt16(command.ExecuteScalar()) == 1 ? true : false;
                }
            }
            finally
            {

            }
            return iterationStatus;
        }
        public int GetPathStartFlag(int queueId)
        {
            int flag = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select PATH_SELECT_PACKAGE_NEW.get_path_start_flag(" + queueId + ") from dual";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    flag = Convert.ToInt16(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                flag = 0;
            }
            finally
            {

            }
            return flag;
        }
        public void ResumeProcedureCall()
        {
            using (OracleConnection con = new DBConnection().getDBConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                OracleCommand command = con.CreateCommand();
                string sql = "RESUME_PROCEDURE";
                command.CommandText = sql;
                command.CommandType = CommandType.StoredProcedure;
                command.ExecuteNonQuery();
            }
        }
        public int GetMachineSearchFlag(int queueId)
        {
            int flag = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "select QUEUE_HANDLING_PACKAGE.get_machine_search_flag(" + queueId + ") from dual";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    flag = Convert.ToInt16(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                flag = 0;
            }
            finally
            {

            }
            return flag;
        }
    }
}
