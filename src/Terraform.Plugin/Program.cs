using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Terraform.Plugin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new PluginHost();
            host.Init(args);
            await host.RunAsync();
        }
    }
}
