using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.CatalogService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugInSizesAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "tag",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "sizes",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_tag_Slug",
                table: "tag",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sizes_Slug",
                table: "sizes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_colors_Slug",
                table: "colors",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tag_Slug",
                table: "tag");

            migrationBuilder.DropIndex(
                name: "IX_sizes_Slug",
                table: "sizes");

            migrationBuilder.DropIndex(
                name: "IX_colors_Slug",
                table: "colors");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "tag");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "sizes");
        }
    }
}
