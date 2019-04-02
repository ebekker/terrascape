using HC.TFPlugin.Attributes;

namespace Terrascape.WinLocalProvider.Registry
{
    public class ArgumentRegValue
    {
        [TFArgument("type",
            Required = true)]
        public string Type { get; set; }

        [TFArgument("value",
            Optional = true,
            ConflictsWith = new[] {
                nameof(ValueBase64),
                nameof(Values),
            })]
        public string Value { get; set; } = string.Empty;

        [TFArgument("value_base64",
            Optional = true,
            ConflictsWith = new[] {
                nameof(Value),
                nameof(Values),
            })]
        public string ValueBase64 { get; set; } = string.Empty;

        [TFArgument("values",
            Optional = true,
            ConflictsWith = new[] {
                nameof(Value),
                nameof(ValueBase64),
            })]
        public string[] Values { get; set; } = RegUtil.EmptyStrings; 
    }
}