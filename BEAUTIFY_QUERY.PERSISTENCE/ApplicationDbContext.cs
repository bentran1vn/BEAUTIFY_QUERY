using System.Reflection;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Entities;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.PERSISTENCE;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public virtual DbSet<Survey> Surveys { get; set; }
    public virtual DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public virtual DbSet<SurveyAnswer> SurveyAnswers { get; set; }
    public virtual DbSet<SurveyResponse> SurveyResponses { get; set; }
    public virtual DbSet<ClinicTransaction> ClinicTransactions { get; set; }
    public virtual DbSet<WalletTransaction> WalletTransaction { get; set; }
    public virtual DbSet<Procedure> Procedures { get; set; }
    public virtual DbSet<ProcedurePriceTypes> ProcedurePriceTypes { get; set; }
    public virtual DbSet<Config> Configs { get; set; }

    private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : Entity<T>
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Order>().HasQueryFilter(x => !x.IsDeleted);
        builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

        builder.Entity<CustomerSchedule>()
            .HasOne(cs => cs.Customer)
            .WithMany(u => u.CustomerSchedules)
            .HasForeignKey(cs => cs.CustomerId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade delete
        builder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique()
            .HasAnnotation("SqlServer:CaseSensitive", true);
        builder.Entity<User>()
            .HasIndex(x => x.PhoneNumber)
            .IsUnique();

        builder.Entity<SubscriptionPackage>()
            .HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<DoctorCertificate>()
            .HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<WalletTransaction>()
            .HasQueryFilter(x => !x.IsDeleted);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.BaseType is not { IsGenericType: true } ||
                entityType.ClrType.BaseType.GetGenericTypeDefinition() != typeof(Entity<>)) continue;
            var method = typeof(ApplicationDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
                ?.MakeGenericMethod(entityType.ClrType);

            method?.Invoke(null, [builder]);
        }

        builder.Entity<CustomerSchedule>().HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Order>()
            .HasOne(lr => lr.OrderFeedback)
            .WithOne() // Assuming one-to-one relationship, adjust if it's one-to-many
            .HasForeignKey<Order>(lr => lr.OrderFeedbackId)
            .OnDelete(DeleteBehavior.Restrict); // Adjust delete behavior as needed

        builder.Entity<CustomerSchedule>()
            .HasOne(lr => lr.Feedback)
            .WithOne() // Assuming one-to-one relationship, adjust if it's one-to-many
            .HasForeignKey<CustomerSchedule>(lr => lr.FeedbackId)
            .OnDelete(DeleteBehavior.Restrict); // Adjust delete behavior as needed

        // builder.Entity<Procedure>()
        //     .HasMany(lr => lr.ProcedurePriceTypes)
        //     .WithOne() // Assuming one-to-one relationship, adjust if it's one-to-many
        //     .HasForeignKey(lr => lr.ProcedureId)
        //     .OnDelete(DeleteBehavior.Restrict); // Adjust delete behavior as needed

        builder.Entity<ProcedurePriceTypes>()
            .HasOne(p => p.Procedure)
            .WithMany(p => p.ProcedurePriceTypes)
            .HasForeignKey(p => p.ProcedureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}