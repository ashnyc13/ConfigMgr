using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ConsoleApp
{

    interface IRuleEvaluator
    {
        void AddRuleToCache(Rule rule, Type contextType);
        bool EvaluateRule(Rule rule, object context);
        string EvaluateRules(IEnumerable<Rule> rules, object context);
    }

    class RuleEvaluator : IRuleEvaluator
    {
        private static readonly IDictionary<Tuple<string, Type>, ScriptRunner<bool>> _lambdaCache =
            new ConcurrentDictionary<Tuple<string, Type>, ScriptRunner<bool>>();

        public void AddRuleToCache(Rule rule, Type contextType)
        {
            if (rule is null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (contextType is null)
            {
                throw new ArgumentNullException(nameof(contextType));
            }

            if (rule.IsDefault) return;

            var cacheKey = CreateCacheKey(rule.When, contextType);
            var script = CSharpScript.Create<bool>(rule.When, null, contextType);
            var runner = script.CreateDelegate();
            _lambdaCache[cacheKey] = runner;
        }

        public bool EvaluateRule(Rule rule, object context)
        {
            if (rule is null)
                throw new ArgumentNullException(nameof(rule));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (rule.IsDefault)
                throw new ArgumentException("Default rules cannot be executed.", nameof(rule));

            // Check if we have a cached lamda available
            var contextType = context.GetType();
            var cacheKey = CreateCacheKey(rule.When, contextType);
            if (!_lambdaCache.ContainsKey(cacheKey))
            {
                // If not, compile a new lambda and cache it
                AddRuleToCache(rule, contextType);
            }

            return ExecuteRuleScript(_lambdaCache[cacheKey], context);
        }

        public string EvaluateRules(IEnumerable<Rule> rules, object context)
        {
            // Go through each rule and evaluate it
            Rule defaultRule = null;
            foreach (var rule in rules)
            {
                if (rule.IsDefault)
                {
                    defaultRule = rule;
                    continue;
                }

                var ruleResult = EvaluateRule(rule, context);
                if (ruleResult) return rule.Value;
            }

            return defaultRule?.Value;
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