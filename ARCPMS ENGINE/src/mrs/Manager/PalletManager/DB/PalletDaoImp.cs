using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Data;
using ARCPMS_ENGINE.src.mrs.DBCon;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PST.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PS.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.PVL.Model;
using ARCPMS_ENGINE.src.mrs.Modules.Machines.EES.Model;


namespace ARCPMS_ENGINE.src.mrs.Manager.PalletManager.DB
{
    class PalletDaoImp:PalletDaoService
    {
        public bool IsPMSInL2()
        {
            bool isInL2 = false;
            try
            {
                int bResult = 0;

                using (OracleConnection con = new DBConnection().getDBConnection())
                {
                  
                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT value FROM L2_CONFIG_MASTER WHERE MODULE_NAME ='SetPoints' and ITEM_NAME='L2AutoSchedle'";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    bResult = Convert.ToInt32(command.ExecuteScalar());
                    isInL2 = bResult == 1;
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return isInL2;
        }
        public int GetCurrentPMSMode()
        {
            int pmsMode = 0;
            try
            {
              

                using (OracleConnection con = new DBConnection().getDBConnection())
                {

                    OracleCommand command = con.CreateCommand();
                    string sql = "SELECT CURRENT_MODE FROM CURRENT_PMS_MODE ";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    pmsMode = Convert.ToInt32(command.ExecuteScalar());
                    if(pmsMode==0) pmsMode=3;
                    
                }
            }
            catch (Exception errMsg)
            {
                pmsMode = 3;
                Console.WriteLine(errMsg.Message);
            }
            finally
            {
            }

            return pmsMode;
        }



        public PSData FindPSForEES(Modules.Machines.EES.Model.EESData objEESData)
        {
            throw new NotImplementedException();
        }

        public PSTData FindPSTForPS(Modules.Machines.PS.Model.PSData objPSData)
        {
            throw new NotImplementedException();
        }

        public PVLData FindPVLForPST(Modules.Machines.PST.Model.PSTData objPSTData)
        {
            throw new NotImplementedException();
        }
    }
}
