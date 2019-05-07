using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.DBCon;
using Oracle.DataAccess.Client;
using System.Data;


namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.DB
{
    class PVLDaoImp:PVLDaoService
    {
        public List<Model.PVLData> GetPVLList()
        {
            List<Model.PVLData> lstPVLData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) // DA.Connection().getDBConnection())
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT PVL_ID, PVL_NAME,MACHINE_CODE,F_AISLE,F_ROW,IS_BLOCKED,STATUS,IS_AUTOMODE"
                                     + " ,MACHINE_CHANNEL,IS_SWITCH_OFF,START_AISLE,END_AISLE,FROM_FLOOR,TO_FLOOR"
                                     + " FROM L2_PVl_MASTER";
                        
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstPVLData = new List<Model.PVLData>();

                                while (reader.Read())
                                {

                                    Model.PVLData objPVLData = new Model.PVLData();

                                    objPVLData.pvlPkId = Int32.Parse(reader["PVL_ID"].ToString());
                                    objPVLData.pvlName = reader["PVL_NAME"].ToString();
                                    objPVLData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objPVLData.aisle = Int32.Parse(reader["F_AISLE"].ToString());

                                    objPVLData.row = Int32.Parse(reader["F_ROW"].ToString());
                                    objPVLData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString())==1;
                                    objPVLData.status = Int32.Parse(reader["STATUS"].ToString());
                                    objPVLData.autoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString());

                                    objPVLData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objPVLData.isSwitchOff = Int32.Parse(reader["IS_SWITCH_OFF"].ToString()) == 1;

                                    objPVLData.startAisle = Int32.Parse(reader["START_AISLE"].ToString());
                                    objPVLData.endAisle = Int32.Parse(reader["END_AISLE"].ToString());

                                    lstPVLData.Add(objPVLData);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            return lstPVLData;
        }

        public Model.PVLData GetPVLDetails(Model.PVLData objPVLData)
        {
           

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) // DA.Connection().getDBConnection())
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT PVL_ID, PVL_NAME,MACHINE_CODE,F_AISLE,F_ROW,IS_BLOCKED,STATUS,IS_AUTOMODE"
                                     + " ,MACHINE_CHANNEL,IS_SWITCH_OFF,START_AISLE,END_AISLE,FROM_FLOOR,TO_FLOOR"
                                     + " FROM L2_PVL_MASTER"
                                     + " WHERE " ;
                        if (objPVLData.pvlPkId != 0)
                            sql += " PVL_ID=" + objPVLData.pvlPkId;
                        if (!string.IsNullOrEmpty(objPVLData.machineCode))
                            sql += " MACHINE_CODE='" + objPVLData.machineCode + "'";
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                              
                                if(reader.Read())
                                {

                                    objPVLData.pvlPkId = Int32.Parse(reader["PVL_ID"].ToString());
                                    objPVLData.pvlName = reader["PVL_NAME"].ToString();
                                    objPVLData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objPVLData.aisle = Int32.Parse(reader["F_AISLE"].ToString());

                                    objPVLData.row = Int32.Parse(reader["F_ROW"].ToString());
                                    objPVLData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString()) == 1;
                                    objPVLData.status = Int32.Parse(reader["STATUS"].ToString());
                                    objPVLData.autoMode = Int32.Parse(reader["IS_AUTOMODE"].ToString());

                                    objPVLData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objPVLData.isSwitchOff = Int32.Parse(reader["IS_SWITCH_OFF"].ToString()) == 1;

                                    objPVLData.startAisle = Int32.Parse(reader["START_AISLE"].ToString());
                                    objPVLData.endAisle = Int32.Parse(reader["END_AISLE"].ToString());

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            return objPVLData;
        }
        public bool IsPVLBlockedInDB(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_BLOCKED FROM L2_PVL_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    bOk = bResult == 1;
                }
            }
            catch (Exception errMsg)
            {
                bOk = true;
            }
            finally
            {
            }

            return bOk;
        }


        public bool UpdateMachineBlockStatus(string machine_code, bool blockStatus)
        {
            bool bOk = false;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_PVL_MASTER set IS_BLOCKED ='" + (blockStatus ? 1 : 0) + "'"
                    + " where MACHINE_CODE = '" + machine_code + "' and IS_BLOCKED !='" + (blockStatus ? 1 : 0) + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bOk=command.ExecuteNonQuery()>0;

                }

            }
            finally
            {

            }
            return bOk;
        }


        public bool IsPVLDisabled(string machineName)
        {
            bool isDisable = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT status FROM L2_PVL_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    isDisable = bResult != 2;
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return isDisable;
        }


        public bool IsPVLSwitchOff(string machineName)
        {
            bool isSwitchOff = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_SWITCH_OFF FROM L2_PVL_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    isSwitchOff = bResult == 1;
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return isSwitchOff;
        }


        public int FindPalletGettingSlotAndPath(Model.PVLData objPVLData)
        {
            int pathID = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PVL_MANIPULATION.PALLET_GET_PROC";
                        command.Parameters.Add("pvl_code", OracleDbType.Varchar2, 20, objPVLData.machineCode, ParameterDirection.Input);
                        command.Parameters.Add("path_id", OracleDbType.Int32, pathID, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["path_id"].Value.ToString(), out pathID);
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            return pathID;
        }

        public int FindPalletStoringSlotAndPath(Model.PVLData objPVLData)
        {
            int pathID = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PVL_MANIPULATION.PALLET_STORE_PROC";
                        command.Parameters.Add("pvl_code", OracleDbType.Varchar2, 20, objPVLData.machineCode, ParameterDirection.Input);
                        command.Parameters.Add("path_id", OracleDbType.Int32, pathID, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["path_id"].Value.ToString(), out pathID);
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            return pathID;
        }


        public void UpdateAfterPVLTask(Model.PVLData objPVLData)
        {
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        if (con.State == ConnectionState.Closed) con.Open();

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PVL_MANIPULATION.task_after_pvl_processing";
                        command.Parameters.Add("pathid", OracleDbType.Int32, objPVLData.queueId, ParameterDirection.Input);
                        command.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception errMsg)
            {
                throw new Exception(errMsg.Message);
            }
        }
        public void TaskAfterGetPalletBundle(int pathID)
        {
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        if (con.State == ConnectionState.Closed) con.Open();

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PVL_MANIPULATION.task_after_get_pallet_bundle";
                        command.Parameters.Add("pathid", OracleDbType.Int32, pathID, ParameterDirection.Input);
                        command.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception errMsg)
            {
                throw new Exception(errMsg.Message);
            }
        }
        public bool InsertQueueForPVL(int requestType, string pvlCode)
        {

            int qId = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "PVL_MANIPULATION.insert_queue_for_pvl";
                        command.Parameters.Add("request_type", OracleDbType.Int32, requestType, ParameterDirection.Input);
                        command.Parameters.Add("arg_pvl_code", OracleDbType.Varchar2, 20, pvlCode, ParameterDirection.Input);
                        command.Parameters.Add("arg_queue_id", OracleDbType.Int32, qId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["arg_queue_id"].Value.ToString(), out qId);
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            return qId != 0;
        }
    }
}
