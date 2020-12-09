﻿using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ConsoleApp
{
    public class Context
    {
        public int ContextProp1 { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var env = "QA";
            var builder = new ConfigurationBuilder()
                 .AddJsonFile($"App_Config\\appSettings.json", true, true)
                 .AddJsonFile($"App_Config\\appSettings.{env}.json", true, true);

            var config = builder.Build();
            Console.WriteLine(config.GetDebugView());

            var appSettings = new AppSettings();
            config.BindWithRules(appSettings, new Context { ContextProp1 = 12 });
            Console.WriteLine(JsonConvert.SerializeObject(appSettings));
        }
    }
}
