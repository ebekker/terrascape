using System.ComponentModel.DataAnnotations.Schema;
using HC.TFPlugin.Attributes;

namespace Terrascape.WinLocalProvider.Registry
{
    public class ComputedRegValue
    {
        [TFComputed("type")]
        public string Type { get; set; }

        [TFComputed("value")]
        public string Value { get; set; } = string.Empty;

        [TFComputed("value_base64")]
        public string ValueBase64 { get; set; } = string.Empty;

        [TFComputed("values")]
        public string[] Values { get; set; } = RegUtil.EmptyStrings; 
    }
}