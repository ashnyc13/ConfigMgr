﻿using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp
{
    public class Context
    {
        public int ContextProp1 { get; set; }
    }

    public class ContextProvider : IContextProvider
    {
        public object GetContext()
        {
            return new Context { ContextProp1 = 12 };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Craete config
            var builder = new ConfigurationBuilder()
                 .AddJsonFile("App_Config\\appSettings.json", false, true)
                 .AddJsonFile("App_Config\\appSettings.QA.json", false, true);
            IConfiguration config = builder.Build();

            // Register services and configuration in IOC
            var hostBuilder = Host.CreateDefaultBuilder();
            hostBuilder.ConfigureServices(sc =>
                {
                    sc.AddTransient<IContextProvider, ContextProvider>();
                    sc.ConfigureWithRules<AppSettings>(config);
                    sc.AddTransient<IGreetingService, GreetingService>();
                });
            var host = hostBuilder.Build();

            // Run the service
            var newScope = host.Services.CreateScope();
            var service = newScope.ServiceProvider.GetService<IGreetingService>();
            service.SayHello();

            // Update the JSON file
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var jsonFile = $"{currentDir}App_Config\\appSettings.json";
            var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(jsonFile));
            var jChildObj = jObject.SelectToken("BoolConfig._rules[0]");
            jChildObj["when"] = "ContextProp1 > 100";
            using (var streamWriter = File.CreateText(jsonFile))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                jObject.WriteTo(writer);
                writer.Flush();
            }

            // Update context
            //context.ContextProp1 = 6;

            // Wait for file update notification to fire
            Thread.Sleep(1000);

            // Re-run the service method in a new scope
            newScope = host.Services.CreateScope();
            service = newScope.ServiceProvider.GetService<IGreetingService>();
            service.SayHello();
        }
    }
    public interface IGreetingService
    {
        void SayHello();
    }


    public class GreetingService : IGreetingService
    {
        private readonly IOptionsSnapshot<AppSettings> _appSettings;

        public GreetingService(IOptionsSnapshot<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }
        public void SayHello()
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine(JsonConvert.SerializeObject(_appSettings.Value));
        }
    }
}
