using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HC.TFPlugin.Attributes;
using Microsoft.Extensions.Logging;
using Tfplugin5;
using static Tfplugin5.AttributePath.Types.Step;

namespace HC.TFPlugin
{
    // References:
    //  https://github.com/hashicorp/terraform/blob/eb1346447fc635b5dea8e31112de129bf5dedfb4/providers/provider.go
    //  https://github.com/hashicorp/terraform/blob/6317d529a9194a7d2d27e80f7f855d381eeffd8a/builtin/providers/terraform/provider.go

    public static class ProviderHelper
    {
        private static readonly ILogger _log = LogUtil.Create(typeof(ProviderHelper));
        
        // public static object Configure(
        //     DynamicValue config, Assembly asm = null)
        // {
        //     try
        //     {
        //         // Default prepared config to incoming config
        //         var preparedConfig = config;

        //         var plugin = SchemaHelper.GetPluginDetails(asm);
        //         var provider = DVHelper.UnmarshalViaJson(plugin.Provider, config);

        //         if (typeof(IHasConfigure).IsAssignableFrom(plugin.Provider))
        //         {
        //             (provider as IHasConfigure).Configure();
        //         }
                
        //         return provider;
        //     }
        //     catch (Exception ex)
        //     {
        //         _log.LogError(ex, $"<!exception from {nameof(Configure)} =");
        //         throw;
        //     }
        // }

        // public static (object provider,DynamicValue preparedConfig) PrepareProviderConfig(
        //     DynamicValue config, Assembly asm = null)
        // {
        //     try
        //     {
        //         // Default prepared config to incoming config
        //         var preparedConfig = config;

        //         var plugin = SchemaHelper.GetPluginDetails(asm);
        //         var provider = DVHelper.UnmarshalViaJson(plugin.Provider, config);

        //         if (typeof(IHasPrepareProviderConfig).IsAssignableFrom(plugin.Provider))
        //         {
        //             (provider as IHasPrepareProviderConfig).PrepareConfig();
        //             preparedConfig = DVHelper.MarshalViaJson(plugin.Provider, provider);
        //         }
                
        //         return (provider, preparedConfig);
        //     }
        //     catch (Exception ex)
        //     {
        //         _log.LogError(ex, $"<!exception from {nameof(PrepareProviderConfig)} =");
        //         throw;
        //     }
        // }

        // public static void ValidateResourceTypeConfig(object provider,
        //     string type, DynamicValue config, Assembly asm = null)
        // {
        //     try
        //     {
        //         var plugin = SchemaHelper.GetPluginDetails(asm);
        //         var resType = plugin.Resources.Where(x =>
        //             type == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();

        //         var res = DVHelper.UnmarshalViaJson(resType, config);
        //         var providerResValidator = typeof(IHasValidateResourceTypeConfig<>).MakeGenericType(resType);
        //         if (providerResValidator.IsAssignableFrom(plugin.Provider))
        //         {
        //             providerResValidator.GetMethod(
        //                 nameof(IHasValidateResourceTypeConfig<object>.ValidateConfig),
        //                 new[] { resType }).Invoke(provider, new[] { res });
        //         }
        //         // // else if (typeof(IHasValidateResourceTypeConfig).IsAssignableFrom(resType))
        //         // // {
        //         // //     (res as IHasValidateResourceTypeConfig).ValidateConfig();
        //         // // }
        //     }
        //     catch (Exception ex)
        //     {
        //         _log.LogError(ex, $"<!exception from {nameof(ValidateDataSourceConfig)} =");
        //         throw;
        //     }
        // }

        // public static IEnumerable<ValuePath> PlanResourceChange(object provider,
        //     string type, DynamicValue priorState, DynamicValue config,
        //     out DynamicValue plannedState, Assembly asm = null)
        // {
        //     try
        //     {
        //         // Default to planned state equal to input config
        //         plannedState = config;

        //         var plugin = SchemaHelper.GetPluginDetails(asm);
        //         var resType = plugin.Resources.Where(x =>
        //             type == x.GetCustomAttribute<TFResourceAttribute>()?.Name).First();

        //         var priorStateRes = DVHelper.UnmarshalViaJson(resType, priorState);
        //         var configRes = DVHelper.UnmarshalViaJson(resType, config);

        //         var providerResPlanner = typeof(IHasPlanResourceChange<>).MakeGenericType(resType);
        //         if (providerResPlanner.IsAssignableFrom(plugin.Provider))
        //         {
        //             var replaceChanges = providerResPlanner.GetMethod(
        //                 nameof(IHasPlanResourceChange<object>.PlanChange),
        //                 new[] { resType }).Invoke(provider, new[] { priorStateRes, configRes });
                    
        //             plannedState = DVHelper.MarshalViaJson(resType, configRes);
        //             return (IEnumerable<ValuePath>)replaceChanges;
        //         }

        //         return null;
        //     }
        //     catch (Exception ex)
        //     {
        //         _log.LogError(ex, $"<!exception from {nameof(PlanResourceChange)} =");
        //         throw;
        //     }
        // }
    }

    // public interface IHasValidateResourceTypeConfig<T>
    // {
    //     void ValidateConfig(T res);
    // }

    // // public interface IHasValidateResourceTypeConfig
    // // {
    // //     void ValidateConfig();
    // // }


}
