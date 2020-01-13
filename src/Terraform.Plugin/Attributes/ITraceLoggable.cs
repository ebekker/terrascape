using Microsoft.Extensions.Logging;

namespace Terraform.Plugin.Attributes
{
    internal interface ITraceLoggable
    {
         ILogger Logger { get; }
    }
}