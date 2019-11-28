using System;
using System.IO;
using System.Threading;
using YobiWi.Development.Models;

namespace Common
{
    public static class Log
    {
        public static LogLevel Logging = LogLevel.DEBUG;
        private static string PathLogs = Directory.GetCurrentDirectory() + "/logs/";
        private static string FileName = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
        private static string FullPathLog = PathLogs + FileName;
        private static DateTime CurrentFileDate = DateTime.Now;
        private static string UserComputer = Environment.UserName + "-" + Environment.MachineName;
        private static StreamWriter Writer;
        public static YobiWiContext context = new YobiWiContext();
        public static Semaphore blocker = new Semaphore(1,1);

        public static void WriteLogMessage(LogMessage log)
        {
            if (Logging != LogLevel.OFF)
            {
                if (!string.IsNullOrEmpty(log.message))
                {
                    if (log.message.Length > 2000)
                    {
                        log.message = log.message.Substring(0, 2000);
                    }
                }
                else
                {
                    Error("Log->message - is null or emply, call WriteLogMessage()");
                    return;
                }
                log.userComputer = UserComputer;
                log.createdAt = DateTime.Now;
                log.threadId = Thread.CurrentThread.ManagedThreadId;
                if (Config.logFiles)
                {
                    ChangeLogFile(log.createdAt);
                    System.Diagnostics.Debug.WriteLine(log.message); 
                    Writer.WriteAsync
                    (
                        "Created at: " + log.createdAt + " | " +
                        log.level + " | " +
                        "Message: " + log.message  + " | " + 
                        "UID: " + log.userId + " | " +
                        "UIP: " + log.userIP + " | " +
                        "TID: " + log.threadId + " | " +
                        "UPC: " + log.userComputer + " | " +
                        "\r\n"
                    );
                    Writer.Flush();
                }
                blocker.WaitOne();
                context.Logs.AddAsync(log);
                context.SaveChanges();
                blocker.Release();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(log.message);
            }   
        }
        private static void ChangeLogFile(DateTime local)
        {
            if (!Directory.Exists(PathLogs))
            {
                Directory.CreateDirectory(PathLogs);
            }
            if (!File.Exists(FullPathLog) || local.Day != CurrentFileDate.Day || Writer == null)
            {
                if (Writer != null)
                {
                    Writer.Dispose();
                }
                CurrentFileDate = local;
                FileName = CurrentFileDate.Day + "-" + CurrentFileDate.Month + "-" + CurrentFileDate.Year;
                FullPathLog = PathLogs + FileName;
                if (File.Exists(FullPathLog))
                {
                    Writer = new StreamWriter(FullPathLog, true);
                }
                else
                {
                    Writer = File.CreateText(FullPathLog);
                }
            }
        }
        public static void Trace(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "trace",
            };
            WriteLogMessage(log);
        }
        public static void Debug(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "debug",
            };
            WriteLogMessage(log);
        }
        public static void Info(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "info"
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "info",
                userIP = ip
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "info",
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, string ip, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "info",
                userIP = ip,
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "Warn",
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "Warn",
                userIP = ip
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "Warn",
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, string ip, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "Warn",
                userIP = ip,
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Error(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
            };
            WriteLogMessage(log);
        }
        public static void Error(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
                userIP = ip
            };
            WriteLogMessage(log);
        }
        public static void Error(string message, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Error(string message, string ip, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
                userIP = ip,
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
                userIP = ip
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message, string ip, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
                userIP = ip,
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Off()
        {
            Logging = LogLevel.OFF;
        }
        private static string SetLevelLog(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.DEBUG: return "debug";
                case LogLevel.ERROR: return "error";
                case LogLevel.FATAL: return "fatal";
                case LogLevel.INFO: return "info";
                case LogLevel.OFF: return "off";
                case LogLevel.TRACE: return "trace";
                case LogLevel.WARN: return "warn";
                default: return "fatal";
            }
        }
    }
}
