using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ConsoleApp
{
    class RuleBasedConfiguration : IConfiguration
    {
        private readonly IConfiguration _internalConfig;
        private readonly object _context;
        protected readonly IRuleEvaluator _evaluator;

        public RuleBasedConfiguration(IConfiguration internalConfig, object context, IRuleEvaluator evaluator)
        {
            _internalConfig = internalConfig;
            _context = context;
            _evaluator = evaluator;
        }

        public string this[string key]
        {
            get => _internalConfig[key];
            set => _internalConfig[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _internalConfig.GetChildren().Select(ProcessSection);
        }

        public IChangeToken GetReloadToken()
        {
            return _internalConfig.GetReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            return ProcessSection(_internalConfig.GetSection(key));
        }

        private IConfigurationSection ProcessSection(IConfigurationSection section)
        {
            // If this is a conditional values section then
            // evalate the rules and return the value obtained from it
            var sectionChildren = section.GetChildren();
            var hasConditionalValues = sectionChildren.Any(child => child.Key == "_rules");
            if (hasConditionalValues)
            {
                var value = EvaluateRules(section.GetSection("_rules"), _context);
                section.Value = value;
            }
            return new RulesBasedConfigSection(section, _context, _evaluator);
        }

        private string EvaluateRules(IConfigurationSection section, object context)
        {
            // Get the conditions array from the section
            var conditions = section.GetChildren().ToArray();

            // Get the default condition (i.e. condition without a `when`)
            var defaultCondition = conditions.FirstOrDefault(c => c.GetChildren().All(prop => prop.Key != "when"));

            // Evaluate conditions one-by-one
            foreach (var condition in conditions)
            {
                // Evaluate when using the context object
                var when = condition.GetSection("when");
                if (string.IsNullOrEmpty(when.Value)) continue;
                if (!_evaluator.EvaluateRule(when.Value, _context)) continue;

                // If evaluation is positive
                var value = condition.GetSection("value");
                return value.Value;
            }

            // If no conditions could be evaluated
            // return the value from default condition
            return defaultCondition?.GetSection("value")?.Value;
        }
    }

    class RulesBasedConfigSection : RuleBasedConfiguration, IConfigurationSection
    {
        private readonly IConfigurationSection _internalSection;

        public RulesBasedConfigSection(IConfigurationSection internalSection, object context, IRuleEvaluator ruleEvaluator) :
            base(internalSection, context, ruleEvaluator)
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