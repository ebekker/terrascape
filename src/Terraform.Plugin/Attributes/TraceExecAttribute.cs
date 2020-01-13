using System;
using System.Reflection;
using MethodBoundaryAspect.Fody.Attributes;
using Microsoft.Extensions.Logging;

namespace Terraform.Plugin.Attributes
{
    /// <summary>
    /// This attribute defines log tracing logic for method entry and exit/exception
    /// with special support for implementors of <see cref="ITraceLoggable" />.  It
    /// works with the Fody `MethodBoundaryAspect` addin to weave in this tracing
    /// behavior during compile time.  If the target instance of the method invocation
    /// implements ITraceLoggable it will use that logger to emit trace messages,
    /// otherwise it will default to its own internal global logger.
    /// </summary>
    internal class TraceExecAttribute : OnMethodBoundaryAspect
    {
        private static readonly ILogger DefaultLogger =
            TFPluginServer.LoggerFactory.CreateLogger<TraceExecAttribute>();

        public override void OnEntry(MethodExecutionArgs exec)
        {
            var logger = (exec.Instance as ITraceLoggable)?.Logger ?? DefaultLogger;
            var execParams = exec.Method.GetParameters();

            logger.LogDebug(">>>entered:{method}>>>", exec?.Method?.Name);

            if (execParams?.Length > 0 && exec.Arguments?.Length > 0)
            {
                for (int i = 0; i < execParams.Length; ++i)
                {
                    try
                    {
                        logger.LogTrace("->input[{@inputName}] = {@input}",
                            execParams[i].Name, exec.Arguments[i]);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("FAILED TO TRACE INPUT PARAMETER DETAILS - ALT SHALLOW: {0}", ex.Message);
                        logger.LogTrace("->input[{@inputName}] = {input}",
                            execParams[i].Name, exec.Arguments[i]);
                    }
                }
            }
        }

        public override void OnExit(MethodExecutionArgs exec)
        {
            var logger = (exec.Instance as ITraceLoggable)?.Logger ?? DefaultLogger;
            var returnType = (exec.Method as MethodInfo)?.ReturnType;

            logger.LogDebug("<<<exited:{method}", exec.Method.Name);
            if (!(returnType is null))
                logger.LogTrace("<-result = {@return}<<<", exec.ReturnValue);
        }

        public override void OnException(MethodExecutionArgs exec)
        {
            var logger = (exec.Instance as ITraceLoggable)?.Logger ?? DefaultLogger;
            logger.LogError(exec.Exception, "<<<!thrown:{method}", exec.Method.Name);
        }
    }
}