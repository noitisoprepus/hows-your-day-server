using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HowsYourDay.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryDateAndTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EntryDate",
                table: "DayEntries",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "AspNetUsers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "DayEntries"
                SET "EntryDate" = ("LoggedAtUtc" AT TIME ZONE 'UTC')::date
                WHERE "EntryDate" IS NULL;
                """);

            migrationBuilder.Sql("""
                DELETE FROM "DayEntries" d
                USING "DayEntries" d2
                WHERE d."UserId" = d2."UserId"
                  AND d."EntryDate" = d2."EntryDate"
                  AND (d."LoggedAtUtc" < d2."LoggedAtUtc"
                       OR (d."LoggedAtUtc" = d2."LoggedAtUtc" AND d."Id" < d2."Id"));
                """);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EntryDate",
                table: "DayEntries",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DayEntries_UserId_EntryDate",
                table: "DayEntries",
                columns: new[] { "UserId", "EntryDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DayEntries_UserId_EntryDate",
                table: "DayEntries");

            migrationBuilder.DropColumn(
                name: "EntryDate",
                table: "DayEntries");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "AspNetUsers");
        }
    }
}
