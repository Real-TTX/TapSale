using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TabSale.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ShiftNamesAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CashShift",
                type: "TEXT",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "CashShift");
        }
    }
}
