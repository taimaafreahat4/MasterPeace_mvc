using Microsoft.EntityFrameworkCore;
using StockMind.Models.Database;

namespace StockMind.Data;

public partial class StockMindDb
{
    public DbSet<RegistrationProfile> RegistrationProfiles => Set<RegistrationProfile>();

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        var profile = modelBuilder.Entity<RegistrationProfile>();
        profile.ToTable("RegistrationProfiles", "dbo");
        profile.HasKey(p => p.UserId);
        profile.Property(p => p.Username).HasMaxLength(40);
        profile.Property(p => p.PhotoType).HasMaxLength(20);
        profile.HasIndex(p => p.Username).IsUnique();
        profile.HasOne(p => p.User).WithOne().HasForeignKey<RegistrationProfile>(p => p.UserId);
    }
}
