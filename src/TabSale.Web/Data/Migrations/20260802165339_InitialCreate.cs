using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TabSale.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUser",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSession",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Token = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    LastSeenDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastSyncDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RevokedDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSession_AppUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashShift",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Token = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SaleListId = table.Column<long>(type: "INTEGER", nullable: false),
                    OpeningCents = table.Column<long>(type: "INTEGER", nullable: false),
                    CountedClosingCents = table.Column<long>(type: "INTEGER", nullable: true),
                    OpenedDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ClosedDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashShift", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashShift_AppUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashShift_SaleList_SaleListId",
                        column: x => x.SaleListId,
                        principalTable: "SaleList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaleListId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    UnitPriceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_SaleList_SaleListId",
                        column: x => x.SaleListId,
                        principalTable: "SaleList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSaleList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SaleListId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSaleList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSaleList_AppUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSaleList_SaleList_SaleListId",
                        column: x => x.SaleListId,
                        principalTable: "SaleList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Token = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceToken = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SaleListId = table.Column<long>(type: "INTEGER", nullable: false),
                    CashShiftId = table.Column<long>(type: "INTEGER", nullable: true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalSaleId = table.Column<long>(type: "INTEGER", nullable: true),
                    SoldDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    TotalCents = table.Column<long>(type: "INTEGER", nullable: false),
                    TenderedCents = table.Column<long>(type: "INTEGER", nullable: true),
                    ChangeCents = table.Column<long>(type: "INTEGER", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sale_AppUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sale_CashShift_CashShiftId",
                        column: x => x.CashShiftId,
                        principalTable: "CashShift",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sale_SaleList_SaleListId",
                        column: x => x.SaleListId,
                        principalTable: "SaleList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sale_Sale_OriginalSaleId",
                        column: x => x.OriginalSaleId,
                        principalTable: "Sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductPriceVersion",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<long>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    UnitPriceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPriceVersion_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaleId = table.Column<long>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<long>(type: "INTEGER", nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    ProductKind = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPriceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    LineTotalCents = table.Column<long>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreateUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdateUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleLine_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SaleLine_Sale_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUser_UserName",
                table: "AppUser",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashShift_SaleListId",
                table: "CashShift",
                column: "SaleListId");

            migrationBuilder.CreateIndex(
                name: "IX_CashShift_Token",
                table: "CashShift",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashShift_UserId",
                table: "CashShift",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_SaleListId",
                table: "Product",
                column: "SaleListId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceVersion_ProductId_Version",
                table: "ProductPriceVersion",
                columns: new[] { "ProductId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sale_CashShiftId",
                table: "Sale",
                column: "CashShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_OriginalSaleId",
                table: "Sale",
                column: "OriginalSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_SaleListId",
                table: "Sale",
                column: "SaleListId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_Token",
                table: "Sale",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sale_UserId",
                table: "Sale",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleLine_ProductId",
                table: "SaleLine",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleLine_SaleId",
                table: "SaleLine",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSaleList_SaleListId",
                table: "UserSaleList",
                column: "SaleListId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSaleList_UserId_SaleListId",
                table: "UserSaleList",
                columns: new[] { "UserId", "SaleListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_Token",
                table: "UserSession",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_UserId",
                table: "UserSession",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPriceVersion");

            migrationBuilder.DropTable(
                name: "SaleLine");

            migrationBuilder.DropTable(
                name: "UserSaleList");

            migrationBuilder.DropTable(
                name: "UserSession");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Sale");

            migrationBuilder.DropTable(
                name: "CashShift");

            migrationBuilder.DropTable(
                name: "AppUser");

            migrationBuilder.DropTable(
                name: "SaleList");
        }
    }
}
