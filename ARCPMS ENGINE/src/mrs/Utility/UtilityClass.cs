using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.Utility
{
    class UtilityClass
    {
        static object photoLock = new object();
        public void  CaptureImage(string ip, string imgPath)
        {
            // string sourceURL = "http://webcam.mmhk.cz/axis-cgi/jpg/image.cgi";
            string sourceURL = "http://" + ip + "/cgi-bin/viewer/video.jpg";
            byte[] buffer = new byte[100000];
            int read, total = 0;
            lock (photoLock)
            {
                try
                {
                    // create HTTP request
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sourceURL);
                    // get response
                    WebResponse resp = req.GetResponse();
                    // get response stream
                    Stream stream = resp.GetResponseStream();
                    // read data from stream
                    while ((read = stream.Read(buffer, total, 1000)) != 0)
                    {
                        total += read;
                    }
                    // get bitmap
                    Bitmap bmp = (Bitmap)Bitmap.FromStream(
                                  new MemoryStream(buffer, 0, total));
                    bmp.Save(imgPath);
                }
                catch(Exception ex)
                {

                }
            }
        }
    }
}
