using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    public static class Utils
    {
        public static string TellMeEverything(this Exception ex)
        {
            return ex.InnerException?.TellMeEverything() + ex.Message + ex.StackTrace;
        }
    }
}
