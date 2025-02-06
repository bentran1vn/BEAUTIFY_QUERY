using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Services.CommandConverts;
using BEAUTIFY_QUERY.PERSISTENCE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using ILogger = Serilog.ILogger;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.PostgreMigrate;

public class PostgreMigrateEventHandler : ICommandHandler<DomainEvents.PostgreMigrate>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PostgreMigrateEventHandler> _logger;

    public PostgreMigrateEventHandler(ApplicationDbContext dbContext, ILogger<PostgreMigrateEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result> Handle(DomainEvents.PostgreMigrate request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"🔄 Processing {request.Operation} for entity {request.EntityType} with ID: {request.PrimaryKey}");
        
        var entityType = _dbContext.Model.GetEntityTypes()
            .Select(e => e.ClrType)
            .FirstOrDefault(t => t.Name.EndsWith(request.EntityType));
        
        if (entityType == null)
        {
            _logger.LogError($"❌ Unknown entity type: {request.EntityType}");
            return Result.Failure(new Error("500", $"❌ Unknown entity type: {request.EntityType}"));
        }
        var entity = JsonConvert.DeserializeObject(request.Data, entityType);
        if (entity == null)
        {
            _logger.LogError($"❌ Failed to deserialize entity {request.EntityType}");
            return Result.Failure(new Error("500", $"❌ Failed to deserialize entity {request.EntityType}"));
        }
        
        var entitySet = _dbContext.GetType()
            .GetMethod("Set")!
            .MakeGenericMethod(entityType)
            .Invoke(_dbContext, null);
        
        if (entitySet == null)
        {
            _logger.LogError($"❌ Could not retrieve DbSet for entity {request.EntityType}");
            return Result.Failure(new Error("500", $"❌ Could not retrieve DbSet for entity {request.EntityType}"));
        }

        switch (request.Operation)
        {
            case "Created":
                entitySet.GetType().GetMethod("Add")!.Invoke(entitySet, new[] { entity });
                break;

            case "Updated":
                var existing = await _dbContext.FindAsync(entityType, request.PrimaryKey, cancellationToken);
                if (existing != null)
                {
                    _dbContext.Entry(existing).CurrentValues.SetValues(entity);
                }
                else
                {
                    _logger.LogWarning($"⚠️ Entity {request.EntityType} with ID {request.PrimaryKey} not found for update. Creating new entry.");
                    entitySet.GetType().GetMethod("Add")!.Invoke(entitySet, new[] { entity });
                }
                break;

            case "Deleted":
                var toDelete = await _dbContext.FindAsync(entityType, request.PrimaryKey, cancellationToken);
                if (toDelete != null)
                {
                    entitySet.GetType().GetMethod("Remove")!.Invoke(entitySet, new[] { toDelete });
                }
                else
                {
                    _logger.LogWarning($"⚠️ Entity {request.EntityType} with ID {request.PrimaryKey} not found for deletion.");
                }
                break;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"✅ Successfully processed {request.Operation} for {request.EntityType}");

        return Result.Success();
    }
}