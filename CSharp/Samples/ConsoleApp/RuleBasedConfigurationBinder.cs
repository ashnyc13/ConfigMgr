using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ConsoleApp
{
    //
    // Summary:
    //     Static helper class that allows binding strongly typed objects to configuration
    //     values.
    public static class RuleBasedConfigurationBinder
    {
        /// <summary>
        /// Attempts to bind the given object instance to the configuration section specified
        /// by the key by matching property names against configuration keys recursively.
        /// </summary>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="key">The key of the configuration section to bind.</param>
        /// <param name="instance">The object to bind.</param>
        /// <param name="context">The context used for evaluating rules.</param>
        public static void BindWithRules(this IConfiguration configuration, string key, object instance, object context)
        {
            configuration.GetSection(key).BindWithRules(instance, context);
        }

        /// <summary>
        /// Attempts to bind the given object instance to configuration values by matching
        /// property names against configuration keys recursively.
        /// </summary>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="instance">The object to bind.</param>
        /// <param name="context">The context used for evaluating rules.</param>
        public static void BindWithRules(this IConfiguration configuration, object instance, object context)
        {
            configuration.BindWithRules(instance, delegate { }, context);
        }

        /// <summary>
        /// Attempts to bind the given object instance to configuration values by matching
        /// property names against configuration keys recursively.
        /// </summary>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="instance">The object to bind.</param>
        /// <param name="configureOptions">Configures the binder options.</param>
        /// <param name="context">The context used for evaluating rules.</param>
        public static void BindWithRules(this IConfiguration configuration, object instance,
            Action<BinderOptions> configureOptions, object context)
        {
            var finalConfig = EvaluateRules(configuration, context);
            finalConfig.Bind(instance, configureOptions);
        }

        // //
        // // Summary:
        // //     Attempts to bind the configuration instance to a new instance of type T. If this
        // //     configuration section has a value, that will be used. Otherwise binding by matching
        // //     property names against configuration keys recursively.
        // //
        // // Parameters:
        // //   configuration:
        // //     The configuration instance to bind.
        // //
        // // Type parameters:
        // //   T:
        // //     The type of the new instance to bind.
        // //
        // // Returns:
        // //     The new instance of T if successful, default(T) otherwise.
        // public static T Get<T>(this IConfiguration configuration);
        // //
        // // Summary:
        // //     Attempts to bind the configuration instance to a new instance of type T. If this
        // //     configuration section has a value, that will be used. Otherwise binding by matching
        // //     property names against configuration keys recursively.
        // //
        // // Parameters:
        // //   configuration:
        // //     The configuration instance to bind.
        // //
        // //   configureOptions:
        // //     Configures the binder options.
        // //
        // // Type parameters:
        // //   T:
        // //     The type of the new instance to bind.
        // //
        // // Returns:
        // //     The new instance of T if successful, default(T) otherwise.
        // public static T Get<T>(this IConfiguration configuration, Action<BinderOptions> configureOptions);
        // //
        // // Summary:
        // //     Attempts to bind the configuration instance to a new instance of type T. If this
        // //     configuration section has a value, that will be used. Otherwise binding by matching
        // //     property names against configuration keys recursively.
        // //
        // // Parameters:
        // //   configuration:
        // //     The configuration instance to bind.
        // //
        // //   type:
        // //     The type of the new instance to bind.
        // //
        // // Returns:
        // //     The new instance if successful, null otherwise.
        // public static object Get(this IConfiguration configuration, Type type);
        // //
        // // Summary:
        // //     Attempts to bind the configuration instance to a new instance of type T. If this
        // //     configuration section has a value, that will be used. Otherwise binding by matching
        // //     property names against configuration keys recursively.
        // //
        // // Parameters:
        // //   configuration:
        // //     The configuration instance to bind.
        // //
        // //   type:
        // //     The type of the new instance to bind.
        // //
        // //   configureOptions:
        // //     Configures the binder options.
        // //
        // // Returns:
        // //     The new instance if successful, null otherwise.
        // public static object Get(this IConfiguration configuration, Type type, Action<BinderOptions> configureOptions);

        private static IConfiguration EvaluateRules(IConfiguration configuration, object context)
        {
            return new RuleBasedConfiguration(configuration, context);
        }
    }

    class RuleBasedConfiguration : IConfiguration
    {
        private readonly IConfiguration _internalConfig;
        private readonly object _context;

        public RuleBasedConfiguration(IConfiguration internalConfig, object context)
        {
            _internalConfig = internalConfig;
            _context = context;
        }

        public string this[string key]
        {
            get => _internalConfig[key];
            set => _internalConfig[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _internalConfig.GetChildren().Select(section => new RulesBasedConfigSection(section, _context));
        }

        public IChangeToken GetReloadToken()
        {
            return _internalConfig.GetReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            return new RulesBasedConfigSection(_internalConfig.GetSection(key), _context);
        }
    }

    class RulesBasedConfigSection : RuleBasedConfiguration, IConfigurationSection
    {
        private readonly IConfigurationSection _internalSection;

        public RulesBasedConfigSection(IConfigurationSection internalSection, object context) :
            base(internalSection, context)
        {
            _internalSection = internalSection;
        }

        public string Key => _internalSection.Key;

        public string Path => _internalSection.Path;

        public string Value
        {
            get => _internalSection.Value;
            set => _internalSection.Value = value;
        }
    }
}