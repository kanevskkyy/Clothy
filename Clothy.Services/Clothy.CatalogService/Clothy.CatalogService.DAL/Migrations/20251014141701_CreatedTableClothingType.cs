using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.CatalogService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CreatedTableClothingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clothes_stock_ClotheId",
                table: "clothes_stock");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "sizes",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "ClothingTypeId",
                table: "clothe_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClothingTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClothingTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clothes_stock_ClotheId_SizeId_ColorId",
                table: "clothes_stock",
                columns: new[] { "ClotheId", "SizeId", "ColorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clothe_items_ClothingTypeId",
                table: "clothe_items",
                column: "ClothingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_clothe_items_ClothingTypes_ClothingTypeId",
                table: "clothe_items",
                column: "ClothingTypeId",
                principalTable: "ClothingTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_clothe_items_ClothingTypes_ClothingTypeId",
                table: "clothe_items");

            migrationBuilder.DropTable(
                name: "ClothingTypes");

            migrationBuilder.DropIndex(
                name: "IX_clothes_stock_ClotheId_SizeId_ColorId",
                table: "clothes_stock");

            migrationBuilder.DropIndex(
                name: "IX_clothe_items_ClothingTypeId",
                table: "clothe_items");

            migrationBuilder.DropColumn(
                name: "ClothingTypeId",
                table: "clothe_items");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "sizes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_clothes_stock_ClotheId",
                table: "clothes_stock",
                column: "ClotheId");
        }
    }
}
