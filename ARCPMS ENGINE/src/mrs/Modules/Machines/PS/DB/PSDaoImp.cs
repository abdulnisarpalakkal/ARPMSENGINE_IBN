using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.DBCon;
using Oracle.DataAccess.Client;
using System.Data;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.DB
{
    class PSDaoImp:PSDaoService
    {
        public List<Model.PSData> GetPSList()
        {
            List<Model.PSData> lstPSData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) // DA.Connection().getDBConnection())
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT PS_ID, PS_NAME,MACHINE_CODE,IS_BLOCKED,ACTUAL_MIN,ACTUAL_MAX,DYNAMIC_MIN,DYNAMIC_MAX,DYNAMIC_HOME,STATUS"
                                     + " ,MACHINE_CHANNEL,IS_SWITCH_OFF,ACTUAL_HOME"
                                     + " FROM L2_PS_MASTER";
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstPSData = new List<Model.PSData>();

                                while (reader.Read())
                                {

                                    Model.PSData objPSData = new Model.PSData();

                                    objPSData.psPkId = Int32.Parse(reader["PS_ID"].ToString());
                                    objPSData.psName = reader["PS_NAME"].ToString();
                                    objPSData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objPSData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString())==1;

                                    objPSData.actualMin = Int32.Parse(reader["ACTUAL_MIN"].ToString());
                                    objPSData.actualMax = Int32.Parse(reader["ACTUAL_MAX"].ToString());
                                    objPSData.dynamicMin = Int32.Parse(reader["DYNAMIC_MIN"].ToString());
                                    objPSData.dynamicMax = Int32.Parse(reader["DYNAMIC_MAX"].ToString());
                                    objPSData.dynamicHome = Int32.Parse(reader["DYNAMIC_HOME"].ToString());
                                    objPSData.status = Int32.Parse(reader["STATUS"].ToString());

                                    objPSData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objPSData.isSwitchOff = Int32.Parse(reader["IS_SWITCH_OFF"].ToString()) == 1;
                                    objPSData.actualHome = Int32.Parse(reader["ACTUAL_HOME"].ToString());


                                    lstPSData.Add(objPSData);
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
            return lstPSData;
        }

        public Model.PSData GetPSDetails(Model.PSData objPSData)
        {
            throw new NotImplementedException();
        }
        public Model.PSData GetPSDetailsIncludeAisle(int aisle)
        {
           Model.PSData objPSData = new Model.PSData();

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) // DA.Connection().getDBConnection())
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT PS_ID, PS_NAME,MACHINE_CODE,IS_BLOCKED,ACTUAL_MIN,ACTUAL_MAX,DYNAMIC_HOME,STATUS"
                                     + " ,MACHINE_CHANNEL,IS_SWITCH_OFF,ACTUAL_HOME,DYNAMIC_MIN,DYNAMIC_MAX"
                                     + " FROM L2_PS_MASTER"
                                     + " where "+aisle+" between DYNAMIC_MIN and DYNAMIC_MAX";
                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                               
                                    objPSData.psPkId = Int32.Parse(reader["PS_ID"].ToString());
                                    objPSData.psName = reader["PS_NAME"].ToString();
                                    objPSData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objPSData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString())==1;

                                    objPSData.actualMin = Int32.Parse(reader["ACTUAL_MIN"].ToString());
                                    objPSData.actualMax = Int32.Parse(reader["ACTUAL_MAX"].ToString());
                                    objPSData.dynamicHome = Int32.Parse(reader["DYNAMIC_HOME"].ToString());
                                    objPSData.status = Int32.Parse(reader["STATUS"].ToString());

                                    objPSData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objPSData.isSwitchOff = Int32.Parse(reader["IS_SWITCH_OFF"].ToString()) == 1;
                                    objPSData.actualHome = Int32.Parse(reader["ACTUAL_HOME"].ToString());

                                    objPSData.dynamicMin = Int32.Parse(reader["DYNAMIC_MIN"].ToString());
                                    objPSData.dynamicMax = Int32.Parse(reader["DYNAMIC_MAX"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            return objPSData;
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
                        qry = "UPDATE L2_PS_MASTER SET " + tableField + " = " + intDataValue
                               + " WHERE machine_code ='" + machineCode + "' and " + tableField + " != " + intDataValue;

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
                        qry = "UPDATE L2_PS_MASTER SET " + tableField + " = " + dataValue
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
                        qry = "UPDATE L2_PS_MASTER SET " + tableField + " = " + dataValue
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

        public bool IsPSBlockedInDB(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_BLOCKED FROM L2_PS_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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

        public int GetValidAisleForMoving(Model.PSData objPSData)
        {
            bool bOk = false;
            int aisle = 0;
         
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = con.CreateCommand())
                    {


                        bOk = true;
                    }
                }
            }
            catch (Exception errMsg)
            {
                throw new Exception(errMsg.Message);
            }

            return aisle;
        }

        public bool UpdateMachineBlockStatus(string machine_code, bool blockStatus)
        {
            bool bOk = false;
            try
            {

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "update L2_PS_MASTER set IS_BLOCKED ='" + (blockStatus ? 1 : 0) + "'"
                                  + " where MACHINE_CODE = '" + machine_code + "' and IS_BLOCKED !='" + (blockStatus ? 1 : 0) + "'";
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


        public bool IsPSDisabled(string machineName)
        {
            bool isDisable = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT status FROM L2_PS_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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


        public bool IsPSSwitchOff(string machineName)
        {
            bool isSwitchOff = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_SWITCH_OFF FROM L2_PS_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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


        
    }
}
