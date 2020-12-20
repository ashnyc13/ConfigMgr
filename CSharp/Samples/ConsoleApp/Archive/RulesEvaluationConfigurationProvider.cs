// using System.Collections.Generic;
// using Microsoft.Extensions.Configuration;

// namespace ConsoleApp
// {
//     class RulesEvaluationConfigurationProvider : ConfigurationProvider
//     {
//         private readonly IConfigurationBuilder _builder;
//         private object _context;

//         public RulesEvaluationConfigurationProvider(IConfigurationBuilder builder, object context)
//         {
//             _builder = builder;
//             _context = context;
//         }

//         public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
//         {
//             var childKeys = base.GetChildKeys(earlierKeys, parentPath);
//             return childKeys;
//         }

//         public override bool TryGet(string key, out string value)
//         {
//             return base.TryGet(key, out value);
//         }

//         public override void Set(string key, string value)
//         {
//             base.Set(key, value);
//         }
//     }
// }