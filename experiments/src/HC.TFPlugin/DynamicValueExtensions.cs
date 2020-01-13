using Microsoft.Extensions.Logging;

namespace HC.TFPlugin
{
    // Special Support for go-cty "Unknown Values":
    //  https://github.com/zclconf/go-cty/blob/master/cty/msgpack/doc.go
    //  https://github.com/zclconf/go-cty/blob/master/cty/msgpack/unknown.go

    public static partial class DynamicValueExtensions
    {
        private static ILogger _log = LogUtil.Create(typeof(DynamicValueExtensions));
    }
}