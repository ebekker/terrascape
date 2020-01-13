using System.Collections.Generic;
using OSP = System.Runtime.InteropServices.OSPlatform;
using HC.TFPlugin;
using HC.TFPlugin.Attributes;

namespace Terrascape.LocalProvider
{
    [TFDataSource("lo_sys_info")]
    public class SystemInfoDataSource
    {
        public const string OtherOSPlatform = "Other";

        public static readonly IEnumerable<OSP> OSPlatforms = new[]
        {
            OSP.FreeBSD,
            OSP.Linux,
            OSP.OSX,
            OSP.Windows,
        };

        [TFComputed("process_architecture")]
        public string ProcessArchitecture { get; set; }

        [TFComputed("os_architecture")]
        public string OSArchitecture { get; set; }

        [TFComputed("os_platform")]
        public string OSPlatform { get; set; }

        [TFComputed("os_description")]
        public string OSDescription { get; set; }

        [TFComputed("os_version_string")]
        public string OSVersionString { get; set; }

        [TFComputed("framework_description")]
        public string FrameworkDescription { get; set; }
    }
}