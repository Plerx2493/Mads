using MADS.Entities;
using MADS.JsonModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Extensions;

public static class ExtensionMethods
{
    public static IServiceCollection AddDbFactoryDebugOrRelease(this IServiceCollection serviceCollection,
        ConfigJson config)
    {
        serviceCollection.AddEntityFrameworkMySql();
        serviceCollection.AddDbContextFactory<MadsContext>(
            options => options.UseMySql(config.ConnectionString, ServerVersion.AutoDetect(config.ConnectionString))
        );

        return serviceCollection;
    }
}