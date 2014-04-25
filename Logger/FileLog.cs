﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TraktRater.Settings;
using TraktRater.Web;
using System.Threading;

namespace TraktRater.Logger
{
    internal static class FileLog
    {
        private static Object lockObject = new object();

        private static string LogDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TraktRater\Logs";
        internal static string LogFileName { get; set; }

        static FileLog()
        {
            // default logging before we load settings
            AppSettings.LogSeverityLevel = AppSettings.LoggingSeverity.Debug;

            // create log directory if it doesn't exist
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            // listen to web events so we can provide useful logging            
            TraktWeb.OnDataSend += new TraktWeb.OnDataSendDelegate(WebRequest_OnDataSend);
            TraktWeb.OnDataReceived += new TraktWeb.OnDataReceivedDelegate(WebRequest_OnDataReceived);
            TraktWeb.OnDataErrorReceived += new TraktWeb.OnDataErrorReceivedDelegate(WebRequest_OnDataErrorReceived);
        }

        internal static void Info(String log)
        {
            if ((int)AppSettings.LogSeverityLevel >= 2)
                WriteToFile(String.Format(CreatePrefix(), "INFO", log));
        }

        internal static void Info(String format, params Object[] args)
        {
            Info(String.Format(format, args));
        }

        internal static void Debug(String log)
        {
            if ((int)AppSettings.LogSeverityLevel >= 3)
                WriteToFile(String.Format(CreatePrefix(), "DEBG", log));
        }

        internal static void Debug(String format, params Object[] args)
        {
            Debug(String.Format(format, args));
        }

        internal static void Error(String log)
        {
            if ((int)AppSettings.LogSeverityLevel >= 0)
                WriteToFile(String.Format(CreatePrefix(), "ERR ", log));
        }

        internal static void Error(String format, params Object[] args)
        {
            Error(String.Format(format, args));
        }

        internal static void Warning(String log)
        {
            if ((int)AppSettings.LogSeverityLevel >= 1)
                WriteToFile(String.Format(CreatePrefix(), "WARN", log));
        }

        internal static void Warning(String format, params Object[] args)
        {
            Warning(String.Format(format, args));
        }

        private static String CreatePrefix()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [{0}] " + String.Format("[{0}][{1}]", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(2, '0')) + ": {1}";
        }

        private static void WebRequest_OnDataSend(string address, string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                Debug("Address: {0}, Post: {1}", address, data);
            }
            else
            {
                Debug("Address: {0}", address);
            }
        }

        private static void WebRequest_OnDataReceived(string response)
        {
            Debug("Response: {0}", response ?? "null");
        }

        private static void WebRequest_OnDataErrorReceived(string error)
        {
            Error("Response: {0}", error ?? "null");
        }

        private static void WriteToFile(String log)
        {
            string filename = LogDirectory + "\\" + LogFileName;

            try
            {
                lock (lockObject)
                {
                    StreamWriter sw = File.AppendText(filename);
                    sw.WriteLine(log);
                    sw.Close();
                }
            }
            catch
            {
                //Error("Failed to write out to log");
            }
        }
    }
}
