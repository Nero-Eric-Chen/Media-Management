
namespace BackItUp.NLogger
{
    public static class LogHelper
    {
        public static ILog DefaultLogger
        {
            get { return LogManager.DefaultLogger; }
        }

        private static ILog uiLogger = null;
        public static ILog UILogger
        {
            get
            {
                if (null == uiLogger)
                {
                    uiLogger = LogManager.GetLogger("UI");
                }
                return uiLogger;
            }
        }

        private static ILog indexLogger = null;
        public static ILog IndexLogger
        {
            get
            {
                if (null == indexLogger)
                {
                    indexLogger = LogManager.GetLogger("Index");
                }
                return indexLogger;
            }
        }
    }
}
