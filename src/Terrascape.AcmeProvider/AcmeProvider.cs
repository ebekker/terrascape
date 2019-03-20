using HC.TFPlugin;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using Terrascape.AcmeProvider;

[assembly: TFPlugin(typeof(AcmeProvider),
    Resources = new[] {
        typeof(AccountResource),
        typeof(CertificateResource),
        typeof(FileResource),
    })]

namespace Terrascape.AcmeProvider
{
    [TFProvider]
    public partial class AcmeProvider :
        IHasConfigure,
        IHasPrepareProviderConfig
    {
        private readonly ILogger _log = LogUtil.Create<AcmeProvider>();

        /// <summary>
        /// The URL to the ACME endpoint's directory.
        /// </summary>
        /// <value></value>
        [TFArgument("server_url", Required = true)]
        public string ServerUrl { get; set; }

        public ConfigureResult Configure(ConfigureInput input)
        {
            _log.LogDebug("{method}: ", nameof(Configure));
            _log.LogTrace("->input = {@input}", input);
            _log.LogTrace("->state = {@state}", this);

            // TODO: configure and return any validation errors
            var result = new ConfigureResult();

            _log.LogTrace("<-state = {@state}", this);
            _log.LogTrace("<-result = {@result}", result);
            return result;
        }

        public PrepareProviderConfigResult PrepareConfig(PrepareProviderConfigInput input)
        {
            _log.LogDebug("{method}: ", nameof(PrepareConfig));
            _log.LogTrace("->input = {@input}", input);
            _log.LogTrace("->state = {@state}", this);

            // TODO: configure and return any validation errors
            var result = new PrepareProviderConfigResult();

            _log.LogTrace("<-state = {@state}", this);
            _log.LogTrace("<-result = {@result}", this);
            return result;
        }
    }
}