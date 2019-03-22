using System;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace HC.TFPlugin
{
    public class StopInput
    { }

    public class StopResult
    {
        public string Error { get; set; }
    }

    public interface IHasStop
    {
        StopResult Stop(StopInput input);
    }

    public interface IDataSourceProvider<TDataSource> :
        IHasValidateDataSourceConfig<TDataSource>,
        IHasReadDataSource<TDataSource>
    { }

    public interface IResourceProvider<TResource> :
        IHasValidateResourceTypeConfig<TResource>,
        IHasPlanResourceChange<TResource>,
        IHasApplyResourceChange<TResource>,
        IHasReadResource<TResource>
    { }

    // References:
    //  https://github.com/hashicorp/terraform/blob/eb1346447fc635b5dea8e31112de129bf5dedfb4/providers/provider.go
    //  https://github.com/hashicorp/terraform/blob/6317d529a9194a7d2d27e80f7f855d381eeffd8a/builtin/providers/terraform/provider.go

    public partial class ProviderImpl : Tfplugin5.Provider.ProviderBase
    {
        private ILogger _log = LogUtil.Create<ProviderImpl>();

        private object _ProviderInstance;

        public ProviderImpl(Assembly pluginAssembly = null)
        {
            PluginAssembly = pluginAssembly;
        }

        public Assembly PluginAssembly { get; }

        public object ProviderInstance => _ProviderInstance;

        public override async Task<Tfplugin5.Stop.Types.Response> Stop(
            Tfplugin5.Stop.Types.Request request, ServerCallContext context)
        {
            _log.LogDebug(">>>{method}>>>", nameof(Stop));
            _log.LogTrace($"->input[{nameof(request)}] = {{@request}}", request);
            _log.LogTrace($"->input[{nameof(context)}] = {{@context}}", context);

            try
            {
                if (_ProviderInstance == null)
                    throw new Exception("provider instance was not configured previously");

                var response = new Tfplugin5.Stop.Types.Response();

                var plugin = SchemaHelper.GetPluginDetails(PluginAssembly);
 
                if (typeof(IHasStop).IsAssignableFrom(plugin.Provider))
                {
                    var invokeInput = new StopInput();
                    
                    var invokeResult = (_ProviderInstance as IHasStop).Stop(invokeInput);
                    if (invokeResult == null)
                        throw new Exception("invocation result returned null");

                    if (!string.IsNullOrEmpty(invokeResult.Error))
                        response.Error = invokeResult.Error;
                }

                _log.LogTrace("<-result = {@response}", response);
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "<!exception = ");
                throw;
            }
        }

        /// <summary>
        /// Creates a service definition that can be registered with the server.
        /// </summary>
        public static ServerServiceDefinition BindService(ProviderImpl impl) => Tfplugin5.Provider.BindService(impl);
    }
}
