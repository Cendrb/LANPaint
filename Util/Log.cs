using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Util
{
    public static class Log
    {
        private static string getTimePrefix()
        {
            return DateTime.Now.ToString("[HH:mm:ss.fff]: ");
        }

        public static void Debug(string message)
        {
            Console.WriteLine(getTimePrefix() + message);
        }

        public static void Warning(string message)
        {
            Console.WriteLine(getTimePrefix() + message);
        }

        public static void Error(string message)
        {
            Console.WriteLine(getTimePrefix() + message);
        }

        public static void Error(Exception e)
        {
            Console.WriteLine(getTimePrefix() + e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}
