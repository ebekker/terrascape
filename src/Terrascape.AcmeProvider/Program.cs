
using System;
using System.Threading.Tasks;
using HC.TFPlugin;
using Microsoft.Extensions.Logging;

namespace Terrascape.AcmeProvider
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var _log = LogUtil.Create<Program>();

            try
            {
                await TFPluginServer.RunService();
            }
            catch (Exception ex)
            {
                _log.LogInformation(ex, "Exception caught at Program Entry");
                throw;
            }
        }
    }
}
