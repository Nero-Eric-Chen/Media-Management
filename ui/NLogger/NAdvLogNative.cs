using System;
using System.Runtime.InteropServices;

namespace BackItUp.NLogger
{
    public enum enumLogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        Off
    }

    internal static class NAdvLogNative
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("Kernel32", CharSet = CharSet.Auto)]
        public static extern IntPtr LoadLibrary(string moduleName);

        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern int NAdvLog_Init();

        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern void NAdvLog_Uninit();

         [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr NAdvLog_GetLoggerEx(string tagModule);

         [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern void NAdvLog_Debug(IntPtr hLogger, string message);

        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
         internal static extern void NAdvLog_Info(IntPtr hLogger, string message);

        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern void NAdvLog_Warn(IntPtr hLogger, string message);

        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern void NAdvLog_Error(IntPtr hLogger, string message);

        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern void NAdvLog_Fatal(IntPtr hLogger, string message);

        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern void NAdvLog_Log(IntPtr hLogger, LogType logType, string message);
        
        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern enumLogLevel NAdvLog_GetLogLevel(IntPtr hLogger);

        [DllImport("NAdvLog.dll", CharSet = CharSet.Unicode)]
        internal static extern void NAdvLog_ReleaseLogger(IntPtr hLogger);
    }
}
