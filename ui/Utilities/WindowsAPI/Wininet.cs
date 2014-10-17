
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
using System.Text;
using System.Runtime.InteropServices;

namespace Utilities.WindowsAPI
{
    /// <summary>
    /// Class for Wininet API
    /// </summary>
    public static class Wininet
    {
        [DllImport("wininet.dll")]
        public extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
    }
     
}
