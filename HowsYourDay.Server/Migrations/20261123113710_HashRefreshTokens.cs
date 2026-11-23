using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HowsYourDay.Server.Migrations
{
    /// <inheritdoc />
    public partial class HashRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "AspNetUsers"
                SET "RefreshToken" = NULL, "RefreshTokenExpiry" = TIMESTAMPTZ '1970-01-01 00:00:00+00'
                WHERE "RefreshToken" IS NOT NULL;
                """);

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "AspNetUsers",
                newName: "RefreshTokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "AspNetUsers"
                SET "RefreshTokenHash" = NULL, "RefreshTokenExpiry" = TIMESTAMPTZ '1970-01-01 00:00:00+00'
                WHERE "RefreshTokenHash" IS NOT NULL;
                """);

            migrationBuilder.RenameColumn(
                name: "RefreshTokenHash",
                table: "AspNetUsers",
                newName: "RefreshToken");
        }
    }
}
