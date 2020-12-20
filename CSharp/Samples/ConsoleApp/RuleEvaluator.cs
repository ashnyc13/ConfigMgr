using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ConsoleApp
{
    interface IRuleEvaluator
    {
        void AddRuleToCache(string rule, Type contextType);
        bool EvaluateRule(string rule, object context);
    }

    class RuleEvaluator : IRuleEvaluator
    {
        private static readonly IDictionary<Tuple<string, Type>, ScriptRunner<bool>> _lambdaCache =
            new ConcurrentDictionary<Tuple<string, Type>, ScriptRunner<bool>>();

        public void AddRuleToCache(string rule, Type contextType)
        {
            var cacheKey = CreateCacheKey(rule, contextType);
            var script = CSharpScript.Create<bool>(rule, null, contextType);
            var runner = script.CreateDelegate();
            _lambdaCache[cacheKey] = runner;
        }

        public bool EvaluateRule(string rule, object context)
        {
            if (string.IsNullOrEmpty(rule))
                throw new System.ArgumentException($"'{nameof(rule)}' cannot be null or empty", nameof(rule));
            if (context is null)
                throw new System.ArgumentNullException(nameof(context));

            // Check if we have a cached lamda available
            var contextType = context.GetType();
            var cacheKey = CreateCacheKey(rule, contextType);
            if (!_lambdaCache.ContainsKey(cacheKey))
            {
                // If not, compile a new lambda and cache it
                AddRuleToCache(rule, contextType);
            }

            return ExecuteRuleScript(_lambdaCache[cacheKey], context);
        }

        private Tuple<string, Type> CreateCacheKey(string rule, Type contextType)
        {
            return new Tuple<string, Type>(rule, contextType);
        }

        private bool ExecuteRuleScript(ScriptRunner<bool> runner, object context)
        {
            var task = runner.Invoke(context);
            if (!task.IsCompleted) task.RunSynchronously();
            return task.Result;
        }
    }
}