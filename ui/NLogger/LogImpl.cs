using System;

namespace BackItUp.NLogger
{
    class LogImpl:ILog
    {
        private string name;
        private enumLogLevel logLevel;
        private NAdvLogWrapper advLog;

        public LogImpl(string name)
        {
            this.name = name;
            advLog = new NAdvLogWrapper(name);
            this.logLevel = advLog.GetLogLevel();
        }
        
        public void Release()
        {
            advLog.Dispose();
            advLog = null;
            logLevel = enumLogLevel.Off;
        }

        private string GetExceptionFormat(object message, Exception ex)
        {
            string msg = string.Empty;
            if (ex == null)
            {
                msg = message.ToString();
            }
            else
            {
                msg = string.Format("{0}, Exception:{1}, Stack:{2}", message, ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                {
                    msg += string.Format("\nInnerException:{0}, InnerStack:{1}", ex.InnerException.Message, ex.InnerException.StackTrace);
                }
            }
            return msg;
        }

        #region ILog Members

        public bool IsDebugEnabled
        {
            get { return (this.logLevel <= enumLogLevel.Debug); }
        }

        public bool IsInfoEnabled
        {
            get { return (this.logLevel <= enumLogLevel.Info); }
        }

        public bool IsWarnEnabled
        {
            get { return (this.logLevel <= enumLogLevel.Warn); }
        }

        public bool IsErrorEnabled
        {
            get { return (this.logLevel <= enumLogLevel.Error); }
        }

        public bool IsFatalEnabled
        {
            get { return (this.logLevel <= enumLogLevel.Fatal); }
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled && message != null)
            {
                advLog.Debug(message.ToString());
            }
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled && message != null)
            {
                string msg = GetExceptionFormat(message, exception);
                advLog.Debug(msg);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                string message = string.Format(format, args);
                advLog.Debug(message);
            }
        }

        public void Info(object message)
        {
            if (IsInfoEnabled && message != null)
            {
                advLog.Info(message.ToString());
            }
        }

        public void Info(object message, Exception exception)
        {
            if (IsInfoEnabled && message != null)
            {
                string msg = GetExceptionFormat(message, exception);
                advLog.Info(msg);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                string message = string.Format(format, args);
                advLog.Info(message);
            }
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled && message != null)
            {
                advLog.Warn(message.ToString());
            }
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled && message != null)
            {
                string msg = GetExceptionFormat(message, exception);
                advLog.Warn(msg);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                string message = string.Format(format, args);
                advLog.Warn(message);
            }
        }

        public void Error(object message)
        {
            if (IsErrorEnabled && message != null)
            {
                advLog.Error(message.ToString());
            }
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled && message != null)
            {
                string msg = GetExceptionFormat(message, exception);
                advLog.Error(msg);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                string message = string.Format(format, args);
                advLog.Error(message);
            }
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled && message != null)
            {
                advLog.Fatal(message.ToString());
            }
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled && message != null)
            {
                string msg = GetExceptionFormat(message, exception);
                advLog.Fatal(msg);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                string message = string.Format(format, args);
                advLog.Fatal(message);
            }
        }

        public void Log(LogType logType, object message)
        {
            advLog.Log(logType, message.ToString());
        }

        #endregion
    }
}
