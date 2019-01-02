using Microsoft.Extensions.DependencyInjection;

namespace TinyCMS.Base
{
    public class TinyCmsBuilder : ITinyCmsBuilder
    {
        public IServiceCollection Services { get; }
        public TinyCmsBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
