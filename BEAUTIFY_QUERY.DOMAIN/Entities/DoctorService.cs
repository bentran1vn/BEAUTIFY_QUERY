﻿namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class DoctorService : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid DoctorId { get; set; }
    public virtual User? Doctor { get; set; }
    public Guid CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}