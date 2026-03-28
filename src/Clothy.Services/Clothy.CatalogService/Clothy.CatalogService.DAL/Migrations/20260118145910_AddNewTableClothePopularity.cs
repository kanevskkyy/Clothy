using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.CatalogService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTableClothePopularity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clothe_popularity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClotheId = table.Column<Guid>(type: "uuid", nullable: false),
                    SoldCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clothe_popularity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clothe_popularity_clothe_items_ClotheId",
                        column: x => x.ClotheId,
                        principalTable: "clothe_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clothe_popularity_ClotheId",
                table: "clothe_popularity",
                column: "ClotheId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clothe_popularity");
        }
    }
}
