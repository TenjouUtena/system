using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class Phase3PlanetGrids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanetGrids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanetId = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanetGrids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanetGrids_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GridSquares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanetGridId = table.Column<int>(type: "integer", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    IronAmount = table.Column<double>(type: "double precision", nullable: true),
                    CopperAmount = table.Column<double>(type: "double precision", nullable: true),
                    FuelAmount = table.Column<double>(type: "double precision", nullable: true),
                    SoilAmount = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GridSquares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GridSquares_PlanetGrids_PlanetGridId",
                        column: x => x.PlanetGridId,
                        principalTable: "PlanetGrids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GridSquares_PlanetGridId_X_Y",
                table: "GridSquares",
                columns: new[] { "PlanetGridId", "X", "Y" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanetGrids_PlanetId",
                table: "PlanetGrids",
                column: "PlanetId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GridSquares");

            migrationBuilder.DropTable(
                name: "PlanetGrids");
        }
    }
}
