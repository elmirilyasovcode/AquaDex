using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaDex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForumThreadSpeciesTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThreadId = table.Column<int>(type: "int", nullable: false),
                    SpeciesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumThreadSpeciesTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumThreadSpeciesTags_ForumThreads_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "ForumThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForumThreadSpeciesTags_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForumThreadWaterbodyTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThreadId = table.Column<int>(type: "int", nullable: false),
                    WaterbodyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumThreadWaterbodyTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumThreadWaterbodyTags_ForumThreads_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "ForumThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForumThreadWaterbodyTags_Waterbodies_WaterbodyId",
                        column: x => x.WaterbodyId,
                        principalTable: "Waterbodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointsTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForumThreadSpeciesTags_SpeciesId",
                table: "ForumThreadSpeciesTags",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumThreadSpeciesTags_ThreadId_SpeciesId",
                table: "ForumThreadSpeciesTags",
                columns: new[] { "ThreadId", "SpeciesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumThreadWaterbodyTags_ThreadId_WaterbodyId",
                table: "ForumThreadWaterbodyTags",
                columns: new[] { "ThreadId", "WaterbodyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumThreadWaterbodyTags_WaterbodyId",
                table: "ForumThreadWaterbodyTags",
                column: "WaterbodyId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_UserId",
                table: "PointsTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForumThreadSpeciesTags");

            migrationBuilder.DropTable(
                name: "ForumThreadWaterbodyTags");

            migrationBuilder.DropTable(
                name: "PointsTransactions");
        }
    }
}
