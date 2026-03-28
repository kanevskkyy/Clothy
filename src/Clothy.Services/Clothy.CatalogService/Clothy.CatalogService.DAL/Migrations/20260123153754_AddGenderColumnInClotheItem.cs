using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.CatalogService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderColumnInClotheItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "clothe_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "clothe_items");
        }
    }
}
