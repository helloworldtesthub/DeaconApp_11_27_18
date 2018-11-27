using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace DeaconCCGManagement.AzureFunctions
{
    public class AzureFunct
    {
        string elmahPath;
        public AzureFunct(string elmahPath)
        {
            this.elmahPath = elmahPath;
        }

        public void CleanElmahLog()
        {
            
            
            string[] logs = Directory.GetFiles(elmahPath);
       

            string logName;
            foreach (var log in logs)
            {
                logName = Path.GetFileNameWithoutExtension(log);
                DateTime dt = GetLogDate(logName);
                if (dt < DateTime.Now.Subtract(new TimeSpan(90, 0, 0, 0, 0)))
                {
                    try
                    {
                        if (File.Exists(log))
                            File.Delete(log);
                    }
                    catch (Exception)
                    {

                    }
                    
                }
            }

            
        }

        private DateTime GetLogDate(string log)
        {

            // ex: error-2018-06-21124221Z-7e8923d6-e59b-4f53-bf58-c237cd00bdb1

            string[] splitLog = log.Split('-');
            int year = int.Parse(splitLog[1]);
            int month = int.Parse(splitLog[2]);
            return new DateTime(year, month, 1);
        }
    }
}