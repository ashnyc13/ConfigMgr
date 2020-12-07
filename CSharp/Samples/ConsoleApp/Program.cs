using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = "QA";
            var builder = new ConfigurationBuilder()
                 .AddJsonFile($"App_Config\\appSettings.json", true, true)
                 .AddJsonFile($"App_Config\\appSettings.{env}.json", true, true);

            var config = builder.Build();
            var appSettings = config.Get<AppSettings>();
            Console.WriteLine(JsonConvert.SerializeObject(appSettings));
        }
    }
}
