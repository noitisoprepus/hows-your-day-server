using HowsYourDay.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HowsYourDay.Server.Data.Configurations;

public class DayEntryAnalysisConfiguration : IEntityTypeConfiguration<DayEntryAnalysis>
{
    public void Configure(EntityTypeBuilder<DayEntryAnalysis> builder)
    {
        builder.ToTable("DayEntryAnalyses");

        builder.HasKey(analysis => analysis.Id);

        builder.Property(analysis => analysis.KeyPhrases)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(analysis => analysis.Entities)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(analysis => analysis.AnalyzedAtUtc)
            .IsRequired();

        builder.HasOne<DayEntry>()
            .WithOne()
            .HasForeignKey<DayEntryAnalysis>(analysis => analysis.DayEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(analysis => analysis.DayEntryId)
            .IsUnique();

        builder.HasIndex(analysis => analysis.UserId);
    }
}
