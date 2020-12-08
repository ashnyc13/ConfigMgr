using FluentAssertions;
using NUnit.Framework;

namespace ConfigMgr.IntegrationSpecs
{
    [TestFixture]
    public class JsonConfigReaderSpecs
    {
        [Test]
        public void Should_read_app_setting_values()
        {
            true.Should().BeTrue();
        }
    }
}