using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.PERSISTENCE;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder) =>
        builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    
}