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

        /// <summary>
        /// When this resource is marked for deletion, by default
        /// we only delete the registry key if (1) we originally
        /// created the key to begin with, and (2) it has no subkeys
        /// and no values after all the values that we manage have
        /// been removed.  By setting this flag to true, we override
        /// this behavior and always delete the registry key.
        /// </summary>
        /// <value></value>
        [TFArgument("force_on_delete",
            Optional = true)]
        public bool? ForceOnDelete { get; set; }

        [TFNested("entry")]
        public Dictionary<string, ArgumentRegValue> Entries { get; set; }
    }
}