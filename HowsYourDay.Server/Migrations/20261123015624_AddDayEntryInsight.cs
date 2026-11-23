using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HowsYourDay.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDayEntryInsight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "DayEntryAnalyses",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Neutral");

            migrationBuilder.AddColumn<double>(
                name: "SentimentScore",
                table: "DayEntryAnalyses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "DayEntryAnalyses",
                type: "character varying(280)",
                maxLength: 280,
                nullable: false,
                defaultValue: "No insight is available for this entry.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "DayEntryAnalyses");

            migrationBuilder.DropColumn(
                name: "SentimentScore",
                table: "DayEntryAnalyses");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "DayEntryAnalyses");
        }
    }
}
