using Microsoft.Extensions.Configuration;

namespace ConsoleApp
{
    static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder EvaluateRulesUsing(this IConfigurationBuilder builder, object context)
        {
            builder.Add(new RulesEvaluationConfigurationSource(context));
            return builder;
        }
    }
}