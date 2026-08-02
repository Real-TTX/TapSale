using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TabSale.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSortableSaleDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SoldDateUnixMilliseconds",
                table: "Sale",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql("""
                UPDATE "Sale"
                SET "SoldDateUnixMilliseconds" = CAST(strftime('%s', "SoldDate") AS INTEGER) * 1000
                WHERE "SoldDateUnixMilliseconds" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoldDateUnixMilliseconds",
                table: "Sale");
        }
    }
}
