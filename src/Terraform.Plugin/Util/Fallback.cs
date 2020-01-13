using System;
using Microsoft.Extensions.DependencyInjection;

namespace Terraform.Plugin.Util
{
    public class Fallback<T>
    {
        IServiceProvider _services;

        public Fallback(IServiceProvider services)
        {
            _services = services;
        }

        public T Instance => ActivatorUtilities.GetServiceOrCreateInstance<T>(_services);
    }
}