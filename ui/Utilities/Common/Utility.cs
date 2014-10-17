using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using BackItUp.NLogger;
using Microsoft.Win32;
using Utilities.WindowsAPI;

namespace Utilities.Common
{
    public class Utility
    {
        /// <summary>
        /// GM: Used to get the complete path from a mapped network path
        /// e.g for Z:\ this may return as \\192.168.0.2\Home.
        /// </summary>
        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int WNetGetConnection(
            string localName,
            StringBuilder remoteName,
            ref int length);

        public static void OnStartProcess(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (System.Exception ex)
            {
                LogHelper.UILogger.Debug("OnStartProcess Exception:", ex);
            }
        }

        /// <summary>
        /// GM: Validate email address.
        /// </summary>
        public static bool IsValidEmailAddress(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                return false;
            }

            // string emailAddressPattern = "^[a-zA-Z0-9][\\w\\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\\w\\.-]*[a-zA-Z0-9]\\.[a-zA-Z][a-zA-Z\\.]*[a-zA-Z]$";//khe 2009-10-7 Task#84762
            string emailAddressPattern = "^[a-zA-Z0-9_+.-]+\\@([a-zA-Z0-9-]+\\.)+[a-zA-Z0-9]{2,4}$";
            return Regex.IsMatch(emailAddress, emailAddressPattern);
        }

        public static bool IsAvailableDateTime(DateTime dateTime)
        {
            if (dateTime == null || dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue
               || dateTime.Year < 1900 || dateTime.Year > 2999)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// GM: Check invalid characters.
        /// </summary>
        /// <param name="name">input string to check for validity</param>
        /// <returns>true if input string contains invalid charcters else false</returns>
        public static bool ContainsInvalidCharacters(string name)
        {
            return name.IndexOfAny(StringConstant.NameInvalidCharacters.ToCharArray()) > -1;
        }

        public static void BringWindowToTop(Window window)
        {
            try
            {
                HwndSource source = (HwndSource)PresentationSource.FromVisual(window);
                if (null == source)
                    return;
                IntPtr handle = source.Handle;
                if (null == handle)
                    return;
                WinAPI.SetForegroundWindow(handle);
                WinAPI.SetFocus(handle);
            }
            catch { }
        }

        public static string GetSingularOrPluralString(long iCount, string SingularResourceID, string PluralResourceID)
        {
            if (1 == iCount || 0 == iCount)
                return ResourceProvider.LoadString(SingularResourceID);
            else
                return ResourceProvider.LoadString(PluralResourceID);
        }

        public static string GetProductVersionFromRegistry()
        {
            const string REGISTRY_NERO_BACKITUP = "SOFTWARE\\Nero\\Nero BackItUp";
            string version = string.Empty;
            try
            {
                RegistryKey regLocalMachine = Registry.LocalMachine;
                if (regLocalMachine != null)
                {
                    RegistryKey regBIU = regLocalMachine.OpenSubKey(REGISTRY_NERO_BACKITUP);
                    if (regBIU != null)
                    {
                        version = regBIU.GetValue("Version").ToString();
                        regBIU.Close();
                    }

                    regLocalMachine.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.UILogger.Debug("GetProductVersionFromRegistry", ex);
            }
            return version;
        }

        public static string GetProductVersion()
        {
            string version = string.Empty;
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                version =  fvi.FileVersion;
            }
            catch (Exception ex)
            {
                LogHelper.UILogger.Debug("GetProductVersion:", ex);
                version = GetProductVersionFromRegistry();
            }
            return version;
        }

        public static string GetProductCopyright()
        {
            string copyRight = string.Empty;
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                if (!string.IsNullOrEmpty(fvi.LegalCopyright))
                {
                    copyRight = fvi.LegalCopyright;
                }
            }
            catch (Exception ex)
            {
                LogHelper.UILogger.Debug("GetProductCopyright:", ex);
                copyRight = StringConstant.Copyright;
            }
            return copyRight;
        }

