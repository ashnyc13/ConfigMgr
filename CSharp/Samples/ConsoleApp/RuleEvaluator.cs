using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ConsoleApp
{
    class RuleEvaluator
    {
        private static readonly IDictionary<Tuple<string, Type>, ScriptRunner<bool>> _lambdaCache =
            new ConcurrentDictionary<Tuple<string, Type>, ScriptRunner<bool>>();

        public bool EvaluateRule(string rule, object context)
        {
            if (string.IsNullOrEmpty(rule))
                throw new System.ArgumentException($"'{nameof(rule)}' cannot be null or empty", nameof(rule));
            if (context is null)
                throw new System.ArgumentNullException(nameof(context));

            // Check if we have a cached lamda available
            var contextType = context.GetType();
            var cacheKey = new Tuple<string, Type>(rule, contextType);
            if (_lambdaCache.ContainsKey(cacheKey))
            {
                return ExecuteRuleScript(_lambdaCache[cacheKey], context);
            }

            // Otherwise compile a new lambda and cache it
            var script = CSharpScript.Create<bool>(rule, null, contextType);
            var runner = script.CreateDelegate();
            _lambdaCache[cacheKey] = runner;
            return ExecuteRuleScript(runner, context);
        }

        private bool ExecuteRuleScript(ScriptRunner<bool> runner, object context)
        {
            var task = runner.Invoke(context);
            if (!task.IsCompleted) task.RunSynchronously();
            return task.Result;
        }
    }
}