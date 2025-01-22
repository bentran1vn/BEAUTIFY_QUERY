using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Attributes;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Entities;
using BEAUTIFY_QUERY.DOMAIN.Constrants;


namespace BEAUTIFY_QUERY.DOMAIN.Documents;
[BsonCollection(TableNames.Mentor)]
public class MentorProjection : Document
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public int Role { get; set; }
    public int Points { get; set; }
    public int Status { get; set; }
    public bool IsDeleted { get; set; }
    public IEnumerable<SkillProjection> MentorSkills { get; set; } = default!;
}

public class SkillProjection : Document
{
    // public Guid DocumentId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CateogoryType { get; set; }
    public List<CertificateProjection> SkillCetificates { get; set; }
}

public class CertificateProjection : Document
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
}