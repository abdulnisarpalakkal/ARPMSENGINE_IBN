using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.Model;
using ARCPMS_ENGINE.src.mrs.DBCon;
using Oracle.DataAccess.Client;
using System.Data;
using ARCPMS_ENGINE.src.mrs.Config;
using ARCPMS_ENGINE.src.mrs.Global;


namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.CM.DB
{
    class CMDaoImp:CMDaoService
    {
        object synchTriggerTableReadUpdate = new object();
        public List<CMData> GetCMDetails(CMData objCMData)
        {
            List<CMData> lstCMData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) // DA.Connection().getDBConnection())
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT CM_ID, CM_NAME,FLOOR,ACTUAL_AISLE_MIN,ACTUAL_AISLE_MAX,VIRTUAL_AISLE_MIN,VIRTUAL_AISLE_MAX,HOME_AISLE"
                                     + " ,MACHINE,MACHINE_CODE,IS_BLOCKED,POSITION_AISLE,HOME_ROW,POSITION_ROW,STATUS,FLOOR_CM_INDEX,CM_CHANNEL,REM_CODE"
                                     + " FROM L2_CM_MASTER"
                                     + " where ";
                        if (!string.IsNullOrEmpty(objCMData.machineCode))
                            sql += " machine_code=" + objCMData.machineCode;
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstCMData = new List<CMData>();

                                while (reader.Read())
                                {

                                    Model.CMData obj2CMData = new Model.CMData();

                                    obj2CMData.cmPkId = Int32.Parse(reader["CM_ID"].ToString());
                                    obj2CMData.cmName = reader["CM_NAME"].ToString();
                                    obj2CMData.floor = Int32.Parse(reader["FLOOR"].ToString());
                                    obj2CMData.actualAisleMin = Int32.Parse(reader["ACTUAL_AISLE_MIN"].ToString());

                                    obj2CMData.actualAisleMax = Int32.Parse(reader["ACTUAL_AISLE_MAX"].ToString());
                                    obj2CMData.virtualAisleMin = Int32.Parse(reader["VIRTUAL_AISLE_MIN"].ToString());
                                    obj2CMData.virtualAisleMax = Int32.Parse(reader["VIRTUAL_AISLE_MAX"].ToString());
                                    obj2CMData.homeAisle = Int32.Parse(reader["HOME_AISLE"].ToString());

                                    obj2CMData.machine = reader["MACHINE"].ToString();
                                    obj2CMData.cmChannel = reader["CM_CHANNEL"].ToString();
                                    obj2CMData.machineCode = reader["MACHINE_CODE"].ToString();
                                    obj2CMData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString()) == 1 ? true : false;

                                    obj2CMData.positionAisle = Convert.ToInt32(reader["POSITION_AISLE"]);
                                    obj2CMData.homeRow = Convert.ToInt32(reader["HOME_ROW"]);
                                    obj2CMData.positionRow = Convert.ToInt32(reader["POSITION_ROW"]);
                                    obj2CMData.status = Convert.ToInt32(reader["STATUS"]);

                                    obj2CMData.floorCmIndex = Convert.ToInt32(reader["FLOOR_CM_INDEX"]);
                                    obj2CMData.remCode = Convert.ToString(reader["REM_CODE"]);

                                    //objCMData.collapseMachine =   
                                    //objCMData.triggerAisle =      
                                    // objCMData.triggerRow =        
                                    //objCMData.inDemoMode =        

                                    // objCMData.defaultHomeAisle =  
                                    //objCMData.isPalletPresent =   


                                    lstCMData.Add(obj2CMData);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return lstCMData;
        }

        public List<CMData> GetCMList()
        {
            List<CMData> lstCMData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) // DA.Connection().getDBConnection())
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT CM_ID, CM_NAME,FLOOR,ACTUAL_AISLE_MIN,ACTUAL_AISLE_MAX,VIRTUAL_AISLE_MIN,VIRTUAL_AISLE_MAX,HOME_AISLE"
                                     + " ,MACHINE,MACHINE_CODE,IS_BLOCKED,POSITION_AISLE,HOME_ROW,POSITION_ROW,STATUS,FLOOR_CM_INDEX,CM_CHANNEL,REM_CODE"
                                     + " FROM L2_CM_MASTER";
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstCMData = new List<CMData>();

                                while (reader.Read())
                                {

                                    Model.CMData objCMData = new Model.CMData();

                                    objCMData.cmPkId = Int32.Parse(reader["CM_ID"].ToString());
                                    objCMData.cmName = reader["CM_NAME"].ToString();
                                    objCMData.floor = Int32.Parse(reader["FLOOR"].ToString());
                                    objCMData.actualAisleMin = Int32.Parse(reader["ACTUAL_AISLE_MIN"].ToString());

                                    objCMData.actualAisleMax = Int32.Parse(reader["ACTUAL_AISLE_MAX"].ToString());
                                    objCMData.virtualAisleMin = Int32.Parse(reader["VIRTUAL_AISLE_MIN"].ToString());
                                    objCMData.virtualAisleMax = Int32.Parse(reader["VIRTUAL_AISLE_MAX"].ToString());
                                    objCMData.homeAisle = Int32.Parse(reader["HOME_AISLE"].ToString());

                                    objCMData.machine = reader["MACHINE"].ToString();
                                    objCMData.cmChannel = reader["CM_CHANNEL"].ToString();
                                    objCMData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objCMData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString()) == 1 ? true : false;

                                    objCMData.positionAisle = Convert.ToInt32(reader["POSITION_AISLE"]);
                                    objCMData.homeRow = Convert.ToInt32(reader["HOME_ROW"]);
                                    objCMData.positionRow = Convert.ToInt32(reader["POSITION_ROW"]);
                                    objCMData.status = Convert.ToInt32(reader["STATUS"]);

                                    objCMData.floorCmIndex = Convert.ToInt32(reader["FLOOR_CM_INDEX"]);
                                    objCMData.remCode = Convert.ToString(reader["REM_CODE"]);

                                    //objCMData.collapseMachine =   
                                    //objCMData.triggerAisle =      
                                    // objCMData.triggerRow =        
                                    //objCMData.inDemoMode =        

                                    // objCMData.defaultHomeAisle =  
                                    //objCMData.isPalletPresent =   


                                    lstCMData.Add(objCMData);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return lstCMData;
        }


        public bool UpdateBoolValueUsingMachineCode(string machineCode, string tableField, bool dataValue)
        {
            string qry = "";
            int intDataValue = 0;
            intDataValue = dataValue == true ? 1 : 0;
            bool result;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        qry = "UPDATE L2_CM_MASTER SET " + tableField + " = " + intDataValue
                               + " WHERE machine_code ='" + machineCode + "'";

                        command.CommandText = qry;
                        command.ExecuteNonQuery();
                        result = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                result = false;
                //   rpt.Notification("UPDATECMPOSITION" + errMsg.Message + ", " + machineName);
            }
            return result;
        }

        public bool UpdateIntValueUsingMachineCode(string machineCode, string tableField, int dataValue)
        {
            string qry = "";
            bool result;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        qry = "UPDATE L2_CM_MASTER SET " + tableField + " = " + dataValue
                               + " WHERE machine_code ='" + machineCode + "' and " + tableField + " != " + dataValue;

                        command.CommandText = qry;
                        command.ExecuteNonQuery();
                        result = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                result = false;
                //   rpt.Notification("UPDATECMPOSITION" + errMsg.Message + ", " + machineName);
            }
            return result;
        }

        public bool UpdateStringValueUsingMachineCode(string machineCode, string tableField, string dataValue)
        {
            string qry = "";
            bool result;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        qry = "UPDATE L2_CM_MASTER SET " + tableField + " = " + dataValue
                               + " WHERE machine_code ='" + machineCode + "' and " + tableField + " != " + dataValue;

                        command.CommandText = qry;
                        command.ExecuteNonQuery();
                        result = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                result = false;
                //   rpt.Notification("UPDATECMPOSITION" + errMsg.Message + ", " + machineName);
            }
            return result;
        }

        public bool UpdateBoolValueUsingRemCode(string machineCode, string tableField, bool dataValue)
        {
            string qry = "";
            bool result;
            int intDataValue = 0;
            intDataValue = dataValue == true ? 1 : 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        qry = "UPDATE L2_CM_MASTER SET " + tableField + " = " + intDataValue
                               + " WHERE REM_CODE ='" + machineCode + "' and " + tableField + " != " + intDataValue;

                        command.CommandText = qry;
                        command.ExecuteNonQuery();
                        result = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                result = false;
                //   rpt.Notification("UPDATECMPOSITION" + errMsg.Message + ", " + machineName);
            }
            return result;
        }


        public bool GetNearbyLCMlist(CMData objCMData, out CMData leftCMData, out CMData rightCMData)
        {
            bool bOk = false;
            leftCMData = new CMData();
            rightCMData = new CMData();

            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    using (OracleCommand command = con.CreateCommand())
                    {
                        
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "config_package.get_nearest_lcms";
                        command.Parameters.Add("cur_lcm", OracleDbType.Varchar2, 20, objCMData.machineCode, ParameterDirection.Input);
                        command.Parameters.Add("left_lcm", OracleDbType.Varchar2, 20, leftCMData.machineCode, ParameterDirection.Output);
                        command.Parameters.Add("left_lcm_channel", OracleDbType.Varchar2, 20, leftCMData.cmChannel, ParameterDirection.Output);
                        command.Parameters.Add("right_lcm", OracleDbType.Varchar2, 20, rightCMData.machineCode, ParameterDirection.Output);
                        command.Parameters.Add("right_lcm_channel", OracleDbType.Varchar2, 20, rightCMData.cmChannel, ParameterDirection.Output);

                        command.ExecuteNonQuery();

                        leftCMData.cmChannel = Convert.ToString(command.Parameters["left_lcm_channel"].Value);
                        leftCMData.machineCode = Convert.ToString(command.Parameters["left_lcm"].Value);
                        rightCMData.cmChannel = Convert.ToString(command.Parameters["right_lcm_channel"].Value);
                        rightCMData.machineCode = Convert.ToString(command.Parameters["right_lcm"].Value);

                        if (leftCMData.cmChannel == "null") leftCMData.cmChannel = "";
                        if (leftCMData.machineCode == "null") leftCMData.machineCode = "";

                        if (rightCMData.cmChannel == "null") rightCMData.cmChannel = "";
                        if (rightCMData.machineCode == "null") rightCMData.machineCode = "";

                        bOk = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                throw new Exception(errMsg.Message);
            }

            return bOk;
        }
        
       
        public bool IsCMBlockedInDB(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_BLOCKED FROM L2_CM_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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

        public CMData GetMovingSideCM(CMData objCMData)
        {
            
            CMData colCMData = null;
            try
            {

                colCMData = new CMData();

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                       
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "GET_MOVING_SIDE_CM ";
                        command.Parameters.Add("MOVE_MACHINE", OracleDbType.Varchar2, 30, objCMData.machineCode, ParameterDirection.Input);
                        command.Parameters.Add("dest_floor", OracleDbType.Int64, objCMData.destFloor, ParameterDirection.Input);
                        command.Parameters.Add("DEST_AISLE", OracleDbType.Int64, objCMData.destAisle, ParameterDirection.Input);
                        command.Parameters.Add("dest_row", OracleDbType.Int64, objCMData.destRow, ParameterDirection.Input);
                        command.Parameters.Add("COL_MACHINE", OracleDbType.Varchar2, 30, colCMData.machineCode, ParameterDirection.Output);
                        command.Parameters.Add("COL_MACHINE_CHANNEL", OracleDbType.Varchar2, 30, colCMData.cmChannel, ParameterDirection.Output);
                        command.Parameters.Add("col_side", OracleDbType.Int64, colCMData.moveSide, ParameterDirection.Output);

                        command.ExecuteNonQuery();
                       
                        colCMData.cmChannel = command.Parameters["col_machine_channel"].Value.ToString();
                        colCMData.machineCode = command.Parameters["COL_MACHINE"].Value.ToString();
                        colCMData.moveSide = Convert.ToInt16(command.Parameters["col_side"].Value.ToString());

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return colCMData;
        }
        public bool GetValidAisleForMoving(CMData objCMData,out int moveAisle,out int moveRow)
        {
            bool bOk = false;
            moveAisle = objCMData.destAisle;
            moveRow = objCMData.destRow;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                        
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "CONFIG_PACKAGE.get_valid_aisle_for_moving";
                        command.Parameters.Add("floor", OracleDbType.Int32, objCMData.destFloor, ParameterDirection.Input);
                        command.Parameters.Add("floor_aisle", OracleDbType.Int32, moveAisle, ParameterDirection.InputOutput);
                        command.Parameters.Add("floor_row", OracleDbType.Int32, moveRow, ParameterDirection.InputOutput);
                        command.Parameters.Add("direction", OracleDbType.Int16, objCMData.moveSide, ParameterDirection.Input);

                        command.ExecuteNonQuery();
                        
                        string tmpMoveAisle = "";
                        string tmpMoveRow = "";
                        tmpMoveAisle = Convert.ToString(command.Parameters["floor_aisle"].Value);
                        Int32.TryParse(tmpMoveAisle, out moveAisle);

                        tmpMoveRow = Convert.ToString(command.Parameters["floor_row"].Value);
                        Int32.TryParse(tmpMoveRow, out moveRow);

                        bOk = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                throw new Exception(errMsg.Message);
            }

            return bOk;
        }
        public bool UpdateMachineBlockStatus(string machine_code,bool blockStatus)
        {
            bool bOk = false;
            
            
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_CM_MASTER set IS_BLOCKED ='" + (blockStatus ? 1 : 0) + "', block_q_id=0, BLOCKED_FOR_HOME =0 "
                        +" where MACHINE_CODE = '" + machine_code + "' and IS_BLOCKED= '" + (blockStatus ? 0 : 1) + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bOk = command.ExecuteNonQuery()>0;
                }
                
            }
            finally
            {

            }
            return bOk;
        }
        public bool UpdateMachineBlockStatusForHome(string machine_code, bool blockStatus)
        {
            bool bOk = false;
            string sql = null;

            sql = "update L2_CM_MASTER set IS_BLOCKED='" + (blockStatus ? 1 : 0) + "' , BLOCKED_FOR_HOME =" + (blockStatus ? 1 : 0)
                    + " where MACHINE_CODE = '" + machine_code + "'  and IS_BLOCKED!= '" + (blockStatus ? 1 : 0) + "' and BLOCKED_FOR_HOME != '" + (blockStatus ? 1 : 0) + "' and block_q_id=0";
           


            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    
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

        public bool IsCMDisabled(string machineName)
        {
            bool isSwitchOff = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT STATUS FROM L2_CM_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    isSwitchOff = bResult != 2;
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
        public bool IsCMSwitchOff(string machineName)
        {
            bool isSwitchOff = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                   
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_SWITCH_OFF FROM L2_CM_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    isSwitchOff = bResult ==1 ;
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



        public bool UpdateCMClearRequest(string machineCode,string clearMachine, int clearFlag)
        {
            bool bOk = false;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_CM_MASTER set CROSS_REFERENCE_CLEAR ='" + clearFlag + "',COLLAPSE_MACHINE='" + clearMachine + "'"
                                    + " where MACHINE_CODE = '" + machineCode + "'";
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
        public bool UpdateCMClearPermission(string machineCode)
        {
            bool bOk = false;
            int permissionFlag = 2;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_CM_MASTER set CROSS_REFERENCE_CLEAR ='" + permissionFlag + "'"
                                    + " where MACHINE_CODE = '" + machineCode + "'";
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
        public bool GetCMClearRequest(string machineCode, out string clearingMachine, out int clearFlag)
        {
            bool success = false;
            clearingMachine = null;
            clearFlag = 0;
            try
            {
               
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT CROSS_REFERENCE_CLEAR,COLLAPSE_MACHINE FROM L2_CM_MASTER WHERE MACHINE_CODE ='" + machineCode + "'";
                    command.CommandText = sql;
                    
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            clearingMachine = reader["COLLAPSE_MACHINE"].ToString();
                            clearFlag = Int32.Parse(reader["CROSS_REFERENCE_CLEAR"].ToString());
                            success = true;
                        }

                    }
                    
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return success;
        }
        public CMData NeedToPushNearestCM(CMData objCMData)
        {
            Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ":--Entered 'NeedToPushNearestCM' ");
            CMData colCMData = null;
            try
            {

                colCMData = new CMData();

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {
                       
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "CONFIG_PACKAGE.need_to_push_nearest_cm ";
                        command.Parameters.Add("move_cm", OracleDbType.Varchar2, 30, objCMData.machineCode, ParameterDirection.Input);
                        command.Parameters.Add("dest_floor", OracleDbType.Int32, objCMData.destFloor, ParameterDirection.Input);
                        command.Parameters.Add("DEST_AISLE", OracleDbType.Int32, objCMData.destAisle, ParameterDirection.Input);
                        command.Parameters.Add("dest_row", OracleDbType.Int32, objCMData.destRow, ParameterDirection.Input);
                        command.Parameters.Add("COL_CM", OracleDbType.Varchar2, 30, colCMData.machineCode, ParameterDirection.Output);
                        command.Parameters.Add("col_cm_channel", OracleDbType.Varchar2, 30, colCMData.cmChannel, ParameterDirection.Output);
                        command.Parameters.Add("col_move_aisle", OracleDbType.Int32, colCMData.destAisle, ParameterDirection.Output);

                        command.Parameters.Add("col_move_row", OracleDbType.Int32, 30, colCMData.destRow, ParameterDirection.Output);
                        command.Parameters.Add("need_to_push", OracleDbType.Int32, objCMData.needToPush, ParameterDirection.Output);
                        command.Parameters.Add("is_wait", OracleDbType.Int32, 30, objCMData.isWait, ParameterDirection.Output);
                        command.Parameters.Add("col_cm_present", OracleDbType.Int32, objCMData.CMPresentOnSide, ParameterDirection.Output);
                        command.Parameters.Add("cm_move_side", OracleDbType.Int32, objCMData.moveSide, ParameterDirection.Output);
                        
                        command.ExecuteNonQuery();

                        colCMData.cmChannel = command.Parameters["col_cm_channel"].Value.ToString();
                        colCMData.machineCode = command.Parameters["COL_CM"].Value.ToString();
                        colCMData.destFloor = objCMData.destFloor;
                        colCMData.destAisle = Convert.ToInt16(command.Parameters["col_move_aisle"].Value.ToString());
                        colCMData.destRow = Convert.ToInt16(command.Parameters["col_move_row"].Value.ToString());
                        objCMData.needToPush = Convert.ToInt16(command.Parameters["need_to_push"].Value.ToString())==1?true:false;
                        objCMData.isWait = Convert.ToInt16(command.Parameters["is_wait"].Value.ToString()) == 1 ? true : false;
                        objCMData.CMPresentOnSide = Convert.ToInt16(command.Parameters["col_cm_present"].Value.ToString()) == 1 ? true : false;
                        objCMData.moveSide = Convert.ToInt16(command.Parameters["cm_move_side"].Value.ToString()) ;

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ": CM=" + objCMData.machineCode + " --need_to_push 'NeedToPushNearestCM':: " + ex.Message);
            }
            finally
            {
                Logger.WriteLogger(GlobalValues.PARKING_LOG, "Queue Id:" + objCMData.queueId + ", isWait=" + objCMData.isWait + ", need_to_push=" + objCMData.needToPush + ":--Exitting 'NeedToPushNearestCM' ");
            }
            return colCMData;
        }
        public int GetCMAisleFromDB(string machineName)
        {
            int cmAisle = 0;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT position_aisle FROM L2_CM_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    cmAisle = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (Exception errMsg)
            {
                cmAisle = 0;
            }
            finally
            {
            }

            return cmAisle;
        }

        public CMData GetBlockedCMDetails(Int64 queueId)
        {
            CMData objCMData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) // DA.Connection().getDBConnection())
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT CM_ID, CM_NAME,FLOOR,ACTUAL_AISLE_MIN,ACTUAL_AISLE_MAX,VIRTUAL_AISLE_MIN,VIRTUAL_AISLE_MAX,HOME_AISLE"
                                      + " ,MACHINE,MACHINE_CODE,IS_BLOCKED,POSITION_AISLE,HOME_ROW,POSITION_ROW,STATUS,FLOOR_CM_INDEX,CM_CHANNEL,REM_CODE"
                                      + " FROM L2_CM_MASTER"
                                     + " where BLOCK_Q_ID=" + queueId+ " and rownum=1";
                      
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                               

                                if (reader.Read())
                                {

                                    objCMData = new Model.CMData();

                                    objCMData.cmPkId = Int32.Parse(reader["CM_ID"].ToString());
                                    objCMData.cmName = reader["CM_NAME"].ToString();
                                    objCMData.floor = Int32.Parse(reader["FLOOR"].ToString());
                                    objCMData.actualAisleMin = Int32.Parse(reader["ACTUAL_AISLE_MIN"].ToString());

                                    objCMData.actualAisleMax = Int32.Parse(reader["ACTUAL_AISLE_MAX"].ToString());
                                    objCMData.virtualAisleMin = Int32.Parse(reader["VIRTUAL_AISLE_MIN"].ToString());
                                    objCMData.virtualAisleMax = Int32.Parse(reader["VIRTUAL_AISLE_MAX"].ToString());
                                    objCMData.homeAisle = Int32.Parse(reader["HOME_AISLE"].ToString());

                                    objCMData.machine = reader["MACHINE"].ToString();
                                    objCMData.cmChannel = reader["CM_CHANNEL"].ToString();
                                    objCMData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objCMData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString()) == 1 ? true : false;

                                    objCMData.positionAisle = Convert.ToInt32(reader["POSITION_AISLE"]);
                                    objCMData.homeRow = Convert.ToInt32(reader["HOME_ROW"]);
                                    objCMData.positionRow = Convert.ToInt32(reader["POSITION_ROW"]);
                                    objCMData.status = Convert.ToInt32(reader["STATUS"]);

                                    objCMData.floorCmIndex = Convert.ToInt32(reader["FLOOR_CM_INDEX"]);
                                    objCMData.remCode = Convert.ToString(reader["REM_CODE"]);

                                 
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {

            }
            return objCMData;
        }

        public bool GetCMRotationEnabledStatus(string machineCode)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection()) // DA.Connection().getDBConnection())
                {
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT config_package.is_rotation_enabled('" + machineCode + "') FROM dual";
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
    }
}
