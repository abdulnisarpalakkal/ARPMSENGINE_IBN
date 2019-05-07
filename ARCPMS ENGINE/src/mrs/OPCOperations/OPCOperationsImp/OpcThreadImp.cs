using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OPCDA.NET;
using OPC;

namespace ARCPMS_ENGINE.src.mrs.OPCOperations.OPCOperationsImp
{
    public class OpcThreadImp : OpcThreadService, IDisposable
    {
        OpcServer OpcSrv;
        SyncIOGroup SioGrp = null;
        public OpcThreadImp(OpcServer srv)
        {
           
            try
            {
                OpcSrv = srv;
                SioGrp = OpcSrv.AddSyncIOGroup();
            }
            catch (Exception errmsg)
            {
              
            }
         }
        public bool Request(string req,object val)
        {
            bool bOK = false;
            //ErrorReportEngine rptEngine = new ErrorReportEngine();

            if (SioGrp == null) return false;
            try
            {
                int rtc = SioGrp.Write(req, val);
                if (HRESULTS.Failed(rtc))
                {
                    bOK = false;

                    // MethodDebug(req.ItemID);

                    //Console.WriteLine("Write failed with error 0x" + rtc.ToString("X"));
                    // rptEngine.TESTPURPOSEOPCLOG("Write failed with error 0x" + rtc.ToString("X"));
                }
                else
                {
                    bOK = true;
                }
            }
            catch (Exception errmsg)
            {

                bOK = false;
            
            }
            return bOK;
        }
        public void Stop()
        {
            if (SioGrp == null)
                return;

            SioGrp.Dispose();
            SioGrp = null;
           
        }
        public void Dispose()
        {
            if (this.SioGrp != null)
            {
                this.SioGrp.Dispose();
                SioGrp = null;
            }
        }

    }

}
