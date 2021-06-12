using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Utils
{
    /// <summary>
    /// Helper methods for strings and string formatting
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class StringUtils
    {
        /// <summary>
        /// Get default cmd working directory
        /// </summary>
        /// <returns>User Profile folder if not admin, System32 if admin</returns>
        public static string DefaultWorkingDirectory()
        {
            return WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) ? SystemDirectory() : ProfileDirectory();
        }

        /// <summary>
        /// Get system directory
        /// </summary>
        /// <returns>Path for %SYSTEMROOT%\System32</returns>
        public static string SystemDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.System); // System32
        }

        /// <summary>
        /// Get cmd.exe path if available, else pretend
        /// </summary>
        /// <returns>Path for console</returns>
        public static string ComSpec()
        {
            string path = Environment.GetEnvironmentVariable("COMSPEC");
            return path ?? string.Concat(SystemDirectory(), Path.DirectorySeparatorChar, "cmd.exe");
        }

        /// <summary>
        /// Get current user's profile directory
        /// </summary>
        /// <returns>Path for %USERPROFILE%</returns>
        public static string ProfileDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        /// <summary>
        /// Get command prompt header
        /// </summary>
        /// <returns>Clone of cmd.exe header</returns>
        public static string PromptHeader()
        {
            return string.Format("Microsoft Windows [Version {0}]\n(c) Microsoft Corporation. All Rights Reserved.\n", WindowsVersion());
        }

        /// <summary>
        /// Get Windows version
        /// </summary>
        /// <returns>Windows version string including UBR</returns>
        public static string WindowsVersion()
        {
            string version;
            if (Environment.OSVersion.Version.Major >= 10)
            {
                version = string.Format("{0}.{1}.{2}.", Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor, Environment.OSVersion.Version.Build);
                using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                version += registryKey.GetValue("UBR").ToString();
            }
            else
            {
                version = Environment.OSVersion.VersionString;
            }
            return version;
        }

        /// <summary>
        /// Converts a <seealso href="https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes">Win32 System Error Code</seealso> into a a <see cref="Win32Exception"/> and extracts the value of the Message variable.
        /// </summary>
        /// <param name="errorCode">The Win32 error code.</param>
        /// <returns>The error Message string if available, or an empty string.</returns>
        public static string GetErrorStringFromErrorCode(uint errorCode)
        {
            return new Win32Exception((int)errorCode).Message;
        }

        /// <summary>
        /// Converts a <seealso href="https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes">Win32 System Error Code</seealso> into a string that follows the output formatting of errors in NET.exe.
        /// </summary>
        /// <param name="errorCode">The Win32 error code.</param>
        /// <returns>The error Message string formatted in the same way as NET.exe output.</returns>
        public static string GetNetCommandStatusStringFromErrorCode(uint errorCode)
        {
            return string.Format("{0}{1}\n\n", errorCode == 0 ? "" : string.Format(strings.netCommandError + "\n\n", errorCode), GetErrorStringFromErrorCode(errorCode));
        }
    }
}
