using Microsoft.Extensions.DependencyInjection;

namespace TinyCMS.Base
{
    public interface ITinyCmsBuilder
    {
        IServiceCollection Services { get; }
    }
}
