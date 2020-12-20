using Microsoft.Extensions.Configuration;

namespace ConsoleApp
{
    static class ConfigurationExtensions
    {
        public static IConfiguration ExtendWithRules(this IConfiguration configuration, object context)
        {
            var evaluator = new RuleEvaluator();
            var rulesBasedConfig = new RuleBasedConfiguration(configuration, context, evaluator);
            return rulesBasedConfig;
        }
    }
}