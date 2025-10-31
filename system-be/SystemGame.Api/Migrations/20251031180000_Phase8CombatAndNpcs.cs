using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class Phase8CombatAndNpcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Battles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    SystemId = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    PositionX = table.Column<double>(type: "double precision", nullable: false),
                    PositionY = table.Column<double>(type: "double precision", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RoundsElapsed = table.Column<int>(type: "integer", nullable: false),
                    WinnerPlayerId = table.Column<string>(type: "text", nullable: true),
                    EndReason = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Battles_AspNetUsers_WinnerPlayerId",
                        column: x => x.WinnerPlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Battles_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Battles_StarSystems_SystemId",
                        column: x => x.SystemId,
                        principalTable: "StarSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NpcShips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SpaceshipId = table.Column<int>(type: "integer", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    BehaviorType = table.Column<int>(type: "integer", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "integer", nullable: false),
                    PatrolTargetX = table.Column<double>(type: "double precision", nullable: true),
                    PatrolTargetY = table.Column<double>(type: "double precision", nullable: true),
                    TargetShipId = table.Column<int>(type: "integer", nullable: true),
                    LastBehaviorUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SpawnTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SpawnSystemId = table.Column<int>(type: "integer", nullable: true),
                    LootIronMin = table.Column<int>(type: "integer", nullable: false),
                    LootIronMax = table.Column<int>(type: "integer", nullable: false),
                    LootCopperMin = table.Column<int>(type: "integer", nullable: false),
                    LootCopperMax = table.Column<int>(type: "integer", nullable: false),
                    LootFuelMin = table.Column<int>(type: "integer", nullable: false),
                    LootFuelMax = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcShips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NpcShips_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NpcShips_Spaceships_SpaceshipId",
                        column: x => x.SpaceshipId,
                        principalTable: "Spaceships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NpcShips_Spaceships_TargetShipId",
                        column: x => x.TargetShipId,
                        principalTable: "Spaceships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NpcShips_StarSystems_SpawnSystemId",
                        column: x => x.SpawnSystemId,
                        principalTable: "StarSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BattleParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BattleId = table.Column<int>(type: "integer", nullable: false),
                    SpaceshipId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<string>(type: "text", nullable: false),
                    IsNpc = table.Column<bool>(type: "boolean", nullable: false),
                    InitialHealth = table.Column<int>(type: "integer", nullable: false),
                    FinalHealth = table.Column<int>(type: "integer", nullable: false),
                    Attack = table.Column<int>(type: "integer", nullable: false),
                    Defense = table.Column<int>(type: "integer", nullable: false),
                    DamageDealt = table.Column<int>(type: "integer", nullable: false),
                    DamageTaken = table.Column<int>(type: "integer", nullable: false),
                    Survived = table.Column<bool>(type: "boolean", nullable: false),
                    Fled = table.Column<bool>(type: "boolean", nullable: false),
                    ExperienceGained = table.Column<int>(type: "integer", nullable: false),
                    LootIron = table.Column<int>(type: "integer", nullable: true),
                    LootCopper = table.Column<int>(type: "integer", nullable: true),
                    LootFuel = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleParticipants_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BattleParticipants_Battles_BattleId",
                        column: x => x.BattleId,
                        principalTable: "Battles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattleParticipants_Spaceships_SpaceshipId",
                        column: x => x.SpaceshipId,
                        principalTable: "Spaceships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BattleEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BattleId = table.Column<int>(type: "integer", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AttackerShipId = table.Column<int>(type: "integer", nullable: true),
                    DefenderShipId = table.Column<int>(type: "integer", nullable: true),
                    DamageDealt = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleEvents_Battles_BattleId",
                        column: x => x.BattleId,
                        principalTable: "Battles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattleEvents_Spaceships_AttackerShipId",
                        column: x => x.AttackerShipId,
                        principalTable: "Spaceships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BattleEvents_Spaceships_DefenderShipId",
                        column: x => x.DefenderShipId,
                        principalTable: "Spaceships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BattleEvents_AttackerShipId",
                table: "BattleEvents",
                column: "AttackerShipId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleEvents_BattleId_Round",
                table: "BattleEvents",
                columns: new[] { "BattleId", "Round" });

            migrationBuilder.CreateIndex(
                name: "IX_BattleEvents_DefenderShipId",
                table: "BattleEvents",
                column: "DefenderShipId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleParticipants_BattleId",
                table: "BattleParticipants",
                column: "BattleId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleParticipants_PlayerId",
                table: "BattleParticipants",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleParticipants_SpaceshipId",
                table: "BattleParticipants",
                column: "SpaceshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_GameId",
                table: "Battles",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_State",
                table: "Battles",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_SystemId",
                table: "Battles",
                column: "SystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_WinnerPlayerId",
                table: "Battles",
                column: "WinnerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcShips_GameId",
                table: "NpcShips",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcShips_SpaceshipId",
                table: "NpcShips",
                column: "SpaceshipId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NpcShips_SpawnSystemId",
                table: "NpcShips",
                column: "SpawnSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_NpcShips_TargetShipId",
                table: "NpcShips",
                column: "TargetShipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattleEvents");

            migrationBuilder.DropTable(
                name: "BattleParticipants");

            migrationBuilder.DropTable(
                name: "NpcShips");

            migrationBuilder.DropTable(
                name: "Battles");
        }
    }
}
