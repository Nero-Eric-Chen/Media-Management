using MediaManagement.Infrastructure;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using Utilities.Common;

namespace MediaManagement
{
    public class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            string strArgs = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                strArgs += args[i];
            }
            BackItUp.NLogger.LogHelper.UILogger.DebugFormat(string.Format("Receive passed args: {0}", strArgs));
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }

    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        App app;
        public SingleInstanceManager()
        {
            this.IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            // First time app is launched
            app = new App();
            app.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            app.Activate();
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        public static bool IsFirstLuanch
        {
            get;
            private set;
        }

        bool IsInstalledFrameworkSupported()
        {
            bool isSupported = false;

            Version supportedFramework = new Version(StringConstant.SupportedFrameworkVersion);

            RegistryKey versionKey;
            if (supportedFramework.Major == 4)
            {
                versionKey = Registry.LocalMachine.OpenSubKey(StringConstant.FrameworkVersionRegistryKeyV4);
            }
            else if (supportedFramework.Major < 4)
            {
                versionKey = Registry.LocalMachine.OpenSubKey(StringConstant.FrameworkVersionRegistryKey);
            }
            else
            {
                return false;
            }

            string[] installedVersions = versionKey.GetSubKeyNames();
            foreach (string installedVersion in installedVersions)
            {
                using (RegistryKey key = versionKey.OpenSubKey(installedVersion))
                {
                    string version = Convert.ToString(key.GetValue("Version"));
                    if (string.IsNullOrEmpty(version)) continue;

                    Version installedFramework = new Version(version);
                    if (installedFramework >= supportedFramework)
                    {
                        isSupported = true;
                        break;
                    }
                }
            }
            return isSupported;
        }

         /// <summary>
         /// Verify the guest account
         /// </summary>
        public bool IsGuestUser()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Guest);
        }

         /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">
        /// More than one instance of the <see cref="T:System.Windows.Application"/> class is created per <see cref="T:System.AppDomain"/>.
        /// </exception>
        public App()
            : base()
        {
            CrashHandler.Instance.SetMode(CrashHandlingCode.AskFirstAndReportImmediately, "com.nero.backitup", "f57c1a55f7d514ec2dacaf91c1c77ab1");
        }

        public void Activate()
        {
            try
            {
                //Reactivate application's main window
                this.MainWindow.Activate();
                this.MainWindow.Show();
                Utility.BringWindowToTop(this.MainWindow);
            }
            catch (Exception e)
            {
                string strFormat = "Activate exception: {0}, StackTrace: {1}, MainWindow is {2}";
                string strMessage = string.Format(strFormat, e.Message, e.StackTrace, (null == MainWindow) ? "NULL" : "Valid");
                Trace.WriteLine(strMessage);
            }
        }

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = LanguageSetting.GetAppCulture();
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
            ResourceDictionary lanRes = LanguageSetting.LoadLanguageResource();
            Application.Current.Resources.MergedDictionaries.Add(lanRes);
             Load UX library.
             BIUPC-85, Don't use anything of UX library please, we kept the reference because Nfx.Framework.Burn.UI used it still.
             Have to load the UXLibrary's resource because we still used its resource in XAML files, e.g. style, brush.
            if (Nero.Framework.UXLibrary.Skins.Skinning.CurrentResources != null)
            {
                this.Resources.MergedDictionaries.Add(Nero.Framework.UXLibrary.Skins.Skinning.CurrentResources);
            }
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/BackItUp;Component/Resources/ConverterDictionary.xaml", UriKind.Relative), });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/BackItUp.Theme;Component/Default/Theme.xaml", UriKind.Relative), });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/themes/generic.xaml", UriKind.Relative), });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Resources/NotifyIconResources.xaml", UriKind.Relative), });

            base.OnStartup(e);

            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            NLogger.LogHelper.UILogger.Debug("Application OnStartup");
            SplashHelper.Show();

            if (UserTrailSetting.GetIsUserTrailEnable())
            {
                AnalyticsHelper.StartAnalyticsTrack(StringConstant.TrackingID, "my.nerobackitup.com", StringConstant.ClientID);
            }
            AnalyticsHelper.TrackEvent(AnalyticsHelper.Categories.Application,
                   IsFirstLuanch ? AnalyticsHelper.Events.StartUp1st : AnalyticsHelper.Events.StartUp);

            if (!IsInstalledFrameworkSupported())
            {
                SplashHelper.Close();

                string title = ResourceProvider.LoadString("IDS_NERO_BACKITUP");
                string desc = string.Format(ResourceProvider.LoadString("IDS_FRAMEWORK_NOT_SUPPORTED"), StringConstant.FrameworkDownloadLink);

                CustomMessageBox msgbox = new CustomMessageBox();
                msgbox.Title = title;
                msgbox.Message = desc;
                msgbox.MessageBoxImage = MessageBoxImage.Warning;
                msgbox.MessageBoxButton = MessageBoxButton.OK;
                msgbox.Topmost = true;
                msgbox.ShowDialog();
                return;
            }

             MI: Guest user have no access to BIU
            if (IsGuestUser())
            {
                SplashHelper.Close();

                string strGuestMsg = ResourceProvider.LoadString("IDS_MSG_FOR_GUEST");
                string strCaption = ResourceProvider.LoadString("IDS_NERO_BACKITUP");
                MessageBox.Show(strGuestMsg, strCaption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            DataManager.GlobalInit();

            BackItUpBootstrapper bootstrapper = new BackItUpBootstrapper();
            bootstrapper.Run();

            SplashHelper.Close();

            this.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
        }

        private static void AppDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception ex)
        {
            if (ex == null)
                return;

            ExceptionPolicy.HandleException(ex, "Default Policy");
            MessageBox.Show(BackItUp.Properties.Resources.UnhandledException);
            Environment.Exit(1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NLogger.LogHelper.UILogger.Error("===App OnExit Enter...");
            try
            {
                AnalyticsHelper.TrackEvent(AnalyticsHelper.Categories.Application, AnalyticsHelper.Events.Exit);

                Dispose(true);

                AnalyticsHelper.StopAnalyticsTrack();

                _notifyIcon.Dispose();
            }
            catch (System.Exception ex)
            {
                NLogger.LogHelper.UILogger.Error("App OnExit Error.", ex);
            }
            NLogger.LogHelper.UILogger.Error("===App OnExit Leave...");
            base.OnExit(e);
        }

        #region IDisposable Members

        private bool _alreadyDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_alreadyDisposed)
                return;
            if (isDisposing)
            {
                //TODO: release managed resources
            }
            //TODO: release unmanaged resources
            _alreadyDisposed = true;
        }

        ~App()
        {
            Dispose(false);
        }
        #endregion
    }
}
