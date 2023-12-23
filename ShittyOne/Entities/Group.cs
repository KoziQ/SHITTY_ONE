using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShittyOne.Entities;

public class Group
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<User> Users { get; set; } = new();
}

public class GroupConfigurations : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasMany(g => g.Users)
            .WithMany(u => u.Groups);
    }
}