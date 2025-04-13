using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class WalletTransaction : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }

    public Guid? ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }


    // Transaction Type: 0 = Deposit, 1 = Withdrawal, 2 = Transfer
    [MaxLength(20)] public required string TransactionType { get; set; }

    // Status: 0 = Pending, 1 = Completed, 2 = Failed, 3 = Cancelled
    [MaxLength(20)] public required string Status { get; set; }

    public bool IsMakeBySystem { get; set; } = true;

    // Reference to the order if this transaction is related to an order
    /*  public Guid? OrderId { get; set; }
      public virtual Order? Order { get; set; }

      public Guid? ClinicTransactionId { get; set; }
      public virtual ClinicTransaction? ClinicTransaction { get; set; }

      public Guid? SystemTransactionId { get; set; }
      public virtual SystemTransaction? SystemTransaction { get; set; }*/

    [MaxLength(255)] public string? Description { get; set; }

    public DateTimeOffset TransactionDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}