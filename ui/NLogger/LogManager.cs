using System.Collections.Generic;

namespace BackItUp.NLogger
{
    public class LogManager
    {
        private const string dafaultLogName = "General";
        private static Dictionary<string, ILog> logTable = new Dictionary<string, ILog>();
        private static bool isInited = false;
        public static ILog DefaultLogger { get; private set; }

        static LogManager()
        {
            Setup();
        }

        public static ILog GetLogger(string name)
        {
            lock (logTable)
            {
                ILog log = null;
                if (!logTable.TryGetValue(name.ToLower(), out log))
                {
                    log = new LogImpl(name);
                    logTable.Add(name.ToLower(), log);
                }

                return log;
            }
        }

        public static bool Setup()
        {
            if (isInited)
            {
                return true;
            }

            isInited = true;
            bool ret = false;
            lock (logTable)
            {
                ret = NAdvLogWrapper.Init();
                if (ret)
                {
                    DefaultLogger = GetLogger(dafaultLogName);
                }
            }

            if (DefaultLogger != null)
            {
                DefaultLogger.Debug("==================================================");
                DefaultLogger.Debug("############### Application Started ###############");
                DefaultLogger.Debug("==================================================");
            }
            return ret;
        }

        public static void Shutdown()
        {
            lock (logTable)
            {
                foreach(ILog log in logTable.Values)
                {
                    LogImpl logImpl = log as LogImpl;
                    if (logImpl != null)
                    {
                        logImpl.Release();
                    }
                }
                logTable.Clear();
                NAdvLogWrapper.Uninit();
            }
        }
    }
}
