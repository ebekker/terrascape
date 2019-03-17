using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HC.TFPlugin.Attributes;
using Tfplugin5;
using static Tfplugin5.AttributePath.Types.Step;

namespace HC.TFPlugin
{
    // References:
    //  https://github.com/hashicorp/terraform/blob/eb1346447fc635b5dea8e31112de129bf5dedfb4/providers/provider.go
    //  https://github.com/hashicorp/terraform/blob/6317d529a9194a7d2d27e80f7f855d381eeffd8a/builtin/providers/terraform/provider.go

    public static class ProviderHelper
    {
        public static object Configure(
            DynamicValue config, Assembly asm = null)
        {
            try
            {
                // Default prepared config to incoming config
                var preparedConfig = config;

                var plugin = SchemaHelper.GetPluginDetails(asm);
                var provider = DVHelper.UnmarshalViaJson(plugin.Provider, config);

                if (typeof(IHasConfigure).IsAssignableFrom(plugin.Provider))
                {
                    (provider as IHasConfigure).Configure();
                }
                
                return provider;
            }
            catch (Exception ex)
            {
                Dumper.Out.WriteLine("ERROR: " + ex);
                throw;
            }
        }

        public static (object provider,DynamicValue preparedConfig) PrepareProviderConfig(
            DynamicValue config, Assembly asm = null)
        {
            try
            {
                // Default prepared config to incoming config
                var preparedConfig = config;

                var plugin = SchemaHelper.GetPluginDetails(asm);
                var provider = DVHelper.UnmarshalViaJson(plugin.Provider, config);

                if (typeof(IHasPrepareProviderConfig).IsAssignableFrom(plugin.Provider))
                {
                    (provider as IHasPrepareProviderConfig).PrepareConfig();
                    preparedConfig = DVHelper.MarshalViaJson(plugin.Provider, provider);
                }
                
                return (provider, preparedConfig);
            }
            catch (Exception ex)
            {
                Dumper.Out.WriteLine("ERROR: " + ex);
                throw;
            }
        }

        public static void ValidateResourceTypeConfig(object provider,
            string type, DynamicValue config, Assembly asm = null)
        {
            try
            {
                var plugin = SchemaHelper.GetPluginDetails(asm);
                var resType = plugin.Resources.Where(x =>
                    type == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();

                var res = DVHelper.UnmarshalViaJson(resType, config);
                var providerResValidator = typeof(IHasValidateResourceTypeConfig<>).MakeGenericType(resType);
                if (providerResValidator.IsAssignableFrom(plugin.Provider))
                {
                    providerResValidator.GetMethod(
                        nameof(IHasValidateResourceTypeConfig<object>.ValidateConfig),
                        new[] { resType }).Invoke(provider, new[] { res });
                }
                // // else if (typeof(IHasValidateResourceTypeConfig).IsAssignableFrom(resType))
                // // {
                // //     (res as IHasValidateResourceTypeConfig).ValidateConfig();
                // // }
            }
            catch (Exception ex)
            {
                Dumper.Out.WriteLine("ERROR: " + ex);
                throw;
            }
        }

        public static IEnumerable<ValuePath> PlanResourceChange(object provider,
            string type, DynamicValue priorState, DynamicValue config,
            out DynamicValue plannedState, Assembly asm = null)
        {
            try
            {
                // Default to planned state equal to input config
                plannedState = config;

                var plugin = SchemaHelper.GetPluginDetails(asm);
                var resType = plugin.Resources.Where(x =>
                    type == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();

                var priorStateRes = DVHelper.UnmarshalViaJson(resType, priorState);
                var configRes = DVHelper.UnmarshalViaJson(resType, config);

                var providerResPlanner = typeof(IHasPlanResourceChange<>).MakeGenericType(resType);
                if (providerResPlanner.IsAssignableFrom(plugin.Provider))
                {
                    var replaceChanges = providerResPlanner.GetMethod(
                        nameof(IHasPlanResourceChange<object>.PlanChange),
                        new[] { resType }).Invoke(provider, new[] { priorStateRes, configRes });
                    
                    plannedState = DVHelper.MarshalViaJson(resType, configRes);
                    return (IEnumerable<ValuePath>)replaceChanges;
                }

                return null;
            }
            catch (Exception ex)
            {
                Dumper.Out.WriteLine("ERROR: " + ex);
                throw;
            }
        }
    }

    public class ValuePath
    {
        private List<(SelectorOneofCase selector, object arg)> _path =
            new List<(SelectorOneofCase selector, object arg)>();

        public IEnumerable<(SelectorOneofCase selector, object arg)> Segments => _path;

        public ValuePath Attribute(string name)
        {
            _path.Add((SelectorOneofCase.AttributeName, name));
            return this;
        }

        public ValuePath Element(long index)
        {
            _path.Add((SelectorOneofCase.ElementKeyInt, index));
            return this;
        }

        public ValuePath Element(string key)
        {
            _path.Add((SelectorOneofCase.ElementKeyString, key));
            return this;
        }
    }

    public interface IHasConfigure
    {
        void Configure();
    }

    /// <summary>
    /// PrepareProviderConfig allows the provider to validate the configuration
	/// values, and set or override any values with defaults.
    /// </summary>
    public interface IHasPrepareProviderConfig
    {
        void PrepareConfig();
    }

    public interface IHasValidateResourceTypeConfig<T>
    {
        void ValidateConfig(T res);
    }

    // // public interface IHasValidateResourceTypeConfig
    // // {
    // //     void ValidateConfig();
    // // }


}
