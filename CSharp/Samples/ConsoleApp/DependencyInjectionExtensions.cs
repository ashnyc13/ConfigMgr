
// using System;
// using System.Linq;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection.Extensions;

// namespace ConsoleApp
// {
//     public static class DependencyInjectionExtensions
//     {
//         public static IServiceCollection AddRuleEvaluationDependencies(this IServiceCollection serviceCollection)
//         {
//             // Get the original config. implementation factory
//             // var configServiceSD = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IConfiguration));
//             // if (configServiceSD == null) throw new ArgumentException($"Given service collection doesn't contain a registration for IConfiguration.", nameof(serviceCollection));

//             serviceCollection.AddSingleton<IRuleEvaluator, RuleEvaluator>();
//             // serviceCollection.Replace(ServiceDescriptor.Singleton<IConfiguration>(sp =>
//             // {
//             //     var evaluator = sp.GetRequiredService<IRuleEvaluator>();
//             //     var config = configServiceSD.ImplementationFactory(sp) as IConfiguration;
//             //     return new RuleBasedConfiguration(config, context, evaluator);
//             // }));
//             return serviceCollection;
//         }
//     }
// }