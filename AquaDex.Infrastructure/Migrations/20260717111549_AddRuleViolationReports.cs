using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaDex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRuleViolationReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RuleViolationReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ViolationType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaterbodyId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleViolationReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleViolationReports_AspNetUsers_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RuleViolationReports_Waterbodies_WaterbodyId",
                        column: x => x.WaterbodyId,
                        principalTable: "Waterbodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RuleViolationReports_ReportedByUserId",
                table: "RuleViolationReports",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleViolationReports_WaterbodyId",
                table: "RuleViolationReports",
                column: "WaterbodyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuleViolationReports");
        }
    }
}
