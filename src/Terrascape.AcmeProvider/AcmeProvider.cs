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
        [TFAttribute("server_url", Required = true)]
        public string ServerUrl { get; set; }

        public void Configure()
        {
            _log.LogInformation("Configuring...");
            _log.LogInformation($"  * [{nameof(ServerUrl)}]=[{ServerUrl}]");
        }

        public void PrepareConfig()
        {
            _log.LogInformation("NOTE: AcmeProvider Preparing Config...");
            _log.LogInformation($"  * [{nameof(ServerUrl)}]=[{ServerUrl}]");
        }
    }
}