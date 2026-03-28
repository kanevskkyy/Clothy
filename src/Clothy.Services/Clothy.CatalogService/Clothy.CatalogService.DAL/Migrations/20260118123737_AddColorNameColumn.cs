using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.CatalogService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddColorNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "colors",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_colors_Name",
                table: "colors",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_colors_Name",
                table: "colors");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "colors");
        }
    }
}
