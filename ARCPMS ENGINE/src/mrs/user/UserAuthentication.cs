using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARCPMS_ENGINE.src.mrs.DBCon;
using Oracle.DataAccess.Client;
using System.Data;
using ARCPMS_ENGINE.src.mrs.DBCon;

namespace ARCPMS_ENGINE.src.mrs.user
{
    class UserAuthentication: UserAuthenticationService
    {
        /// <summary>
        /// validate user credentials
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateUser(string userName, string password)
        {



            string query = "select USER_PK_ID from L2_USER_DATA where USER_ID= '" + userName + "'"
                                                     + "  and PASSWORD = '" + password + "'";

            bool isValidate = false;
            try
            {

                using (OracleConnection con = new UserDBConnection().getDBConnection())
                {


                    using (OracleCommand command = new OracleCommand(query))
                    {
                        command.CommandText = query;
                        command.Connection = con;

                        //int.TryParse(command.ExecuteScalar().ToString(), out val);
                        using (OracleDataReader dreader = command.ExecuteReader())
                        {
                            isValidate = dreader.HasRows;
                            
                        }

                    }
                }


            }
            catch (Exception errMsg)
            {
                Console.WriteLine(errMsg);

            }
            return isValidate;
        }
    }
}
