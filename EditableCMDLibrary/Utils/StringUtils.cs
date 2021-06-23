using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
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
        /// <param name="comspec">The default command interpreter's full path.</param>
        /// <param name="forceCacheRefresh">Forces a refresh of the cache.</param>
        /// <remarks>If the environment variable doesn't exist, <paramref name="comspec"/>'s value is <see cref="GetSystemDirectory"/>\<see cref="strings.commandPromptFilename"/>.</remarks>
        /// <returns>True if <paramref name="comspec"/>'s value is that of the environment variable. False if it <see cref="GetSystemDirectory"/>\<see cref="strings.commandPromptFilename"/>.</returns>
        public static bool TryGetComSpec(out string comspec, bool forceCacheRefresh = false)
        {
            // Guid used for cache entry name for bool storing existence of ComSpec environment variable.
            const string comspecValueExistsGuid = "{879198DD-AA22-4246-9DE3-4C32FCEFCF5F}";
            // Get the cached value of the ComSpec environment variable's existence. null is returned if nothing is cached.
            bool? comspecValueExistsCached = MemoryCache.Default.Get(comspecValueExistsGuid) as bool?;
            // Guid used for cache entry name for string storing ComSpec environment variable's value
            const string comspecValueGuid = "{53B13996-B543-454C-BCFC-5CB6D08B4A51}";
            // Get the cached value of the ComSpec environment variable. null is returned if nothing is cached.
            comspec = MemoryCache.Default.Get(comspecValueGuid) as string;

            // If the two cache entries exist, use their values:
            if (!forceCacheRefresh && comspecValueExistsCached != null && comspec != null)
            {
                return comspecValueExistsCached == true;
            }
            // Otherwise, refresh the cache.
            else
            {
                return updateCache(out comspec);
            }

            static bool updateCache(out string comspec)
            {
                // bool CacheItem for storing existence of environment variable
                CacheItem comspecExists = new(comspecValueExistsGuid, true);
                // string CacheItem for storing value of environment variable
                CacheItem comspecValue = new(comspecValueGuid, Environment.GetEnvironmentVariable(strings.envKeyCommandSpecifier));
                // If the environment variable doesn't exist:
                if (comspecValue.Value == null)
                {
                    // Environment variable doesn't exist
                    comspecExists.Value = false;
                    // Use a default ComSpec value of cmd.exe in the System32 folder
                    comspecValue.Value = string.Join(Path.DirectorySeparatorChar, GetSystemDirectory(), strings.commandPromptFilename);
                }
                // Cache for 3 hours
                CacheItemPolicy policy = new() { SlidingExpiration = new TimeSpan(hours: 3, minutes: 0, seconds: 0) };

                // Add the CacheItems to the cache, overwriting if they already exist
                MemoryCache.Default.Set(comspecValue, policy);
                MemoryCache.Default.Set(comspecExists, policy);

                // Return the values now in the cache
                comspec = comspecValue.Value as string;
                return comspecExists.Value as bool? == true;
            }
        }

        /// <summary>
        /// When system user preferences change, refresh cached values
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // Preference changes in category General
            if (e.Category == UserPreferenceCategory.General)
            {
                // Environment variables may have changed, force a refresh
                _ = TryGetComSpec(out _, true);
            }
        }

        /// <summary>
        /// Get the value of environment variable %ComSpec%.
        /// </summary>
        /// <returns>The default command interpreter's full path, or <see cref="GetSystemDirectory"/>\<see cref="strings.commandPromptFilename"/>.</returns>
        public static string GetComSpec()
        {
            _ = TryGetComSpec(out string comspec);
            return comspec;
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
            // Guid used for cache entry name for bool storing existence of UBR registry key.
            const string ubrValueExistsGuid = "{A322D33B-4CBA-48B0-9CFB-21827C73880A}";
            // Get the cached value of the UBR registry key's existence. null is returned if nothing is cached.
            bool? ubrValueExistsCached = MemoryCache.Default.Get(ubrValueExistsGuid) as bool?;
            // Guid used for cache entry name for string storing UBR registry key's value
            const string ubrValueGuid = "{841EF3D4-9D49-4AE8-8870-E22291EA16A8}";
            // Get the cached value of the UBR registry key. null is returned if nothing is cached.
            ubr = MemoryCache.Default.Get(ubrValueGuid) as string;

            // If the two cache entries exist, use their values:
            if (ubrValueExistsCached != null && ubr != null)
            {
                return ubrValueExistsCached == true;
            }
            // Otherwise, refresh the cache.
            else
            {
                return updateCache(out ubr);
            }

            static bool updateCache(out string ubr)
            {
                // bool CacheItem for storing existence of registry key
                CacheItem ubrExists = new(ubrValueExistsGuid, true);
                // string CacheItem for storing value of registry key
                CacheItem ubrValue = new(ubrValueGuid, GetRegistryValue(strings.win10RegKeyUBR, strings.win10RegNameUBR));
                // If the registry key doesn't exist:
                if (ubrValue.Value == null)
                {
                    // Registry key doesn't exist
                    ubrExists.Value = false;
                    // Use a default UBR of 0
                    ubrValue.Value = strings.win10VersionDefaultUBR;
                }
                // Updating Windows 10 to a new build requires a reboot - UBR won't change for life of a console session.
                CacheItemPolicy policy = new() { Priority = CacheItemPriority.NotRemovable };

                // Add the CacheItems to the cache, overwriting if they already exist
                MemoryCache.Default.Set(ubrValue, policy);
                MemoryCache.Default.Set(ubrExists, policy);

                // Return the values now in the cache
                ubr = ubrValue.Value as string;
                return ubrExists.Value as bool? == true;
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
        /// <param name="prependHeader">True prepends the header with the application's name, version, and copyright.</param>
        /// <returns>The Command Prompt header, prepended with the application's information if <paramref name="prependHeader"/> is <c>true</c>.</returns>
        public static string GetPromptHeader(bool prependHeader)
        {
            string operatingSystemVersionCopyright = GetVersionCopyrightHeader(strings.commandPromptWindowsOSName, GetWindowsVersion(), strings.commandPromptWindowsOSCopyright);
            return prependHeader
                ? string.Concat(
                    GetVersionCopyrightHeader((Assembly.GetEntryAssembly().GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute).Product, Assembly.GetEntryAssembly().GetName().Version.ToString(), (Assembly.GetEntryAssembly().GetCustomAttribute(typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute).Copyright.Replace("\u00a9", "(c)")),
                    operatingSystemVersionCopyright
                    )
                : operatingSystemVersionCopyright;
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
