using System.Collections.Generic;
using HC.TFPlugin.Attributes;

namespace Terrascape.WinLocalProvider.Registry
{
    [TFDataSource("winlo_registry_key", Version = 1L)]
    public partial class RegistryKeyDataSource
    {
        [TFArgument("root",
            Required = true)]
        public string Root { get; set; }

        [TFArgument("path",
            Required = true)]
        public string Path { get; set; }

        [TFComputed("key_names")]
        public string[] KeyNames { get; set; }

        [TFComputed("value_names")]
        public string[] ValueNames { get; set; }

        [TFNested("entries")]
        public Dictionary<string, ComputedRegValue> Entries { get; set; }
    }
}