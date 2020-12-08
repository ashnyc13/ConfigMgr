using Microsoft.Extensions.Configuration;

namespace ConsoleApp
{
    class RulesEvaluationConfigurationSource : IConfigurationSource
    {
        private readonly object _context;

        public RulesEvaluationConfigurationSource(object context)
        {
            _context = context;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RulesEvaluationConfigurationProvider(builder, _context);
        }
    }
}