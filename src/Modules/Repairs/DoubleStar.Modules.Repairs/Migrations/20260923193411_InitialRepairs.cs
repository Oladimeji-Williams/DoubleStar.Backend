using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DoubleStar.Modules.Repairs.Migrations
{
    /// <inheritdoc />
    public partial class InitialRepairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "repairs");

            migrationBuilder.CreateTable(
                name: "RepairTickets",
                schema: "repairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ImeiOrSerial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FaultDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DiagnosisNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    QuotedPriceKobo = table.Column<long>(type: "bigint", nullable: true),
                    TechnicianUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairTickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepairParts",
                schema: "repairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepairTicketId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitCostKobo = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairParts_RepairTickets_RepairTicketId",
                        column: x => x.RepairTicketId,
                        principalSchema: "repairs",
                        principalTable: "RepairTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairStatusHistory",
                schema: "repairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepairTicketId = table.Column<int>(type: "integer", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepairStatusHistory_RepairTickets_RepairTicketId",
                        column: x => x.RepairTicketId,
                        principalSchema: "repairs",
                        principalTable: "RepairTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairParts_ProductId",
                schema: "repairs",
                table: "RepairParts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairParts_RepairTicketId",
                schema: "repairs",
                table: "RepairParts",
                column: "RepairTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairStatusHistory_RepairTicketId",
                schema: "repairs",
                table: "RepairStatusHistory",
                column: "RepairTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairTickets_CustomerId",
                schema: "repairs",
                table: "RepairTickets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairTickets_Status",
                schema: "repairs",
                table: "RepairTickets",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairParts",
                schema: "repairs");

            migrationBuilder.DropTable(
                name: "RepairStatusHistory",
                schema: "repairs");

            migrationBuilder.DropTable(
                name: "RepairTickets",
                schema: "repairs");
        }
    }
}
