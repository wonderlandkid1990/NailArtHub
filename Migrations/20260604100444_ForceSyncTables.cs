using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NailArtHub.Migrations
{
    /// <inheritdoc />
    public partial class ForceSyncTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {                
            migrationBuilder.CreateTable(
                name: "NewApplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShopName = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerName = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    InstagramUrl = table.Column<string>(type: "TEXT", nullable: false),
                    PinterestUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SelectedTagsString = table.Column<string>(type: "TEXT", nullable: true),
                    ApplyDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewApplies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShopName = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerName = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    InstagramUrl = table.Column<string>(type: "TEXT", nullable: false),
                    PinterestUrl = table.Column<string>(type: "TEXT", nullable: false),
                    IsAgreed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopTagBridges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShopId = table.Column<int>(type: "INTEGER", nullable: false),
                    NailTagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopTagBridges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopTagBridges_NailTags_NailTagId",
                        column: x => x.NailTagId,
                        principalTable: "NailTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopTagBridges_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopTagBridges_NailTagId",
                table: "ShopTagBridges",
                column: "NailTagId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopTagBridges_ShopId",
                table: "ShopTagBridges",
                column: "ShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NailTrend");

            migrationBuilder.DropTable(
                name: "NewApplies");

            migrationBuilder.DropTable(
                name: "ShopTagBridges");

            migrationBuilder.DropTable(
                name: "NailTags");

            migrationBuilder.DropTable(
                name: "Shops");
        }
    }
}
