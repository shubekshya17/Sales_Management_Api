using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Invoice = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Party = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Gross = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSale = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Vat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TRNUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TRNTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    STax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Pax = table.Column<int>(type: "int", nullable: true),
                    BillToPan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BillToMob = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Cash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditCard = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Online = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GVoucher = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesReturnVoucher = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Complimentary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrderMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TRNDATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BSDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VCHRNO = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    REFNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Desca = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BILLQTY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BaseUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BaseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSale = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Taxable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonTaxable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Vat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmnt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TRNUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TRNTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Division = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Salesman = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Terminal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDetail", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_SalesCollection_Date_Invoice",
                table: "SalesCollections",
                columns: new[] { "Date", "Invoice" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SalesDetail_TRNDATE_VCHRNO",
                table: "SalesDetail",
                columns: new[] { "TRNDATE", "VCHRNO" },
                unique: true,
                filter: "[VCHRNO] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesCollections");

            migrationBuilder.DropTable(
                name: "SalesDetail");
        }
    }
}
