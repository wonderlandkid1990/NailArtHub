using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NailArtHub.Migrations
{
    /// <inheritdoc />
    public partial class AddCityDistrictToNowApply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Shops",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Shops",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "NewApplies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "NewApplies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "City",
                table: "NewApplies");

            migrationBuilder.DropColumn(
                name: "District",
                table: "NewApplies");
        }
    }
}
