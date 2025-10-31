using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class Phase4BuildingsConstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GridSquareId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    ConstructionProgress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ConstructionStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConstructionCompleteTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                    table.CheckConstraint("CK_Building_ProgressRange", "\"ConstructionProgress\" >= 0 AND \"ConstructionProgress\" <= 100");
                    table.ForeignKey(
                        name: "FK_Buildings_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Buildings_GridSquares_GridSquareId",
                        column: x => x.GridSquareId,
                        principalTable: "GridSquares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpaceStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SystemId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IronAmount = table.Column<double>(type: "double precision", nullable: false),
                    CopperAmount = table.Column<double>(type: "double precision", nullable: false),
                    FuelAmount = table.Column<double>(type: "double precision", nullable: false),
                    SoilAmount = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaceStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaceStations_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpaceStations_StarSystems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "StarSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Builders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanetId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AssignedBuildingId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Builders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Builders_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Builders_Buildings_AssignedBuildingId",
                        column: x => x.AssignedBuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Builders_Planets_PlanetId",
                        column: x => x.PlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Builders_AssignedBuildingId",
                table: "Builders",
                column: "AssignedBuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Builders_PlanetId",
                table: "Builders",
                column: "PlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_Builders_PlayerId",
                table: "Builders",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_GridSquareId",
                table: "Buildings",
                column: "GridSquareId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_PlayerId",
                table: "Buildings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceStations_PlayerId",
                table: "SpaceStations",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceStations_SystemId",
                table: "SpaceStations",
                column: "SystemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Builders");

            migrationBuilder.DropTable(
                name: "SpaceStations");

            migrationBuilder.DropTable(
                name: "Buildings");
        }
    }
}
