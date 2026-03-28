using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.CatalogService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMainPhotoUrlInClotheItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainPhotoURL",
                table: "clothe_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainPhotoURL",
                table: "clothe_items",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }
    }
}
