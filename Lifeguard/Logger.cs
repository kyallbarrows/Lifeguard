using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifeguard
{
    class Logger
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void LogException(Exception e) {
            if (e is AggregateException)
            {
                e = (e as AggregateException).Flatten();
            }

            log.Error(e.ToString());
        }

        public static void LogError(string error) {
            log.Error(error);
        }
    }

}
