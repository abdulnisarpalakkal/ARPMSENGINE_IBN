using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.Manager.ParkingManager.Model;
using ARCPMS_ENGINE.src.mrs.DBCon;
using Oracle.DataAccess.Client;
using System.Data;

namespace ARCPMS_ENGINE.src.mrs.Manager.ClickTransferManager.DB
{
    class ClickTransferDaoImp:ClickTransferDaoService
    {
        public bool GetInitialTransferPath(int queueId)
        {
            bool success = false;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = new OracleCommand())
                    {

                        //Allocate slot.
                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "CLICK_TRANSFER_PACKAGE.find_transfer_path_first";
                        command.Parameters.Add("transfer_q_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
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

        public int GetDynamicTransferPath(int queueId)
        {
            int pathId = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = new OracleCommand())
                    {

                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "CLICK_TRANSFER_PACKAGE.find_transfer_path_second";
                        command.Parameters.Add("transfer_q_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.Parameters.Add("transfer_path_id", OracleDbType.Int64, pathId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["transfer_path_id"].Value.ToString(), out pathId);
                    }
                }


               
            }
            catch (Exception errMsg)
            {
            }
            return pathId;
        }
        /// <summary>
        /// reallocate Path
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public int GetAllocatePath(int queueId)
        {
            int pathId = 0;
            try
            {
                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    using (OracleCommand command = new OracleCommand())
                    {

                        command.Connection = con;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "CLICK_TRANSFER_PACKAGE.find_transfer_path";
                        command.Parameters.Add("transfer_q_id", OracleDbType.Int64, queueId, ParameterDirection.Input);
                        command.Parameters.Add("transfer_path_id", OracleDbType.Int64, pathId, ParameterDirection.Output);
                        command.ExecuteNonQuery();
                        int.TryParse(command.Parameters["transfer_path_id"].Value.ToString(), out pathId);
                    }
                }



            }
            catch (Exception errMsg)
            {
            }
            return pathId;
        }
        public bool UpdateAfterTransfer(int queueId)
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
                        command.CommandText = "CLICK_TRANSFER_PACKAGE.update_after_click_transfer";
                        command.Parameters.Add("transfer_q_id", OracleDbType.Int32, queueId, ParameterDirection.Input);
                        command.ExecuteNonQuery();
                        success = true;
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
    }
}
