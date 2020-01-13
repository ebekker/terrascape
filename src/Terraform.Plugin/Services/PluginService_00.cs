using System;
using Microsoft.Extensions.Logging;
using Terraform.Plugin.Attributes;
using Tfplugin5;

namespace Terraform.Plugin.Services
{
    /// <summary>
    /// This class implements the RPC service defined by the protobuf
    /// description for a Terraform Provider.
    /// 
    /// Because each RPC method is rather involved and potentially
    /// complicated on its own, each one is broken out into its own
    /// source file by using the _partial_ mechanism.
    /// </summary>
    public partial class PluginService : Provider.ProviderBase, ITraceLoggable
    {
        private readonly ILogger _log;

        private readonly TFPluginServer _pluginServer;

        private readonly ISchemaResolver _schemaResolver;

        private object _PluginProviderInstance;

        ILogger ITraceLoggable.Logger => _log;

        public PluginService(ILogger<PluginService> logger,
            TFPluginServer pluginServer, ISchemaResolver schemaResolver)
        {
            _log = logger;
            _pluginServer = pluginServer;
            _schemaResolver = schemaResolver;
            _log.LogInformation("PluginService constructed");
        }

        public object PluginProviderInstance
        {
            get
            {
                if (_PluginProviderInstance == null)
                {
                    var providerType = _schemaResolver.PluginDetails.Provider;
                    _PluginProviderInstance = _pluginServer.Services.GetService(providerType);

                    if (_PluginProviderInstance == null)
                    {
                        _log.LogInformation("No explicit Plugin Provider instance registered, attempting fallback");
                        var fallbackType = typeof(Util.Fallback<>).MakeGenericType(providerType);
                        var fallback = _pluginServer.Services.GetService(fallbackType);
                        if (fallback == null)
                            throw new Exception("Unable to resolve provider type");

                        _log.LogInformation("Fallback container resolved, resolving provider instance");                        
                        _PluginProviderInstance = fallbackType
                            .GetProperty(nameof(Util.Fallback<object>.Instance))
                            .GetValue(fallback);
                        if (fallback == null)
                            throw new Exception("Unable to resolve provider type using fallback");
                    }
                }
                return _PluginProviderInstance;
            }
        }
    }
}