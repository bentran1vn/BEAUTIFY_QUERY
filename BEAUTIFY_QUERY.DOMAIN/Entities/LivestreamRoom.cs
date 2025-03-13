﻿using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_QUERY.DOMAIN.Entities;
public class LivestreamRoom : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(100)] public required string Name { get; set; }
    public TimeOnly? StartDate { get; set; }
    public TimeOnly? EndDate { get; set; }
    [MaxLength(50)] public string? Status { get; set; }
    public DateOnly? Date { get; set; }
    [MaxLength(50)] public string? Type { get; set; }
    public int? Duration { get; set; }
    public int? TotalViewers { get; set; }
    public Guid? ClinicId { get; set; }
    public virtual Clinic Clinic { get; set; }
    public virtual ICollection<Promotion>? Promotions { get; set; }


    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}