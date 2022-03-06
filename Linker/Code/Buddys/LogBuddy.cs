using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.File.Header;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Linker
{
    /// <summary>
    /// Your buddy for logging stuff
    /// </summary>
    public static class LogBuddy
    {
        static Logger Logger { get; }

        private const string HEADER = "Linker Application log \n Created by Rob H \n ------------------------------------------------------------------------ \n";
        private const string OUTPUT_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss} \t [{Level}] \t {Message:lj}{NewLine}";

        static LogBuddy()
        {
            var logPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs", "AppLog.log");
            Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, outputTemplate: OUTPUT_TEMPLATE, hooks: new HeaderWriter(HEADER))
                .CreateLogger();
            
            Application.Current.Suspending += Current_Suspending;
        }

        /// <summary>
        /// Log a basic information log
        /// </summary>
        /// <param name="message">The message to pass</param>
        public static void Log(string message)
        {
            Logger.Information(message);
        }


        /// <summary>
        /// Log a message and supply what to log level
        /// </summary>
        /// <param name="message">The mesage to log</param>
        /// <param name="notifyType">Log event sevirity</param>
        public static void Log(string message, LogEventLevel notifyType)
        {
            Logger.Write(notifyType, message);
        }

        /// <summary>
        /// Log a message and supply what to log level
        /// </summary>
        /// <param name="logSource">Retrieves the name of the object so it is included in the log</param>
        /// <param name="message">The mesage to log</param>
        public static void Log(object logSource, string message)
        {
            if (logSource == null) 
                Logger.Information(message);
            else
                Logger.Information(string.Concat(nameof(logSource), ": ", message));
        }


        /// <summary>
        /// Log a message and supply what to log level
        /// </summary>
        /// <param name="logSource">Retrieves the name of the object so it is included in the log</param>
        /// <param name="notifyType">Log event sevirity<</param>
        /// <param name="message">The mesage to log</param>
        public static void Log(object logSource, LogEventLevel notifyType, string message)
        {
            if (logSource == null)
                Logger.Write(notifyType, message);
            else
                Logger.Write(notifyType, string.Concat(nameof(logSource), ": ", message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logSource">Retrieves the name of the object so it is included in the log</param>
        /// <param name="notifyType">Log event sevirity<</param>
        /// <param name="exception">Exception</param>
        /// <param name="message">The mesage to log</param>
        public static void Log(object logSource, LogEventLevel notifyType, Exception exception, string message)
        {
            if (logSource == null)
                Logger.Write(notifyType, exception, message);
            else
                Logger.Write(notifyType, exception, string.Concat(nameof(logSource), ": ", message));
        }


        private static void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Logger.Information("Application suspending");
        }
    }
}
