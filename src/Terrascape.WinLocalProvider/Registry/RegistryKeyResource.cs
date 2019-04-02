using System.Collections.Generic;
using HC.TFPlugin.Attributes;

namespace Terrascape.WinLocalProvider.Registry
{
    [TFResource("winlo_registry_key", Version = 1L)]
    public class RegistryKeyResource
    {
        [TFArgument("root",
            Required = true,
            ForceNew = true)]
        public string Root { get; set; }

        [TFArgument("path",
            Required = true,
            ForceNew = true)]
        public string Path { get; set; }

        [TFNested("entry")]
        public Dictionary<string, ArgumentRegValue> Entries { get; set; }
    }
}