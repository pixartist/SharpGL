using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL
{
    /// <summary>
    /// Logging helper class
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// If true, debug messages will be logged
        /// </summary>
        public static bool ShowDebug { get; set; }
        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="error">The error message</param>
        public static void Error(string error)
        {
            if (!error.EndsWith("NoError") && error.Length > 0)
                Console.WriteLine("[ERROR]: " + error);
        }
        /// <summary>
        /// Logs a text
        /// </summary>
        /// <param name="message">The text message</param>
        public static void Write(string message)
        {
            Console.WriteLine("[LOG]: " + message);
        }
        /// <summary>
        /// Logs a debug message if ShowDebug is set to true
        /// </summary>
        /// <param name="message"The debug message></param>
        public static void Debug(string message)
        {
            if (ShowDebug)
                Console.WriteLine("[LOG]: " + message);
        }
    }
}
