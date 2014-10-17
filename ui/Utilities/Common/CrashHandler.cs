using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Windows;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using BackItUp.NLogger;

namespace Utilities.Common
{
    internal enum MINIDUMP_TYPE
    {
        MiniDumpNormal = 0x00000000,
        MiniDumpWithDataSegs = 0x00000001,
        MiniDumpWithFullMemory = 0x00000002,
        MiniDumpWithHandleData = 0x00000004,
        MiniDumpFilterMemory = 0x00000008,
        MiniDumpScanMemory = 0x00000010,
        MiniDumpWithUnloadedModules = 0x00000020,
        MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
        MiniDumpFilterModulePaths = 0x00000080,
        MiniDumpWithProcessThreadData = 0x00000100,
        MiniDumpWithPrivateReadWriteMemory = 0x00000200,
        MiniDumpWithoutOptionalData = 0x00000400,
        MiniDumpWithFullMemoryInfo = 0x00000800,
        MiniDumpWithThreadInfo = 0x00001000,
        MiniDumpWithCodeSegs = 0x00002000
    }

    internal static class NativeMethods
    {
        [DllImport("dbghelp.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            Int32 ProcessId,
            IntPtr hFile,
            MINIDUMP_TYPE DumpType,
            IntPtr ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallackParam);
    }

    public sealed class MiniDumpCreator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
        public static void MiniDumpToFile(String fileToDump)
        {
            FileStream fsToDump = null;
            if (File.Exists(fileToDump))
                fsToDump = File.Open(fileToDump, FileMode.Append);
            else
                fsToDump = File.Create(fileToDump);
            Process thisProcess = Process.GetCurrentProcess();
            var writeMiniDumpThread = new Thread(() =>
            {
                NativeMethods.MiniDumpWriteDump(thisProcess.Handle, thisProcess.Id,
                    fsToDump.SafeFileHandle.DangerousGetHandle(), MINIDUMP_TYPE.MiniDumpNormal,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                fsToDump.Close();
            }
            );
            writeMiniDumpThread.Start();
            writeMiniDumpThread.Join();
        }

        private MiniDumpCreator() { }
    };

    public enum CrashHandlingCode
    {
        DoNothing,
        ReportImmediatelyDoNotAsk,    // send the crash report when it happens without asking
        AskFirstAndReportImmediately, // ask and send the crash report when it happens
        ReportNextTimeDoNotAsk,       // send the crash report by the next start
        AskFirstAndReportNextTime     // ask by the next start and send the crash report
    }

    public class Logs
    {
        private static string fileName = "NeroBackItUp.txt";
        private static string filePath = string.Empty;
        private static string folderPath = System.Environment.ExpandEnvironmentVariables(@"%appdata%\Nero\Nero 15\Nero BackItUp\Cache\");
        /// <summary>
        /// write log to cache 
        /// </summary>
        /// <param name="content"></param>
        //public static void LogUnhandledException(Exception ex)
        //{
        //    try
        //    {
        //        StringBuilder builder = new StringBuilder();

        //        builder.AppendLine("Unhandled exceptions has occurred at " + DateTime.Now);
        //        builder.AppendLine("Application:");
        //        builder.AppendLine(ConstString.AppTitle);

        //        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        //        FileVersionInfo fvi = assembly == null ? null : FileVersionInfo.GetVersionInfo(assembly.Location);
        //        builder.AppendLine("Version:");
        //        builder.AppendLine(assembly == null ? "Unkown" : fvi.FileVersion);

        //        while (ex != null)
        //        {
        //            builder.AppendLine("-------------------Begin of exception -------------------");
        //            builder.AppendLine("Message:");
        //            builder.AppendLine(ex.Message);

        //            builder.AppendLine("Source:");
        //            builder.AppendLine(ex.Source);

        //            builder.AppendLine("StackTrace:");
        //            builder.AppendLine(ex.StackTrace);

        //            builder.AppendLine("Type:");
        //            builder.AppendLine(ex.GetType().ToString());
        //            builder.AppendLine("-------------------End of exception -------------------");


        //            if (ex.InnerException == null)
        //                break;
        //            ex = ex.InnerException;
        //        }

        //        string exception = builder.ToString();
        //        Trace.WriteLine(exception);

        //        //UtilityFunctions.WriteToEventLog(null, ConstString.AppTitle, exception, EventLogEntryType.Error);

        //        fileName = DateTime.Now.ToString("yyyyMMdd hhmmss") + ".txt";
        //        //string path = Path.Combine(string.IsNullOrEmpty(App.LocalPreferences.CachePath) ?
        //        //    System.Environment.ExpandEnvironmentVariables(@"%appdata%\Nero\Nero 2014\Nero BackItUp\Cache\") : App.LocalPreferences.CachePath,
        //        //    "UnhandledException");

        //        if (!Directory.Exists(folderPath))
        //        {
        //            Directory.CreateDirectory(folderPath);
        //        }

        //        filePath = Path.Combine(folderPath, fileName);
        //        File.WriteAllText(filePath, exception);
        //    }
        //    catch (System.Exception e)
        //    {
        //        //App.RaiseExceptionIgnoredEvent(e, new StackFrame());
        //        App.RaiseExceptionIgnoredEvent(e);
        //    }
        //}

        public static string GetFilePath()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = Path.Combine(folderPath, fileName);
            }

