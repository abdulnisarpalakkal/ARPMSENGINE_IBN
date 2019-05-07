using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPCDA.NET;
using ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp;
using OPCDA;
namespace ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp
{
    class OpcOperationsImp : OpcOperationsService//,IDisposable
    {
        OpcServer objOpcServer = null;
        OpcServer camOpcServer = null;
        public OpcOperationsImp(OpcServer objOpcServer)
        {
            this.objOpcServer = objOpcServer;
        }
        public T ReadTag<T>(string channel, string machine, string tagName)
        {

            T result = default(T);
           
            string command = channel + "." + machine + "." + tagName;
            objOpcServer = OpcConnection.GetOPCServerConnection();
            

                try
                {

                    if (IsMachineHealthy(command))
                    {
                        ItemProperties[] item = null;

                        //get value of command.
                        item = objOpcServer.GetProperties(new string[1] { command }, true, new int[] { 2 });
                        Property property = item[0].Properties[0];
                        result = (T)Convert.ChangeType(property.Value, typeof(T));


                    }
                    else
                    {
                        //MethodDebug("OPC READ = MACHINE IS NOT HEALTHY, COMMAND =" + command);

                    }

                }

                catch (Exception errMsg)
                {
                    Console.WriteLine(errMsg.Message);
                }
                finally
                {

                }


                return result;
        }

        public bool WriteTag<T>(string channel, string machineName, string tagName, T value)
        {
          
            bool bOk = false;
          
            objOpcServer = OpcConnection.GetOPCServerConnection();
            OpcThreadService opcthread = new OpcThreadImp(objOpcServer);
            string instruction=channel +"."+ machineName+"."+tagName;

            try
            {
                if (IsMachineHealthy(instruction))
                {
                    bOk = opcthread.Request(instruction, value);
                }
              
            }
            catch (Exception errMsg)
            {
               
                bOk = false;

            }
            finally
            {

            }
            return bOk;
        }

        //public T ReadTagFromCamOpc<T>(string tagName)
        //{
        //    T result = default(T);

        //    string command = tagName;
        //    camOpcServer = OpcConnection.GetCamOPCServerConnection();

        //    try
        //    {

        //        if (IsCameraHealthy(command))
        //        {
        //            ItemProperties[] item = null;

        //            //get value of command.
        //            item = objOpcServer.GetProperties(new string[1] { command }, true, new int[] { 2 });
        //            Property property = item[0].Properties[0];
        //            result = (T)property.Value;


        //        }
        //        else
        //        {


        //        }

        //    }

        //    catch (Exception errMsg)
        //    {

        //    }
        //    finally
        //    {

        //    }


        //    return result;
        //}

        //public bool WriteTagToCamOpc<T>(string tagName, T value)
        //{
        //    bool bOk = false;

        //    camOpcServer = OpcConnection.GetCamOPCServerConnection();
        //    OpcThreadService opcthread = new OpcThreadImp(objOpcServer);
        //    string instruction = tagName;

        //    try
        //    {
        //        if (IsCameraHealthy(instruction))
        //        {
        //            bOk = opcthread.Request(instruction, value);
        //        }

        //    }
        //    catch (Exception errMsg)
        //    {

        //        bOk = false;

        //    }
        //    finally
        //    {

        //    }
        //    return bOk;
        //}


        public bool IsMachineHealthy(string command)
        {
            bool result = false;
            //qualityBits machineQuality = qualityBits.bad;

            try
            {
                objOpcServer = OpcConnection.GetOPCServerConnection();
                ItemProperties[] item = null;


                //get value of command.
                item = objOpcServer.GetProperties(new string[1] { command }, true, new int[] { 3 });

                Property property = item[0].Properties[0];
                result = ((OPCDA.OPCQuality)property.Value).QualityField == OPCDA.qualityBits.good;

               
            }
            catch (Exception errMsg)
            {
                result = false;
               // Console.WriteLine(errMsg.Message);
            }

            finally
            {
                //Console.WriteLine(command);
            }


            return result;


           
        }
        public bool IsCameraHealthy(string command)
        {
            bool result = false;
            //qualityBits machineQuality = qualityBits.bad;

            try
            {
                camOpcServer = OpcConnection.GetOPCServerConnection();
                ItemProperties[] item = null;


                //get value of command.
                item = objOpcServer.GetProperties(new string[1] { command }, true, new int[] { 3 });

                Property property = item[0].Properties[0];
                result = ((OPCDA.OPCQuality)property.Value).QualityField == OPCDA.qualityBits.good;

               
            }
            catch (Exception errMsg)
            {
                result = false;
                Console.WriteLine(errMsg.Message);
            }

            finally
            {
                //Console.WriteLine(command);
            }


            return result;


           
        }
        

        public void Dispose()
        {
          //  throw new NotImplementedException();
        }
    }
}
