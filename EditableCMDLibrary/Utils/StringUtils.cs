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
        #region Special Folders

        /// <summary>
        /// Get the path for <see cref="Environment.SpecialFolder.System"/> as a string.
        /// </summary>
        /// <remarks>On non-Windows, value is <see cref="string.Empty"/>.</remarks>
        /// <returns>System directory - path for %SystemRoot%\System32.</returns>
        public static string GetSystemDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.System);
        }

        /// <summary>
        /// Get the path for <see cref="Environment.SpecialFolder.UserProfile"/> as a string.
        /// </summary>
        /// <remarks>If the environment variable doesn't exist, value is <see cref="string.Empty"/>.</remarks>
        /// <returns>User's profile directory - %UserProfile%, $HOME, $XDG_HOME.</returns>
        public static string GetUserProfileDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        #endregion

        #region Environment Variables

        /// <summary>
        /// Get the value of environment variable %ComSpec%.
        /// </summary>
        /// <returns>Path for console.</returns>
        public static string GetComSpec()
        {
            string path = Environment.GetEnvironmentVariable("COMSPEC");
            return path ?? string.Concat(GetSystemDirectory(), Path.DirectorySeparatorChar, "cmd.exe");
        }



        #endregion

        #region Registry Values

        /// <summary>
        /// Get a string value from the registry.
        /// </summary>
        /// <param name="subKey">The SubKey containing the value.</param>
        /// <param name="name">The name of the registry value.</param>
        /// <returns>The registry value, or null if it doesn't exist.</returns>
        public static string GetRegistryValue(string subKey, string name)
        {
            using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(subKey);
            return registryKey?.GetValue(name)?.ToString();
        }

        /// <summary>
        /// Get the Windows Update Build Revision (UBR).
        /// </summary>
        /// <returns>True if <paramref name="ubr"/> contains the value of the registry key. False if <paramref name="ubr"/> contains the value of <see cref="strings.win10VersionDefaultUBR"/>.</returns>
        public static bool TryGetWindowsVersionUBR(out string ubr)
        {
            ubr = GetRegistryValue(strings.win10RegKeyUBR, strings.win10RegNameUBR);
            if (ubr != null)
            {
                return true;
            }
            else
            {
                ubr = strings.win10VersionDefaultUBR;
                return false;
            }
        }

        #endregion

        #region Paths

        /// <summary>
        /// Get Command Prompt default working directory.
        /// </summary>
        /// <returns><see cref="GetSystemDirectory"/> if admin, otherwise <see cref="GetUserProfileDirectory"/>.</returns>
        public static string GetDefaultWorkingDirectory()
        {
            bool hasAdminRights = false;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                hasAdminRights = WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
            return hasAdminRights
                ? GetSystemDirectory()
                : GetUserProfileDirectory();
        }


        #endregion

        #region Versions and Copyrights

        /// <summary>
        /// Get the Command Prompt version.
        /// </summary>
        /// <returns>The Command Prompt version (same format as VER command).</returns>
        public static string GetCommandPromptVersion()
        {
            return GetVersionHeader(strings.commandPromptWindowsOSName, GetWindowsVersion());
        }

        /// <summary>
        /// Get a Command Prompt style version header.
        /// </summary>
        /// <param name="name">Name of the operating system or application (e.g. Microsoft Windows).</param>
        /// <param name="version">Version of the operating system or application (e.g. 10.0.0.0).</param>
        /// <returns>A version string formatted using Command Prompt VER format.</returns>
        public static string GetVersionHeader(string name, string version)
        {
            return string.Format(strings.commandPromptVerFormat, name, version);
        }

        /// <summary>
        /// Version and copyright string formatted using Command Prompt header format.
        /// </summary>
        /// <param name="name">Name of the operating system or application (e.g. <see cref="strings.commandPromptWindowsOSName"/>).</param>
        /// <param name="version">Version of the operating system or application (e.g. <see cref="GetWindowsVersion"/>).</param>
        /// <param name="copyright">Copyright notice of the operating system or application (e.g. <see cref="strings.commandPromptWindowsOSCopyright"/>).</param>
        /// <returns></returns>
        public static string GetVersionCopyrightHeader(string name, string version, string copyright)
        {
            return string.Format(strings.commandPromptHeaderFormat, GetVersionHeader(name, version), copyright);
        }

        /// <summary>
        /// Get the header for the prompt.
        /// </summary>
        /// <returns>The Command Prompt header.</returns>
        public static string GetPromptHeader()
        {
            return GetVersionCopyrightHeader(strings.commandPromptWindowsOSName, GetWindowsVersion(), strings.commandPromptWindowsOSCopyright);
        }

        /// <summary>
        /// Get the Windows version as a string.
        /// </summary>
        /// <returns>Windows version (including Windows 10 UBR).</returns>
        public static string GetWindowsVersion()
        {
            bool hasUBR = false;
            string updateBuildVersion = null;
            // Windows 10 (and later?) uses a different version string format.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 10)
            {
                hasUBR = TryGetWindowsVersionUBR(out updateBuildVersion);
            }
            return !hasUBR
                ? Environment.OSVersion.Version.ToString()
                : string.Format(strings.win10VersionFormat, Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor, Environment.OSVersion.Version.Build, updateBuildVersion);
        }

        #endregion

        #region Error Messages

        /// <summary>
        /// Converts a <seealso href="https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes">Win32 System Error Code</seealso> into a a <see cref="Win32Exception"/> and extracts the value of the Message variable.
        /// </summary>
        /// <param name="errorCode">The Win32 error code.</param>
        /// <returns>The error Message string if available, or an empty string.</returns>
        public static string GetErrorMessageFromErrorCode(uint errorCode)
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
            string netCommandStatus = string.Format(strings.netCommandStatusFormat, GetErrorMessageFromErrorCode(errorCode));
            return errorCode == 0
                ? netCommandStatus
                : string.Concat(string.Format(strings.netCommandErrorFormat, errorCode), netCommandStatus);
        }

        #endregion
    }
}
