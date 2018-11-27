using Elmah;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeaconCCGManagement.Elmah
{
    public static class ElmahService
    {
        public static void LogException(Exception exception)
        {
            try
            {
                // Log caught exception with Elmah
                ErrorSignal.FromCurrentContext().Raise(exception);
            }
            catch (Exception ex)
            {
                // Ignore exception: If exception here then Elmah can't log                
            }
        }
    }
}