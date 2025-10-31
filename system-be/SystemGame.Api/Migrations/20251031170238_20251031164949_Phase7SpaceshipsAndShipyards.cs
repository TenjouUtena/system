using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class _20251031164949_Phase7SpaceshipsAndShipyards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CurrentBehaviorName = table.Column<string>(type: "text", nullable: true),
                    BehaviorConfig = table.Column<string>(type: "text", nullable: true),
                    LastExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentSystemId = table.Column<int>(type: "integer", nullable: true),
                    CurrentPlanetId = table.Column<int>(type: "integer", nullable: true),
                    PositionX = table.Column<double>(type: "double precision", nullable: true),
                    PositionY = table.Column<double>(type: "double precision", nullable: true),
                    BuilderId = table.Column<int>(type: "integer", nullable: true),
                    SpaceshipId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agents_Builders_BuilderId",
                        column: x => x.BuilderId,
                        principalTable: "Builders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Agents_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agents_Planets_CurrentPlanetId",
                        column: x => x.CurrentPlanetId,
                        principalTable: "Planets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Agents_StarSystems_CurrentSystemId",
                        column: x => x.CurrentSystemId,
                        principalTable: "StarSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Shipyards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    SpaceStationId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MaxConcurrentBuilds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipyards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipyards_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shipyards_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shipyards_SpaceStations_SpaceStationId",
                        column: x => x.SpaceStationId,
                        principalTable: "SpaceStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentLogs_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spaceships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    ShipyardId = table.Column<int>(type: "integer", nullable: true),
                    ConstructionProgress = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    ConstructionTimeSeconds = table.Column<int>(type: "integer", nullable: false),
                    ConstructionStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConstructionCompletedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentSystemId = table.Column<int>(type: "integer", nullable: false),
                    PositionX = table.Column<double>(type: "double precision", nullable: false),
                    PositionY = table.Column<double>(type: "double precision", nullable: false),
                    Speed = table.Column<double>(type: "double precision", nullable: false),
                    DestinationSystemId = table.Column<int>(type: "integer", nullable: true),
                    DestinationX = table.Column<double>(type: "double precision", nullable: true),
                    DestinationY = table.Column<double>(type: "double precision", nullable: true),
                    MovementStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CargoIron = table.Column<int>(type: "integer", nullable: true),
                    CargoCopper = table.Column<int>(type: "integer", nullable: true),
                    CargoFuel = table.Column<int>(type: "integer", nullable: true),
                    CargoSoil = table.Column<int>(type: "integer", nullable: true),
                    CargoCapacity = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    MaxHealth = table.Column<int>(type: "integer", nullable: false),
                    Attack = table.Column<int>(type: "integer", nullable: false),
                    Defense = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spaceships", x => x.Id);
                    table.CheckConstraint("CK_Spaceship_ProgressRange", "\"ConstructionProgress\" >= 0 AND \"ConstructionProgress\" <= 100");
                    table.ForeignKey(
                        name: "FK_Spaceships_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Spaceships_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spaceships_Shipyards_ShipyardId",
                        column: x => x.ShipyardId,
                        principalTable: "Shipyards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Spaceships_StarSystems_CurrentSystemId",
                        column: x => x.CurrentSystemId,
                        principalTable: "StarSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Spaceships_StarSystems_DestinationSystemId",
                        column: x => x.DestinationSystemId,
                        principalTable: "StarSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentLogs_AgentId_Timestamp",
                table: "AgentLogs",
                columns: new[] { "AgentId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_BuilderId",
                table: "Agents",
                column: "BuilderId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_CurrentPlanetId",
                table: "Agents",
                column: "CurrentPlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_CurrentSystemId",
                table: "Agents",
                column: "CurrentSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_GameId",
                table: "Agents",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_PlayerId",
                table: "Agents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_State",
                table: "Agents",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Shipyards_GameId",
                table: "Shipyards",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipyards_PlayerId",
                table: "Shipyards",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipyards_SpaceStationId",
                table: "Shipyards",
                column: "SpaceStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Spaceships_CurrentSystemId",
                table: "Spaceships",
                column: "CurrentSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Spaceships_DestinationSystemId",
                table: "Spaceships",
                column: "DestinationSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Spaceships_GameId",
                table: "Spaceships",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Spaceships_PlayerId",
                table: "Spaceships",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Spaceships_ShipyardId",
                table: "Spaceships",
                column: "ShipyardId");

            migrationBuilder.CreateIndex(
                name: "IX_Spaceships_State",
                table: "Spaceships",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentLogs");

            migrationBuilder.DropTable(
                name: "Spaceships");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Shipyards");
        }
    }
}
