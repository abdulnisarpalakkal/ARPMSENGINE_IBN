using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.DBCon;
using Oracle.DataAccess.Client;
using System.Data;

namespace ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.DB
{
    class PSTDaoImp:PSTDaoService
    {
        public List<Model.PSTData> GetPSTList()
        {
            List<Model.PSTData> lstPSTData = null;

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) 
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT PST_ID, PST_NAME,AISLE_NO,ROW_NO,QUANTITY,MACHINE_CODE,IS_BLOCKED,STATUS"
                                     + " ,MACHINE_CHANNEL,IS_SWITCH_OFF,PVL_ID"
                                     + " FROM L2_PST_MASTER";

                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                lstPSTData = new List<Model.PSTData>();

                                while (reader.Read())
                                {

                                    Model.PSTData objPSTData = new Model.PSTData();

                                    objPSTData.pstPkId = Int32.Parse(reader["PST_ID"].ToString());
                                    objPSTData.pstName = reader["PST_NAME"].ToString();
                                    objPSTData.aisle = Int32.Parse(reader["AISLE_NO"].ToString());
                                    objPSTData.row = Int32.Parse(reader["ROW_NO"].ToString()) ;

                                    objPSTData.quantity = Int32.Parse(reader["QUANTITY"].ToString());
                                    objPSTData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objPSTData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString())==1;
                                    objPSTData.status = Int32.Parse(reader["STATUS"].ToString());

                                    objPSTData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objPSTData.isSwitchOff = Int32.Parse(reader["IS_SWITCH_OFF"].ToString()) == 1;
                                    objPSTData.pvlPkId = Int32.Parse(reader["PVL_ID"].ToString());


                                    lstPSTData.Add(objPSTData);
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
            return lstPSTData;
        }

        public Model.PSTData GetPSTDetails(Model.PSTData objPSTData)
        {
         

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT PST_ID, PST_NAME,AISLE_NO,ROW_NO,QUANTITY,MACHINE_CODE,IS_BLOCKED,STATUS"
                                     + " ,MACHINE_CHANNEL,IS_SWITCH_OFF,PVL_ID"
                                     + " FROM L2_PST_MASTER"
                                     + " Where ";
                        if(objPSTData.pvlPkId!=0)
                            sql += " PVL_ID= " + objPSTData.pvlPkId;

                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                               
                                while (reader.Read())
                                {

                                    objPSTData.pstPkId = Int32.Parse(reader["PST_ID"].ToString());
                                    objPSTData.pstName = reader["PST_NAME"].ToString();
                                    objPSTData.aisle = Int32.Parse(reader["AISLE_NO"].ToString());
                                    objPSTData.row = Int32.Parse(reader["ROW_NO"].ToString());

                                    objPSTData.quantity = Int32.Parse(reader["QUANTITY"].ToString());
                                    objPSTData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objPSTData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString()) == 1;
                                    objPSTData.status = Int32.Parse(reader["STATUS"].ToString());

                                    objPSTData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objPSTData.isSwitchOff = Int32.Parse(reader["IS_SWITCH_OFF"].ToString()) == 1;
                                    objPSTData.pvlPkId = Int32.Parse(reader["PVL_ID"].ToString());

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
            return objPSTData;
        }

        public Model.PSTData GetPSTDetailsInRange(int minAisle, int maxAisle)
        {
            Model.PSTData objPSTData = new Model.PSTData();

            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection()) 
                {
                    // if (con.State == System.Data.ConnectionState.Closed) con.Open();
                    using (OracleCommand command = con.CreateCommand())
                    {
                        string sql = "SELECT PST_ID, PST_NAME,AISLE_NO,ROW_NO,QUANTITY,MACHINE_CODE,IS_BLOCKED,STATUS"
                                     + " ,MACHINE_CHANNEL,IS_SWITCH_OFF,PVL_ID"
                                     + " FROM L2_PST_MASTER"
                                     + " WHERE AISLE_NO between " + minAisle + " and " + maxAisle
                                     + " and AISLE_NO between " + maxAisle + " and " + minAisle
                                     + " and STATUS=2 and rownum=1";

                        command.CommandText = sql;
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                               
                                if(reader.Read())
                                {

                                   

                                    objPSTData.pstPkId = Int32.Parse(reader["PST_ID"].ToString());
                                    objPSTData.pstName = reader["PST_NAME"].ToString();
                                    objPSTData.aisle = Int32.Parse(reader["AISLE_NO"].ToString());
                                    objPSTData.row = Int32.Parse(reader["ROW_NO"].ToString()) ;

                                    objPSTData.quantity = Int32.Parse(reader["QUANTITY"].ToString());
                                    objPSTData.machineCode = reader["MACHINE_CODE"].ToString();
                                    objPSTData.isBlocked = Int32.Parse(reader["IS_BLOCKED"].ToString())==1;
                                    objPSTData.status = Int32.Parse(reader["STATUS"].ToString());

                                    objPSTData.machineChannel = reader["MACHINE_CHANNEL"].ToString();
                                    objPSTData.isSwitchOff = Int32.Parse(reader["IS_SWITCH_OFF"].ToString()) == 1;
                                    objPSTData.pvlPkId = Int32.Parse(reader["PVL_ID"].ToString());

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
            return objPSTData;
        }
        public bool IsPSTBlockedInDB(string machineName)
        {
            bool bOk = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_BLOCKED FROM L2_PST_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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
                    string sql = "update L2_PST_MASTER set IS_BLOCKED ='" + (blockStatus ? 1 : 0) + "'"
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


        public bool IsPSTDisabled(string machineName)
        {
            bool isDisable = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT status FROM L2_PST_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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


        public bool IsPSTSwitchOff(string machineName)
        {
            bool isSwitchOff = true;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT IS_SWITCH_OFF FROM L2_PST_MASTER WHERE MACHINE_CODE ='" + machineName + "'";
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
