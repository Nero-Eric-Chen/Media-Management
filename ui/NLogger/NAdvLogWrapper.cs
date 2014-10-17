using System;

namespace BackItUp.NLogger
{
    class NAdvLogWrapper :IDisposable
    {
        private IntPtr loggerHandle = IntPtr.Zero;
        
        public NAdvLogWrapper(string name)
        {
            loggerHandle = NAdvLogWrapper.GetLogger(name);
        }

        public static bool Init()
        {
            int ret = NAdvLogNative.NAdvLog_Init();
            if (ret == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void Uninit()
        {
            NAdvLogNative.NAdvLog_Uninit();
        }

        static IntPtr GetLogger(string name)
        {
            return NAdvLogNative.NAdvLog_GetLoggerEx(name);
        }

        static void ReleaseLogger(IntPtr loggerPtr)
        {
            NAdvLogNative.NAdvLog_ReleaseLogger(loggerPtr);
        }

        public void Debug(string message)
        {
            if (this.loggerHandle == IntPtr.Zero)
            {
                return;
            }
            NAdvLogNative.NAdvLog_Debug(this.loggerHandle, message);
        }

        public void Info(string message)
        {
            if (this.loggerHandle == IntPtr.Zero)
            {
                return;
            }
            NAdvLogNative.NAdvLog_Info(this.loggerHandle, message);
        }

        public void Warn(string message)
        {
            if (this.loggerHandle == IntPtr.Zero)
            {
                return;
            }
            NAdvLogNative.NAdvLog_Warn(this.loggerHandle, message);
        }

        public void Error(string message)
        {
            if (this.loggerHandle == IntPtr.Zero)
            {
                return;
            }
            NAdvLogNative.NAdvLog_Error(this.loggerHandle, message);
        }

        public void Fatal(string message)
        {
            if (this.loggerHandle == IntPtr.Zero)
            {
                return;
            }
            NAdvLogNative.NAdvLog_Fatal(this.loggerHandle, message);
        }

        public void Log(LogType logType, string message)
        {
            if (this.loggerHandle == IntPtr.Zero)
            {
                return;
            }

            NAdvLogNative.NAdvLog_Log(this.loggerHandle, logType, message);
        }

        public enumLogLevel GetLogLevel()
        {
            if (this.loggerHandle == IntPtr.Zero)
            {
                return enumLogLevel.Off;
            }

            return NAdvLogNative.NAdvLog_GetLogLevel(this.loggerHandle);
        }



        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.loggerHandle != IntPtr.Zero)
                {
                    NAdvLogWrapper.ReleaseLogger(this.loggerHandle);
                }
            }
        }
        #endregion
    }
}
