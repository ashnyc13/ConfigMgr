using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class ConditionaValue
    {
        public string When { get; set; }
        public object Value { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var env = "QA";
            var builder = new ConfigurationBuilder()
                //  .AddJsonFile($"App_Config\\appSettings.json", true, true)
                //  .AddJsonFile($"App_Config\\appSettings.{env}.json", true, true);
                .AddJsonFile($"App_Config\\appSettings.json", true, true)
                .EvaluateRulesUsing(new { ContextProp1 = 12 });

            var config = builder.Build();
            //var appSettings = config.Get<AppSettings>();
            Console.WriteLine(config.GetDebugView());
        }
    }
}
