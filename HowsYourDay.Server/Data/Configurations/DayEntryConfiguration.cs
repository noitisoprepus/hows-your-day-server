using HowsYourDay.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HowsYourDay.Server.Data.Configurations;

public class DayEntryConfiguration : IEntityTypeConfiguration<DayEntry>
{
    public void Configure(EntityTypeBuilder<DayEntry> builder)
    {
        builder.ToTable("DayEntries");

        builder.HasKey(day => day.Id);

        builder.Property(day => day.Rating)
            .IsRequired();

        builder.ToTable(table => table.HasCheckConstraint(
            "CK_DayEntries_Rating",
            "\"Rating\" >= 1 AND \"Rating\" <= 10"));

        builder.Property(day => day.Note)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(day => day.LoggedAtUtc)
            .IsRequired();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(day => day.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(day => new { day.UserId, day.LoggedAtUtc });
    }
}
