using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShittyOne.Entities
{
    public class UserRefresh
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Date { get; private set; } = DateTime.Now;
    }

    public class UserRefreshConfiguration : IEntityTypeConfiguration<UserRefresh>
    {
        public void Configure(EntityTypeBuilder<UserRefresh> builder)
        {
            builder.HasOne(r => r.User)
                .WithMany(u => u.Refreshes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        }
    }
}
