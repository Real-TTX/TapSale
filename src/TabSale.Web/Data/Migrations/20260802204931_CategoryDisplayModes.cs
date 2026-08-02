using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TabSale.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CategoryDisplayModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryDisplayMode",
                table: "SaleList",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryDisplayMode",
                table: "SaleList");
        }
    }
}
