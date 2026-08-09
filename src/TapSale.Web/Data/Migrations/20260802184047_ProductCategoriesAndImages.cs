using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TapSale.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductCategoriesAndImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Product",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProductCategoryId",
                table: "Product",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCategory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaleListId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategory_SaleList_SaleListId",
                        column: x => x.SaleListId,
                        principalTable: "SaleList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductCategoryId",
                table: "Product",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_SaleListId_Name",
                table: "ProductCategory",
                columns: new[] { "SaleListId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ProductCategory_ProductCategoryId",
                table: "Product",
                column: "ProductCategoryId",
                principalTable: "ProductCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_ProductCategory_ProductCategoryId",
                table: "Product");

            migrationBuilder.DropTable(
                name: "ProductCategory");

            migrationBuilder.DropIndex(
                name: "IX_Product_ProductCategoryId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProductCategoryId",
                table: "Product");
        }
    }
}
