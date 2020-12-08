using EnumerableExtensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace ConfigMgr.IntegrationSpecs.Specs
{
    [TestFixture]
    public class JsonConfigReaderSpecs
    {
        [TestCase("simpleAppSettings.json", "simpleAppSettings.qa.json")]
        public void Should_read_simple_app_setting_values(params string[] sourceFiles)
        {
            var builder = new ConfigurationBuilder();
            sourceFiles.ForEach(file => builder.AddJsonFile($"App_Config\\{file}", true, true));

            var config = builder.Build();
            var appSettings = config.Get<AppSettings>();

            appSettings.Should().NotBeNull();
            appSettings.StringConfig.Should().Be("Overriden value");
            appSettings.IntConfig.Should().Be(25);
            appSettings.BoolConfig.Should().BeTrue();
            appSettings.AdditionalConfig.Should().Be("QA only value");
        }
    }
}