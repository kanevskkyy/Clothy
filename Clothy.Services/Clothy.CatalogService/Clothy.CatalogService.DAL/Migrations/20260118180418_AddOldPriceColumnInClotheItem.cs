using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.CatalogService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOldPriceColumnInClotheItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OldPrice",
                table: "clothe_items",
                type: "numeric(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldPrice",
                table: "clothe_items");
        }
    }
}
