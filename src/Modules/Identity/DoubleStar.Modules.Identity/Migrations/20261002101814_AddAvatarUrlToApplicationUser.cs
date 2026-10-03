using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoubleStar.Modules.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarUrlToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                schema: "identity",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                schema: "identity",
                table: "AspNetUsers");
        }
    }
}
