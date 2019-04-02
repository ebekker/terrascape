using System.Collections.Generic;
using Microsoft.Win32;
using WinReg = Microsoft.Win32.Registry;

namespace Terrascape.WinLocalProvider.Registry
{
    public class RegUtil
    {
        public static readonly string[] EmptyStrings = new string[0];

        public const string LocalMachineRoot = "HKLM";
        public const string CurrentUserRoot = "HKCU";
        public static readonly IEnumerable<string> AllRoots = new[] {
            LocalMachineRoot,
            CurrentUserRoot,
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

        public static RegistryKey ParseRootKey(string name)
        {
            switch (name)
            {
                case LocalMachineRoot:
                    return WinReg.LocalMachine;
                case CurrentUserRoot:
                    return WinReg.CurrentUser;
            }

            return null;
        }
    }
}