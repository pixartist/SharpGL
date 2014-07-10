using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL
{
    public static class Log
    {
		public static bool ShowDebug { get; set; }
        public static void Error(string error)
        {
            if(!error.EndsWith("NoError") && error.Length > 0)
                Console.WriteLine("[ERROR]: " + error);
        }
        public static void Write(string message)
        {
            Console.WriteLine("[LOG]: " + message);
        }
		public static void Debug(string message)
		{
			if(ShowDebug)
				Console.WriteLine("[LOG]: " + message);
		}
    }
}
