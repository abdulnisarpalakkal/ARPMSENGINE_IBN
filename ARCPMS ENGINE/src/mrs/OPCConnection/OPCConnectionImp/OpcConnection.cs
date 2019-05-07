using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using ARCPMS_ENGINE.src.mrs.Global;
using OPCDA;
using OPCDA.NET;
using OPC;

namespace ARCPMS_ENGINE.src.mrs.OPCConnection.OPCConnectionImp
{

    public static class OpcConnection
    {
        static string opcMachineHost;
        static string opcServerName = null;
        //static OpcThread SrvAccess = null;
        static OpcServer opcServer = null;

        static string camOPCMachineHost;
        static string camOPCServerName;
        static OpcServer camOPCServer = null;

        static object lockOpcServer = new object();
        static object lockCamOpcServer = new object();
        static object opcConLock = new object();

        // public OpcServer opcServer { get; set; }

        public static bool IsOpcServerConnectionAvailable()
        {
            if (opcServer == null) opcServer = new OpcServer();
            int rtc = 0;
            SERVERSTATUS objSERVERSTATUS = new SERVERSTATUS();
            bool isConnected = false;
            bool isServerRunning = true;


            try
            {
                isConnected = opcServer.isConnectedDA;
                if (isConnected)
                {
                    opcServer.GetStatus(out objSERVERSTATUS);
                    isServerRunning = objSERVERSTATUS.eServerState == OpcServerState.Running;
                }


                if (!isConnected || !isServerRunning)
                {
                    opcMachineHost = GlobalValues.OPC_MACHINE_HOST;
                    opcServerName = GlobalValues.OPC_SERVER_NAME;
                    rtc = opcServer.Connect(opcMachineHost, opcServerName);
                }
            }
            catch (Exception errMsg)
            {
                Console.WriteLine("" + errMsg);

            }
            finally { }
            return opcServer.isConnectedDA;
        }
        public static OpcServer GetOPCServerConnection()
        {


            if (opcServer == null) opcServer = new OpcServer();

            int rtc = 0;
            SERVERSTATUS objSERVERSTATUS = new SERVERSTATUS();
            bool isConnected = false;
            bool isServerRunning = true;

            lock (opcConLock)
            {

                do
                {
                    try
                    {
                        isConnected = opcServer.isConnectedDA;
                        if (isConnected)
                        {
                            opcServer.GetStatus(out objSERVERSTATUS);
                            isServerRunning = objSERVERSTATUS.eServerState == OpcServerState.Running;

                        }


                        if (!isConnected || !isServerRunning)
                        {
                            opcMachineHost = GlobalValues.OPC_MACHINE_HOST;
                            opcServerName = GlobalValues.OPC_SERVER_NAME;
                            rtc = opcServer.Connect(opcMachineHost, opcServerName);
                            if (!isServerRunning && IsOPCServerIsRunning())
                                new InitializeEngine().AsynchReadSettings();

                        }


                    }
                    catch (Exception errMsg)
                    {
                        Console.WriteLine("" + errMsg.Message);

                    }
                    finally { }

                } while (opcServer.isConnectedDA == false);
            }



            return opcServer;



        }
        static bool IsOPCServerIsRunning()
        {
            bool isRunning = false;
            SERVERSTATUS objSERVERSTATUS = new SERVERSTATUS();
            opcServer.GetStatus(out objSERVERSTATUS);
            isRunning = objSERVERSTATUS.eServerState == OpcServerState.Running;
            return isRunning;
        }
        //public void initializeSynchIfOPCStopped()
        //{
        //    if (!isServerRunning) //once igs server stopped, then all listener will stop working
        //        new InitializeEngine().AsynchReadSettings();
        //}
        //public static bool IsCamOpcServerConnectionAvailable()
        //{
        //    if (camOPCServer == null)
        //        camOPCServer = new OpcServer();

        //    int rtc = 0;
        //    try
        //    {

        //        if (camOPCServer.isConnectedDA == false)
        //        {
        //            camOPCMachineHost = GlobalValues.CAM_HOST_SERVER;
        //            camOPCServerName = GlobalValues.CAM_OPC_SERVER_NAME;
        //            rtc = camOPCServer.Connect(camOPCMachineHost, camOPCServerName);
        //        }

        //    }
        //    catch (Exception errMsg)
        //    {
        //        Console.WriteLine("" + errMsg);

        //    }
        //    finally { }



        //    return camOPCServer.isConnectedDA;
        //}
        //public static OpcServer GetCamOPCServerConnection()
        //{
        //    int rtc = 0;
        //    try
        //    {




        //        if (camOPCServer == null)
        //            camOPCServer = new OpcServer();

        //        do
        //        {
        //            try
        //            {


        //                if (camOPCServer.isConnectedDA == false)
        //                {
        //                    camOPCMachineHost = GlobalValues.CAM_HOST_SERVER;
        //                    camOPCServerName = GlobalValues.CAM_OPC_SERVER_NAME;

        //                    rtc = camOPCServer.Connect(camOPCMachineHost, camOPCServerName);


        //                }


        //            }
        //            catch (Exception errMsg)
        //            {

        //            }
        //            finally { }


        //        } while (camOPCServer.isConnectedDA == false);

        //    }
        //    catch (Exception errMsg)
        //    {

        //    }
        //    finally { }



        //    return camOPCServer;
        //}
        public static void StopOPCServer()
        {
            try
            {
                lock (lockOpcServer)
                {
                    if (opcServer != null && opcServer.isConnectedDA) opcServer.Disconnect();
                }

                //lock (lockCamOpcServer)
                //{
                //    if (camOPCServer != null)
                //    {
                //        if (camOPCServer.isConnectedDA) camOPCServer.Disconnect();
                //    }
                //}
            }
            finally { }
        }
    }
}
