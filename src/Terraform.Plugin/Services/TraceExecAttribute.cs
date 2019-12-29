using System.Reflection;
using MethodBoundaryAspect.Fody.Attributes;
using Microsoft.Extensions.Logging;

namespace Terraform.Plugin.Services
{
    /// <summary>
    /// This attribute defines log tracing logic for method entry and exit/exception
    /// specifically to be used with the <see cref="PluginService" /> class.  It
    /// works with the Fody `MethodBoundaryAspect` addin to weave in this tracing
    /// behavior during compile time.
    /// </summary>
    public class TraceExecAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs exec)
        {
            var ps = exec.Instance as PluginService;
            if (ps == null)
                return;
            
            var logger = ps._log;
            var execParams = exec.Method.GetParameters();

            logger.LogDebug(">>>entered:{method}>>>", exec.Method.Name);

            for (int i = 0; i < execParams.Length; ++i) {
                logger.LogTrace($"->input[{execParams[i].Name}] = {{@input}}",
                    exec.Arguments[i]);
            }
        }

        public override void OnExit(MethodExecutionArgs exec)
        {
            var ps = exec.Instance as PluginService;
            if (ps == null)
                return;
            
            var logger = ps._log;
            var returnType = (exec.Method as MethodInfo)?.ReturnType;

            logger.LogDebug("<<<exited:{method}", exec.Method.Name);
            if (!(returnType is null))
                logger.LogTrace("<-result = {@return}<<<", exec.ReturnValue);
        }

        public override void OnException(MethodExecutionArgs exec)
        {
            var ps = exec.Instance as PluginService;
            if (ps == null)
                return;
            
            var logger = ps._log;
            logger.LogError(exec.Exception, "<<<!thrown:{method}", exec.Method.Name);
        }
    }
}