
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/******************************************************************************************************************************************

	Copyright Nero AG. (Ltd.) 2009-2010


	Changes:
	--------
	__date__________name________remarks_________________________________________________
	01.SEP.2008                 Created
******************************************************************************************************************************************/
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text;

namespace Utilities.WindowsAPI
{ 
    /// <summary>
    /// IAY: Used to import kernel32.dll APIs.
    /// </summary>
    public  static class Kernel32API
    {
        #region Constants

        public const int WM_QUERYENDSESSION = 0x0011;
        public const int ENDSESSION_CLOSEAPP = 0x0001;
        public const int WM_ENDSESSION = 0x0016;

        public const int WM_DEVICECHANGE = 0x0219;
        public const int DBT_DEVICEARRIVAL = 0x8000;        // System detected a new device
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;    // Preparing to remove (any program can disable the removal)
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // Removed 
        public const int DBT_DEVICEARIVALNONELETTER = 0x0007;
        #endregion

        #region Public Methods

        [DllImport("kernel32.dll")]
        public static extern uint RegisterApplicationRestart([MarshalAs(UnmanagedType.LPWStr)] string pszCommandline, int dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetVolumeInformation(string volume, StringBuilder volumeName,
            uint volumeNameSize, out uint SerialNumber, out uint serialNumberLength,
            out uint flags, StringBuilder fs, uint fs_size);
        
        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);
        #endregion
    }
}
