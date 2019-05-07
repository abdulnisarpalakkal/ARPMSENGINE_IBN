using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ARCPMS_ENGINE.src.mrs.Config
{
    class Logger
    {
        static object fileLock = new object();

        /// <summary>
        /// write data into the log file
        /// </summary>
        /// <param name="logFileName"></param>
        /// <param name="lines"></param>
        public static void WriteLogger(String logFileName,String lines)
        {

            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log
            String logPath = null;
            try
            {
                string apLocation = BasicConfig.GetApplicationLocation();
                logPath = "C:\\logs\\";
                string dir = logPath + logFileName + "\\";

                //if dir is not exists, create directory

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string fileName = logFileName + "_" + System.DateTime.Now.Date.Date.Day + "_" + System.DateTime.Now.Date.Date.Month + "_" + System.DateTime.Now.Date.Date.Year + ".txt";
                lock (fileLock)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@dir + fileName, true))
                    {
                        if (file != null)
                        {
                            file.WriteLine(System.DateTime.Now + " : " + lines
                                + System.Environment.NewLine + "-------------------------------------------" + System.Environment.NewLine);
                            file.Flush();
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {

            }
            catch (Exception ex)
            {

            }

        }
    }
}
