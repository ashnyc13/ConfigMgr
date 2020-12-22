using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ConsoleApp
{
    static class ConfigurationExtensions
    {
        public static IConfiguration ExtendWithRules(this IConfiguration configuration,
            IContextProvider contextProvider)
        {
            var evaluator = new RuleEvaluator();
            return new RuleBasedConfiguration(configuration, contextProvider, evaluator);
        }

        public static IServiceCollection ConfigureWithRules<TOptions>(
            this IServiceCollection services, IConfiguration config,
            string name = "", Action<BinderOptions> configureBinder = null)
            where TOptions : class
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (configureBinder is null)
            {
                configureBinder = (options) => { };
            }

            // Add required dependencies
            services.TryAddSingleton<IRuleEvaluator, RuleEvaluator>();
            services.Replace(ServiceDescriptor.Transient<IConfiguration>(sp =>
            {
                var provider = sp.GetRequiredService<IContextProvider>();
                var evaluator = sp.GetRequiredService<IRuleEvaluator>();
                return new RuleBasedConfiguration(config, provider, evaluator);
            }));

            // Add requested options.
            services.AddOptions();
            services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return (IOptionsChangeTokenSource<TOptions>)
                    new ConfigurationChangeTokenSource<TOptions>(name, config);
            });
            return services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return (IConfigureOptions<TOptions>)
                    new NamedConfigureFromConfigurationOptions<TOptions>(name, config, configureBinder);
            });
        }
    }
}