        public static bool IsWindowsVistaLater()
        {
            // Newer than Windows Vista
            return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 6);
        }

        /// <summary>
        /// Gets the week index of month, 0 being first week and 4 being last week
        /// </summary>
        /// <param name="time"></param>
        /// <returns>0 being first week and 4 being last week</returns>
        public static int GetWeekOfMonth(DateTime time)
        {
            DateTime firstDay = new DateTime(time.Year, time.Month, 1);
            int wFirstDayWeek = (int)firstDay.DayOfWeek;
            return ((time.Day + wFirstDayWeek - 1) / 7);
        }

        /// <summary>
        /// Is optical disc i.e CD, DVD or removeable disc ... etc.
        /// </summary>
        public static bool IsOpticalDisc(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                if (path == StringConstant.ImageRecorder)
                    return true;

                System.IO.DriveInfo di = new System.IO.DriveInfo(path.Substring(0, 1));
                return (di.DriveType == System.IO.DriveType.CDRom);
            }
            catch (Exception exp)
            {
                Debug.Assert(false, exp.Message);
            }

            return false;
        }

        /// <summary>
        /// Is system path.
        /// </summary>
        public static bool IsSystemPath(string path)
        {
            return Environment.SystemDirectory.Contains(path);
        }

        /// <summary>
        /// Is network path.
        /// </summary>
        public static bool IsNetworkPath(string path)
        {
            return (path != null && path.Length >= 3 && (path.StartsWith(StringConstant.NetworkPath) || path.StartsWith(StringConstant.NetworkPathSeparator)));
        }

        /// <summary>
        /// Detect the computer is connected to internet or not.
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectedToInternet()
        {
            int Desc = 0;
            return Wininet.InternetGetConnectedState(out   Desc, 0);
        }

        /// <summary>
        ///  To check the string is GUID string or not
        /// </summary>        
        public static bool IsGuid(string guidString)
        {
            bool isGuid = false;
            try
            {
                Guid guid = Guid.Parse(guidString);
                if (guid != null)
                {
                    isGuid = true;
                }
            }
            catch (FormatException)
            {
            }
            catch (ArgumentNullException)
            {

            }
            catch (Exception)
            {
            }
            return isGuid;
        }

        /// <summary>
        /// Detect that BIU support GPT disk or not. 
        /// </summary>
        /// <returns></returns>
        public static bool IsGPTSupported()
        {
            // Newer than Windows Vista
            return IsWindowsVistaLater();
        }

        public static string GetUNCPath(string originalPath)
        {
            // As the max path can be.
            StringBuilder uncPath = new StringBuilder(512);

            // Set the size i.e.512. 
            int size = uncPath.Capacity;

            // Validate the path letter.
            if (originalPath.Length > 2 && originalPath[1] == ':')
            {
                // Validate the drive letter.
                // Valid drive letters range is [a-z && A-Z].
                char letter = originalPath[0];
                if ((letter >= 'a' && letter <= 'z') || (letter >= 'A' && letter <= 'Z'))
                {
                    // Get the unc path from mapped network drive letter.
                    int error = WNetGetConnection(originalPath.Substring(0, 2), uncPath, ref size);
                    // If successful UNC path return.
                    if (error == 0)
                    {
                        // Get the directory info from the drive letter.
                        DirectoryInfo dir = new DirectoryInfo(originalPath);

                        // Get the remaining path from excluding the root path.
                        string path = System.IO.Path.GetFullPath(originalPath).Substring(System.IO.Path.GetPathRoot(originalPath).Length);

                        // Make the complete UNC path.
                        originalPath = System.IO.Path.Combine(uncPath.ToString().TrimEnd(), path);
                    }
                }
            }
            return originalPath;
        }

        /// <summary>
        /// for target selection, 
        /// if target is drive root, add "Nero BackItUp"
        /// when valid or browse, need to remove it.
        /// </summary>
        /// <param name="targetPath"></param>
        public static string TrimTargetPath(string targetPath)
        {
            targetPath = targetPath.Trim();
            if (!Directory.Exists(targetPath) && targetPath.EndsWith(StringConstant.AutoAppendedFolderName))
                targetPath = Directory.GetParent(targetPath).FullName;
            return targetPath;
        }

        public static DateTime ConvertJsonToDatetime(string jsonDateTime)
        {
            if (jsonDateTime == null) 
                return DateTime.MinValue;

            DateTime dt = DateTime.MinValue;
            DateTime.TryParse(jsonDateTime, out dt);
            return dt;
        }

    }
}
