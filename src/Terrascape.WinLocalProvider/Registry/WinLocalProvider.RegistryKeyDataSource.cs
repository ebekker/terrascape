using System;
using System.Linq;
using HC.TFPlugin;
using HC.TFPlugin.Diagnostics;
using Microsoft.Win32;
using WinReg = Microsoft.Win32.Registry;
using Terrascape.WinLocalProvider.Registry;
using System.Collections.Generic;

namespace Terrascape.WinLocalProvider
{
    public partial class WinLocalProvider : IDataSourceProvider<RegistryKeyDataSource>
    {
        public ValidateDataSourceConfigResult<RegistryKeyDataSource> ValidateConfig(
            ValidateDataSourceConfigInput<RegistryKeyDataSource> input)
        {
            var result = new ValidateDataSourceConfigResult<RegistryKeyDataSource>();

            if (!RegUtil.AllRoots.Contains(input.Config.Root))
                result.Error("invalid root, must be one of:  "
                    + string.Join(" | ", RegUtil.AllRoots),
                    steps: new TFSteps().Attribute("root"));

            return result;
        }

        public ReadDataSourceResult<RegistryKeyDataSource> Read(
            ReadDataSourceInput<RegistryKeyDataSource> input)
        {
            var result = new ReadDataSourceResult<RegistryKeyDataSource>();

            var root = RegUtil.ParseRootKey(input.Config.Root);
            if (root == null)
            {
                    result.Error("invalid root, must be one of:  "
                        + string.Join(" | ", RegUtil.AllRoots));
            }
            else
            {
                using (var regKey = root.OpenSubKey(input.Config.Path))
                {
                    result.State = new RegistryKeyDataSource
                    {
                        Root = input.Config.Root,
                        Path = input.Config.Path,
                        KeyNames = regKey.GetSubKeyNames(),
                        ValueNames = regKey.GetValueNames(),
                        Entries = new Dictionary<string, Registry.ComputedRegValue>(),
                    };
                    foreach (var n in result.State.ValueNames)
                    {
                        var val = regKey.GetValue(n);
                        var typ = regKey.GetValueKind(n);
                        var regVal = new ComputedRegValue { Type = RegUtil.ToString(typ) };
                        switch (typ)
                        {
                            case RegistryValueKind.MultiString:
                                regVal.Values = (string[])regKey.GetValue(n);
                                break;
                            case RegistryValueKind.Binary:
                                regVal.ValueBase64 = Convert.ToBase64String((byte[])regKey.GetValue(n)
                                    ?? new byte[0]);
                                break;
                            default:
                                regVal.Value = regKey.GetValue(n)?.ToString();
                                break;
                        }

                        result.State.Entries.Add(n, regVal);
                    }
                }
            }

            return result;
        }
    }
}