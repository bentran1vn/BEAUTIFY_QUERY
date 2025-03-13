using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Behaviors;
using BEAUTIFY_QUERY.APPLICATION.Mapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BEAUTIFY_QUERY.APPLICATION.DependencyInjection.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatRApplication(this IServiceCollection services)
    {
        return services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly)
            )
            // .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationDefaultBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformancePipelineBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingPipelineBehaviorCachingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehavior<,>))
            .AddValidatorsFromAssembly(CONTRACT.AssemblyReference.Assembly, includeInternalTypes: true);
    }

    public static IServiceCollection AddAutoMapperApplication(this IServiceCollection services)
    {
        return services.AddAutoMapper(typeof(ServiceProfile));
    }
}