using System;
using System.Collections.Generic;
using Microsoft.Win32;
using WinReg = Microsoft.Win32.Registry;

namespace Terrascape.WinLocalProvider.Registry
{
    public class RegUtil
    {
        public static readonly string[] EmptyStrings = new string[0];

        public static readonly string LocalMachineRootName = WinReg.LocalMachine.Name;
        public static readonly string CurrentUserRootName = WinReg.CurrentUser.Name;

        public const string LocalMachineRootAlias = "HKLM";
        public const string CurrentUserRootAlias = "HKCU";
        public static readonly IEnumerable<string> AllRootAliases = new[] {
            LocalMachineRootAlias,
            CurrentUserRootAlias,
        };

        public static string ToString(RegistryValueKind kind)
        {
            switch (kind)
            {
                case RegistryValueKind.String:
                    return "string";
                case RegistryValueKind.Binary:
                    return "binary";
                case RegistryValueKind.DWord:
                    return "dword";
                case RegistryValueKind.QWord:
                    return "qword";
                case RegistryValueKind.MultiString:
                    return "multi";
                case RegistryValueKind.ExpandString:
                    return "expand";
                default:
                    return "";
            }
        }

        public static RegistryValueKind ParseValueKind(string kind)
        {
            switch (kind?.ToLower())
            {
                case "string":
                    return RegistryValueKind.String;
                case "binary":
                    return RegistryValueKind.Binary;
                case "dword":
                    return RegistryValueKind.DWord;
                case "qword":
                    return RegistryValueKind.QWord;
                case "multi":
                    return RegistryValueKind.MultiString;
                case "expand":
                    return RegistryValueKind.ExpandString;
                default:
                    return RegistryValueKind.Unknown;
            }
        }

        public static object ResolveValue(ArgumentRegValue arg)
        {
            switch (ParseValueKind(arg.Type))
            {
                case RegistryValueKind.Binary:
                    return Convert.FromBase64String(arg.ValueBase64);
                case RegistryValueKind.DWord:
                    return int.Parse(arg.Value);
                case RegistryValueKind.QWord:
                    return long.Parse(arg.Value);
                case RegistryValueKind.MultiString:
                    return arg.Values;
                case RegistryValueKind.ExpandString:
                    return string.IsNullOrEmpty(arg.Value)
                        ? arg.Values ?? EmptyStrings
                        : (object)arg.Value;
                default:
                    return arg.Value;
            }
        }

        public static RegistryKey ParseRootKey(string name)
        {
            if (name == LocalMachineRootName
                || name == LocalMachineRootAlias)
                return WinReg.LocalMachine;
            if (name == CurrentUserRootName
                || name == CurrentUserRootAlias)
                return WinReg.CurrentUser;

            return null;
        }

        /// <summary>
        /// Returns a 3-tuple containing the root reg key, the parent reg key,
        /// and the single-segment name of the argument key.  If the parent,
        /// of the argument key is a root key, then the parent reg key component
        /// of the tuple will be null.
        /// </summary>
        public static (RegistryKey root, RegistryKey parent, string name)? OpenParentWithName(RegistryKey key, bool writable = false)
        {
            return OpenParentWithName(key.Name);
        }

        /// <summary>
        /// Returns a 3-tuple containing the root reg key, the parent reg key,
        /// and the single-segment name of the argument key.  If the parent,
        /// of the argument key is a root key, then the parent reg key component
        /// of the tuple will be null.
        /// </summary>
        public static (RegistryKey root, RegistryKey parent, string name)? OpenParentWithName(string keyFullName, bool writable = false)
        {
            var lastSep = keyFullName.LastIndexOf('\\');
            if (lastSep < 0)
                return null;
            
            var parentFullName = keyFullName.Substring(0, lastSep);
            var keyName = keyFullName.Substring(lastSep + 1);

            var split = parentFullName.Split('\\', 2);
            if (split.Length < 1)
                return null;

            var root = ParseRootKey(split[0]);
            if (root == null)
                return null;
            
            if (split.Length == 1)
                return (root, null, keyName);
            else
                return (root, root.OpenSubKey(split[1], writable), keyName);
        }
    }
}