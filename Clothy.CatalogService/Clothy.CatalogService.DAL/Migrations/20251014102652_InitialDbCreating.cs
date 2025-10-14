using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.CatalogService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbCreating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhotoURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "colors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    hexcode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_colors", x => x.Id);
                    table.CheckConstraint("CK_Color_HexCode", "\"hexcode\" ~ '^#[0-9A-Fa-f]{6}$'");
                });

            migrationBuilder.CreateTable(
                name: "materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sizes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clothe_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MainPhotoURL = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: true),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clothe_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clothe_items_brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clothe_items_collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clothe_materials",
                columns: table => new
                {
                    ClotheId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clothe_materials", x => new { x.ClotheId, x.MaterialId });
                    table.CheckConstraint("ck_clothe_materials_percetage_valid", "\"percentage\" >= 0 AND \"percentage\" <= 100");
                    table.ForeignKey(
                        name: "FK_clothe_materials_clothe_items_ClotheId",
                        column: x => x.ClotheId,
                        principalTable: "clothe_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clothe_materials_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clothe_tags",
                columns: table => new
                {
                    ClotheId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clothe_tags", x => new { x.ClotheId, x.TagId });
                    table.ForeignKey(
                        name: "FK_clothe_tags_clothe_items_ClotheId",
                        column: x => x.ClotheId,
                        principalTable: "clothe_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clothe_tags_tag_TagId",
                        column: x => x.TagId,
                        principalTable: "tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clothes_stock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClotheId = table.Column<Guid>(type: "uuid", nullable: false),
                    SizeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ColorId = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clothes_stock", x => x.Id);
                    table.CheckConstraint("ck_clothes_stock_quantity_valid", "\"quantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_clothes_stock_clothe_items_ClotheId",
                        column: x => x.ClotheId,
                        principalTable: "clothe_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clothes_stock_colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clothes_stock_sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "photo_clothes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClotheId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_photo_clothes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_photo_clothes_clothe_items_ClotheId",
                        column: x => x.ClotheId,
                        principalTable: "clothe_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_brands_Name",
                table: "brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clothe_items_BrandId",
                table: "clothe_items",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_clothe_items_CollectionId",
                table: "clothe_items",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_clothe_items_Name",
                table: "clothe_items",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_clothe_items_Price",
                table: "clothe_items",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_clothe_items_Slug",
                table: "clothe_items",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clothe_materials_MaterialId",
                table: "clothe_materials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_clothe_tags_ClotheId",
                table: "clothe_tags",
                column: "ClotheId");

            migrationBuilder.CreateIndex(
                name: "IX_clothe_tags_TagId",
                table: "clothe_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_clothes_stock_ClotheId",
                table: "clothes_stock",
                column: "ClotheId");

            migrationBuilder.CreateIndex(
                name: "IX_clothes_stock_ColorId",
                table: "clothes_stock",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_clothes_stock_SizeId",
                table: "clothes_stock",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_collections_Name",
                table: "collections",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_collections_Slug",
                table: "collections",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_colors_hexcode",
                table: "colors",
                column: "hexcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_materials_Name",
                table: "materials",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_materials_Slug",
                table: "materials",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_photo_clothes_ClotheId",
                table: "photo_clothes",
                column: "ClotheId");

            migrationBuilder.CreateIndex(
                name: "IX_sizes_Name",
                table: "sizes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tag_Name",
                table: "tag",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clothe_materials");

            migrationBuilder.DropTable(
                name: "clothe_tags");

            migrationBuilder.DropTable(
                name: "clothes_stock");

            migrationBuilder.DropTable(
                name: "photo_clothes");

            migrationBuilder.DropTable(
                name: "materials");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropTable(
                name: "colors");

            migrationBuilder.DropTable(
                name: "sizes");

            migrationBuilder.DropTable(
                name: "clothe_items");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "collections");
        }
    }
}
