using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.PERSISTENCE.DependencyInjection.Options;
using BEAUTIFY_QUERY.PERSISTENCE.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
//using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace BEAUTIFY_QUERY.PERSISTENCE.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
     
     public static void AddSqlServerPersistence(this IServiceCollection services)
     {
         services.AddDbContextPool<DbContext, ApplicationDbContext>((provider, builder) =>
         {
             var configuration = provider.GetRequiredService<IConfiguration>();
             var options = provider.GetRequiredService<IOptionsMonitor<SqlServerRetryOptions>>();

             #region ============== SQL-SERVER-STRATEGY-1 ==============

             builder
                 .EnableDetailedErrors()
                 .EnableSensitiveDataLogging()
                 .UseLazyLoadingProxies() // => If UseLazyLoadingProxies, all of the navigation fields should be VIRTUAL
                 .UseSqlServer(
                     configuration.GetConnectionString("ConnectionStrings"),
                     optionsBuilder
                         => optionsBuilder.ExecutionStrategy(
                                 dependencies => new SqlServerRetryingExecutionStrategy(
                                     dependencies,
                                     options.CurrentValue.MaxRetryCount,
                                     options.CurrentValue.MaxRetryDelay,
                                     options.CurrentValue.ErrorNumbersToAdd))
                             .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

             #endregion ============== SQL-SERVER-STRATEGY-1 ==============

             #region ============== SQL-SERVER-STRATEGY-2 ==============

             //builder
             //.EnableDetailedErrors(true)
             //.EnableSensitiveDataLogging(true)
             //.UseLazyLoadingProxies(true) // => If UseLazyLoadingProxies, all of the navigation fields should be VIRTUAL
             //.UseSqlServer(
             //    connectionString: configuration.GetConnectionString("ConnectionStrings"),
             //        sqlServerOptionsAction: optionsBuilder
             //            => optionsBuilder
             //            .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

             #endregion ============== SQL-SERVER-STRATEGY-2 ==============
         });
     }
    
    // public static void AddPostgreSqlPersistence(this IServiceCollection services)
    // {
    //     services.AddDbContextPool<DbContext, ApplicationDbContext>((provider, builder) =>
    //     {
    //         var configuration = provider.GetRequiredService<IConfiguration>();
    //         var options = provider.GetRequiredService<IOptionsMonitor<PostgreSqlRetryOptions>>();
    //
    //         #region ============== POSTGRESQL-STRATEGY-1 ==============
    //
    //         builder
    //             .EnableDetailedErrors()
    //             .EnableSensitiveDataLogging()
    //             .UseLazyLoadingProxies() // => If UseLazyLoadingProxies, all of the navigation fields should be VIRTUAL
    //             .UseNpgsql(
    //                 configuration.GetConnectionString("PostgreSqlConnection"),
    //                 npgsqlOptionsAction: optionsBuilder =>
    //                     optionsBuilder.ExecutionStrategy(dependencies =>
    //                         new NpgsqlRetryingExecutionStrategy(
    //                             dependencies,
    //                             options.CurrentValue.MaxRetryCount,
    //                             options.CurrentValue.MaxRetryDelay,
    //                             []))
    //                     .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)
    //                     .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)); // Optional: Optimize performance
    //
    //         #endregion ============== POSTGRESQL-STRATEGY-1 ==============
    //
    //         #region ============== POSTGRESQL-STRATEGY-2 ==============
    //
    //         //builder
    //         //.EnableDetailedErrors(true)
    //         //.EnableSensitiveDataLogging(true)
    //         //.UseLazyLoadingProxies(true) // => If UseLazyLoadingProxies, all of the navigation fields should be VIRTUAL
    //         //.UseNpgsql(
    //         //    configuration.GetConnectionString("PostgreSqlConnection"),
    //         //    npgsqlOptionsAction: optionsBuilder
    //         //        => optionsBuilder
    //         //        .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
    //
    //         #endregion ============== POSTGRESQL-STRATEGY-2 ==============
    //     });
    // }

    public static void ConfigureServicesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

        services.AddSingleton<IMongoDbSettings>(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

        services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
    }

    public static void AddRepositoryPersistence(this IServiceCollection services)
    {
        services.AddTransient(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
    }

    public static OptionsBuilder<SqlServerRetryOptions> ConfigureSqlServerRetryOptionsPersistence(
        this IServiceCollection services, IConfigurationSection section)
    {
        return services
            .AddOptions<SqlServerRetryOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
    
    // public static OptionsBuilder<PostgreSqlRetryOptions> ConfigurePostgreSqlRetryOptionsPersistence(
    //     this IServiceCollection services, IConfigurationSection section)
    // {
    //     return services
    //         .AddOptions<PostgreSqlRetryOptions>()
    //         .Bind(section)
    //         .ValidateDataAnnotations()
    //         .ValidateOnStart();
    // }
}