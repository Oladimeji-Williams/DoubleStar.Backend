using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoubleStar.Modules.Identity.Migrations
{
    /// <inheritdoc />
    public partial class SyncIdentityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkTokenHash",
                schema: "identity",
                table: "EmailSignInCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TwoFactorChallenges",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorChallenges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorChallenges_TokenHash",
                schema: "identity",
                table: "TwoFactorChallenges",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TwoFactorChallenges",
                schema: "identity");

            migrationBuilder.DropColumn(
                name: "LinkTokenHash",
                schema: "identity",
                table: "EmailSignInCodes");
        }
    }
}
