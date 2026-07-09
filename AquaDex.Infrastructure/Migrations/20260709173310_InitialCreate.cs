using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaDex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommonNameAz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommonNameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LatinName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HabitatType = table.Column<int>(type: "int", nullable: false),
                    MinSizeCm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxSizeCm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Diet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConservationStatus = table.Column<int>(type: "int", nullable: false),
                    BestBaitTechnique = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LegalSeasonNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Waterbodies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waterbodies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesWaterbodies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpeciesId = table.Column<int>(type: "int", nullable: false),
                    WaterbodyId = table.Column<int>(type: "int", nullable: false),
                    AbundanceRating = table.Column<int>(type: "int", nullable: false),
                    SeasonNotes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesWaterbodies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeciesWaterbodies_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpeciesWaterbodies_Waterbodies_WaterbodyId",
                        column: x => x.WaterbodyId,
                        principalTable: "Waterbodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesWaterbodies_SpeciesId_WaterbodyId",
                table: "SpeciesWaterbodies",
                columns: new[] { "SpeciesId", "WaterbodyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesWaterbodies_WaterbodyId",
                table: "SpeciesWaterbodies",
                column: "WaterbodyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeciesWaterbodies");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropTable(
                name: "Waterbodies");
        }
    }
}
