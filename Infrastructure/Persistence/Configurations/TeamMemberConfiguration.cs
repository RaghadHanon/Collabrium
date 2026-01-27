using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMemberEntity>
{
    public void Configure(EntityTypeBuilder<TeamMemberEntity> builder)
    {
        builder.HasKey(x => new { x.TeamId, x.UserId });

        builder.HasOne(x => x.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(u => u.TeamMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.JoinedAt).IsRequired();
    }
}
