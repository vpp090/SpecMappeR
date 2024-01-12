using Microsoft.Extensions.DependencyInjection;

namespace SpecMapperR.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpecMapper(this IServiceCollection services) 
        {
            services.AddScoped<ISpecialMapper, SpecialMapper>();

            return services;
        }
    }
}
