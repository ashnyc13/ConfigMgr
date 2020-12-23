using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace ConsoleApp
{
    class RuleBasedConfiguration : IConfiguration
    {
        private readonly IConfiguration _internalConfig;
        private readonly IContextProvider _contextProvider;
        protected readonly IRuleEvaluator _evaluator;

        public RuleBasedConfiguration(IConfiguration internalConfig,
            IContextProvider contextProvider, IRuleEvaluator evaluator)
        {
            _internalConfig = internalConfig ?? throw new System.ArgumentNullException(nameof(internalConfig));
            _contextProvider = contextProvider ?? throw new System.ArgumentNullException(nameof(contextProvider));
            _evaluator = evaluator ?? throw new System.ArgumentNullException(nameof(evaluator));
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
                var value = EvaluateRules(section.GetSection("_rules"),
                    _contextProvider.GetContext());
                section.Value = value;
            }
            return new RulesBasedConfigSection(section, _contextProvider, _evaluator);
        }

        private string EvaluateRules(IConfigurationSection section, object context)
        {
            var rules = section.GetChildren().Select(CreateRule);
            return _evaluator.EvaluateRules(rules, context);
        }

        private Rule CreateRule(IConfigurationSection configSection)
        {
            // Get when and value
            var when = configSection.GetSection("when");
            var value = configSection.GetSection("value");
            return new Rule
            {
                When = when.Value,
                Value = value.Value
            };
        }
    }

    class RulesBasedConfigSection : RuleBasedConfiguration, IConfigurationSection
    {
        private readonly IConfigurationSection _internalSection;

        public RulesBasedConfigSection(IConfigurationSection internalSection,
            IContextProvider contextProvider, IRuleEvaluator ruleEvaluator) :
            base(internalSection, contextProvider, ruleEvaluator)
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