            return filePath;
        }
    }

    /// <summary>
    /// Implement this interface in App, will call it when unhandled exception occurs
    /// </summary>
    public interface IUnhandledExceptionHandler
    {
        string AdditionalLog { get; }
    }

    public sealed class CrashHandler
    {
        private const String LogDirectoryName = "BIUPC_Logs";
        private const String DumpDirectoryName = "BIUPC_Dumps";
        private const String OfficialPackage = "com.nero.backitup";
        private const String OfficialAppId = "f57c1a55f7d514ec2dacaf91c1c77ab1";

        private static readonly CrashHandler instance = new CrashHandler();

        private static CrashHandlingCode crashHandlingMode = CrashHandlingCode.AskFirstAndReportImmediately;
        private static string packageName = OfficialPackage;
        private static string appID = OfficialAppId;

        static CrashHandler() { }
        private CrashHandler()
        {
        }

        private static string GetTempFolder(string subfolder)
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), subfolder);
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            return tempFolder;
        }

        private static DateTime CrashTime
        {
            get
            {
                if (crashTime == DateTime.MinValue)
                    crashTime = DateTime.Now;
                return crashTime;
            }
        }
        private static DateTime crashTime = DateTime.MinValue;

        private static string GetDumpFileName()
        {
            return Path.Combine(GetTempFolder(DumpDirectoryName), "BIUPC_" + CrashTime.ToString("yyyy_MM_dd__HH_mm_ss", CultureInfo.InvariantCulture) + ".dmp");
        }

        private static string GetDatabaseDumpFileName()
        {
            return Path.Combine(GetTempFolder(DumpDirectoryName), "BIUPC_" + CrashTime.ToString("yyyy_MM_dd__HH_mm_ss", CultureInfo.InvariantCulture) + ".KML");
        }

        private static string Get2ndLogfileDumpFileName()
        {
            return Path.Combine(GetTempFolder(DumpDirectoryName), "BIUPC_" + CrashTime.ToString("yyyy_MM_dd__HH_mm_ss", CultureInfo.InvariantCulture) + ".log");
        }

        private static string GetReportFileName()
        {
            return Path.Combine(GetTempFolder(LogDirectoryName), "BIUPC_" + CrashTime.ToString("yyyy_MM_dd__HH_mm_ss", CultureInfo.InvariantCulture) + ".log");
        }

        public static CrashHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public void SetMode(CrashHandlingCode mode, string package, string id)
        {
            crashHandlingMode = mode;
            packageName = package;
            appID = id;

            if (mode != CrashHandlingCode.DoNothing)
            {
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionHandler;
            }

            HandleCrashes(true);
        }

        // CompressFile with GZipStream works, but the compression ratios are miserable
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static bool CompressFile(string sourceFile, string targetFile)
        {
            try
            {
                using (FileStream srcStream = File.OpenRead(sourceFile))
                using (GZipStream gz = new GZipStream(File.OpenWrite(targetFile), CompressionMode.Compress))
                {
                    srcStream.CopyTo(gz);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static string CreateCrashDump()
        {
            string dumpName = GetDumpFileName();
            MiniDumpCreator.MiniDumpToFile(dumpName);
            string[] files = new string[] { dumpName };
            string result = dumpName + ".7z";
            //if (Olymp.Helper.SevenZipHelper.CompressFiles(result, files) && File.Exists(result))
            //{
            //    File.Delete(dumpName);
            //}
            //else
            {
                result = dumpName + ".gz";
                if (CompressFile(dumpName, result) && File.Exists(result))
                {
                    File.Delete(dumpName);
                }
                else
                {
                    result = dumpName;
                }
            }

            return result;
        }

        //private static string CreateDatabaseDump()
        //{
        //    StringBuilder builder = new StringBuilder();
        //    builder.AppendLine("============");

        //    string dumpName = GetDatabaseDumpFileName();
        //    string result = dumpName + ".7z";
        //    string databaseDirectory = Nero.Framework.SmmBasics.DefaultSettings.BaseDirectoryForAppData;

        //    if (Olymp.Helper.SevenZipHelper.CompressDirectory(result, databaseDirectory) && File.Exists(result))
        //        builder.AppendLine("Location of the database dump file: " + result);
        //    else
        //        builder.AppendLine("Could not create database dump file.");

        //    return builder.ToString();
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static string Create2ndLogFileDump()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("============");
            string dumpName = Get2ndLogfileDumpFileName();
            if (!string.IsNullOrEmpty(Logs.GetFilePath()) && File.Exists(Logs.GetFilePath()))
            {
                File.Copy(Logs.GetFilePath(), dumpName, true);

                builder.Append("Location of 2nd logfile is: ");

                string[] files = new string[] { dumpName };
                string result = dumpName + ".7z";
                //if (Olymp.Helper.SevenZipHelper.CompressFiles(result, files) && File.Exists(result))
                //{
                //    builder.AppendLine(result);
                //    File.Delete(dumpName);
                //}
                //else
                {
                    result = dumpName + ".gz";
                    if (CompressFile(dumpName, result) && File.Exists(result))
                    {
                        builder.AppendLine(result);
                        File.Delete(dumpName);
                    }
                    else
                    {
                        builder.AppendLine(dumpName);
                    }
                }
                try
                { 
                    //File.Delete(Logs.GetFilePath()); 
                }
                catch (System.Exception)
                {
                    // May be that the logfile is locked by the logger. Ignore errors deleting it. 
                }
            }
            else
            {
                builder.AppendLine("No 2nd logfile existed.\n");
            }
            return builder.ToString();
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {                
                string dumpName = CreateCrashDump();
                StringBuilder builder = new StringBuilder();
                builder.Append(CreateHeader());
                builder.AppendLine();                

                IUnhandledExceptionHandler handler = Application.Current as IUnhandledExceptionHandler;
                if (handler != null)
                {
                    builder.AppendLine(string.Format("Additional: \r\n{0}", handler.AdditionalLog));
                    builder.AppendLine();
                }

                builder.Append(GetSenderInfor(sender));
                builder.AppendLine();

                builder.Append(CreateStackTrace(args, dumpName));
                builder.AppendLine();

                //builder.Append(CreateDatabaseDump());

                builder.Append(Create2ndLogFileDump());

                string logFile = GetReportFileName();
                builder.AppendLine(logFile);
                string strCrash = builder.ToString();
                LogHelper.UILogger.Debug("========================Application Crashed========================");
                LogHelper.UILogger.Debug(strCrash);
                LogHelper.UILogger.Debug("================================End================================");
                
                SaveLog(strCrash, logFile);
            }
            finally
            {
                var p = Process.GetCurrentProcess();
                if (p != null)
                {
                    p.Kill();
                }
            }
        }

        public static string GetSenderInfor(object sender)
        {
            StringBuilder builder = new StringBuilder("Sender:");            
            if (sender != null)
            {
                builder.Append(sender.ToString());                

                Type type = sender.GetType();
                if (type != null)
                {
                    builder.AppendFormat("Type: {0}, Namespace: {1} \r\n",
                         type.FullName,
                         type.Namespace);

                    MemberInfo[] members = type.GetMembers();
                    if (members != null)
                    {
                        builder.AppendFormat("{0} member(s) found: \r\n", members.Count());
                        foreach (MemberInfo item in members)
                        {
                            builder.AppendFormat("Name: {0}, Type: {1} \r\n",
                                item.Name,
                                item.MemberType.ToString());
                        }
                    }
                }                              
            }
            builder.AppendLine();
          
            return builder.ToString();
        }

        public static String CreateHeader()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Date time: {0}", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            builder.AppendLine();
            builder.AppendLine();

            builder.AppendFormat("Package: {0} ", packageName); //application.GetType().Namespace);
            builder.AppendLine();
            
            builder.AppendFormat("Version: {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()); // GetAppVersion());
            builder.AppendLine();
            builder.AppendLine();

            builder.AppendFormat("OS:  {0}, {1}", 
            Environment.OSVersion.ToString(),                 
            Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit");
            builder.AppendLine();

            builder.AppendLine(string.Format("Machine Name: {0} ", System.Environment.MachineName));
            //builder.AppendLine(string.Format("User Name: {0}  Domain: {1}", System.Environment.UserName, System.Environment.UserDomainName));
            builder.AppendLine(string.Format("Current Directory: {0}", System.Environment.CurrentDirectory));
            builder.AppendLine(string.Format("CPU Count: {0}", Environment.ProcessorCount));
            builder.AppendLine(string.Format("Total memory: {0}", Environment.WorkingSet));            
            builder.AppendLine();

            // process information
            Process process = Process.GetCurrentProcess();
            if (process != null)
            {
                try
                {                    
                    builder.AppendLine(string.Format("Process Name: {0}   ", process.ProcessName));                    
                    builder.AppendLine(string.Format("Used Memory: {0} Bytes  ", process.WorkingSet64));      
                    if (Thread.CurrentThread != null && Thread.CurrentThread.CurrentUICulture != null)
                    {
                        builder.AppendLine(string.Format("Language: {0}   ",  Thread.CurrentThread.CurrentUICulture.ToString()));   
                    }
                    builder.AppendLine(string.Format("Total time: {0} from {1} ", process.TotalProcessorTime.ToString(), process.StartTime.ToString()));
                    if (process.StartInfo != null )
                    {
                        builder.AppendLine(string.Format("StartInfo: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments));
                    }                   

                    if (process.Threads != null)
                    {                        
                        builder.AppendLine(string.Format("Threads count: {0}:  ",  process.Threads.Count));
                        foreach (ProcessThread thread in process.Threads)
                        {                     
                            builder.AppendLine(string.Format("Thread {0}: {1},Total time {2} from {3} ", thread.Id, thread.ToString(),
                                thread.TotalProcessorTime.ToString(), thread.StartTime.ToString()));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    builder.AppendLine();
                    builder.AppendFormat("Exception occurs: {0}   ", ex.ToString());
                }
                
                builder.AppendLine();
            }          
                        
            return builder.ToString();
        }

        private static string CreateStackTrace(UnhandledExceptionEventArgs args, string dumpName)
        {
            StringBuilder builder = new StringBuilder();
            Exception exception = (args != null ? args.ExceptionObject as Exception : null);

            if (args != null)
            {
                if (!string.IsNullOrEmpty(dumpName) && File.Exists(dumpName))
                {
                    builder.AppendFormat("Location of the dump file: {0}", dumpName);
                    builder.AppendLine();
                }
                builder.AppendFormat("Incident Identifier: {0}    ", args.GetHashCode());                
                builder.AppendFormat("Terminating: {0}", args.IsTerminating);
                builder.AppendLine();
                builder.AppendLine();
            }

            if (exception != null)
                AddExceptionInfo(builder, exception, false);

            if (args != null && exception == null)
            {
                builder.AppendFormat("\nException object: {0}\n", args.ExceptionObject);
            }

            return builder.ToString().Trim();
        }

        private static void AddSpaces(StringBuilder builder, bool isInner)
        {
            if (isInner)
                builder.Append("  ");
        }

        private static void AddExceptionInfo(StringBuilder builder, Exception exception, bool isInner)
        {
            if (exception != null)
            {
                if (isInner)
                    builder.Append("Inner Exception: ");
                builder.AppendLine(string.Format("Exception: {0}", exception.GetType().ToString()));                
                builder.AppendFormat("Message: {0}", exception.Message.ToString());
                builder.AppendLine();

                AddSpaces(builder, isInner);
                if (exception.Source != null)
                {
                    builder.AppendFormat("Source: {0}\n", exception.Source.ToString());
                }

                AddSpaces(builder, isInner);
                if (exception.StackTrace == null)
                    builder.AppendLine("Stack Trace: (null)");
                else
                    builder.AppendLine("Stack Trace:\n" + exception.StackTrace.ToString());
                Exception inner = exception.InnerException;
                if (inner != null)
                    AddExceptionInfo(builder, inner, true);
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void SaveLog(String log, string filename)
        {
            try
            {
                StreamWriter stream = File.CreateText(filename);
                stream.Write(log);
                stream.Close();

                if (crashHandlingMode == CrashHandlingCode.AskFirstAndReportImmediately ||
                    crashHandlingMode == CrashHandlingCode.ReportImmediatelyDoNotAsk)
                {
                    HandleCrashes(false);
                }
            }
            catch
            {
                Trace.TraceError("Ignore all exceptions (CrashHandler::SaveLog)");
            }
        }
      

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void HandleCrashes(bool isSettingMode)
        {
            try
            {
                string tempFolder = GetTempFolder(LogDirectoryName);
                if (Directory.Exists(tempFolder))
                {
                    string[] filenames = Directory.GetFiles(tempFolder, "BIUPC_*.log");
                    //string[] filenames = Directory.GetFiles(tempFolder);
                    Debugger.Log(0, "HockeyApp", filenames.ToString());

                    if (filenames.Length > 0)
                    {
#if DEBUG
                        StringBuilder builder = new StringBuilder();
                        string exitMessage = ResourceProvider.LoadString("IDS_APPLICATION_RUNTIME_ERROR");
                        builder.Append(exitMessage);
                        builder.AppendLine();
                        builder.Append("Log files: ");
                        builder.AppendLine();
                        foreach (string item in filenames)
                        {
                            builder.AppendLine(item);
                        }
                        builder.AppendLine();
                        builder.AppendLine("Click \"Yes\" to open, \"No\" to remove them.");
                        MessageBoxResult result = MessageBox.Show(
                             builder.ToString(),
                             "For debugging only",
                             MessageBoxButton.YesNo,
                             MessageBoxImage.Warning,
                             MessageBoxResult.Yes);

                        if (result == MessageBoxResult.Yes)
                        {
                            foreach (string item in filenames)
                            {
                                System.Diagnostics.Process.Start(item);
                            }
                        }
                        else
                        {
                            DeleteCrashes();
                        }
#else
                        bool notCanceled = true;
                        if ((!isSettingMode && crashHandlingMode == CrashHandlingCode.AskFirstAndReportImmediately) ||
                            crashHandlingMode == CrashHandlingCode.AskFirstAndReportNextTime)
                        {
                            StringBuilder builder = new StringBuilder();
                            string exitMessage = ResourceProvider.LoadString("IDS_APPLICATION_RUNTIME_ERROR");
                            builder.Append(exitMessage);
                            builder.AppendLine();
                            builder.Append(ResourceProvider.LoadString("IDS_APPLICATION_ERROR_CHECK_SEND"));
                            MessageBoxResult result = MessageBox.Show(
                                 builder.ToString(),
                                 StringConstant.ApplicationName,
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Warning,
                                 MessageBoxResult.Yes);

                            notCanceled = (result == MessageBoxResult.Yes);
                        }

                        if (notCanceled)
                        {
                            bool result = SendCrashes(tempFolder, filenames);
                            if (result)
                                Trace.TraceError("Send completed");
                            else
                                Trace.TraceError("Could not send reports");
                        }
                        else
                        {
                            DeleteCrashes();
                        }
#endif
                    }
                }
            }
            catch (Exception)
            {
                Trace.TraceError("Ignore all exceptions (CrashHandler::HandleCrashes)");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool SendCrashes(string tempFile, string[] filenames)
        {
            foreach (String filename in filenames)
            {
                try
                {
                    string log = "";
                    string fullFileName = Path.Combine(tempFile, filename);
                    using (StreamReader reader = new StreamReader(File.OpenRead(fullFileName)))
                    {
                        log = reader.ReadToEnd();
                    }
                    string body = "raw=" + Uri.EscapeDataString(log);

                    WebRequest request = WebRequest.Create(new Uri("https://rink.hockeyapp.net/api/2/apps/" + appID + "/crashes"));
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";

                    // The following lines came from the test code of HockeyApp
                    // They are commented-out, because it made the http request fail
                    //                     WebHeaderCollection hs = new WebHeaderCollection();
                    //                     hs.Add(HttpRequestHeader.UserAgent, "Hockey/Win");
                    //                     request.Headers = hs;

                    var requestResult = request.GetRequestStream();
                    {
                        try
                        {
                            Stream stream = requestResult;
                            byte[] byteArray = Encoding.UTF8.GetBytes(body);
                            stream.Write(byteArray, 0, body.Length);
                            stream.Close();

                            var responseResult = request.GetResponse();
                            {
                                Boolean deleteCrash = true;
                                try
                                {
                                    request.GetResponse();
                                }
                                catch (WebException e)
                                {
                                    if ((e.Status == WebExceptionStatus.ConnectFailure) ||
                                        (e.Status == WebExceptionStatus.ReceiveFailure) ||
                                        (e.Status == WebExceptionStatus.SendFailure) ||
                                        (e.Status == WebExceptionStatus.Timeout) ||
                                        (e.Status == WebExceptionStatus.UnknownError))
                                    {
                                        deleteCrash = false;
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                finally
                                {
                                    if (deleteCrash)
                                    {
                                        File.Delete(fullFileName);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.UILogger.Debug("SendCrashes Failed", ex);
                            return false;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        private static void CleanFolder(string folder, string mask)
        {
            string[] filenames = Directory.GetFiles(folder, mask);
            foreach (String filename in filenames)
            {
                File.Delete(Path.Combine(folder, filename));
            }
        }

        private static void DeleteCrashes()
        {
            CleanFolder(GetTempFolder(LogDirectoryName), "*.log");
            CleanFolder(GetTempFolder(DumpDirectoryName), "*.dmp");
        }
    }
}
