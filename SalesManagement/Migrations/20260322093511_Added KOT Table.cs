using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddedKOTTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KOT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KOTNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KOTTIME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TABLENO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WAITER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WAITERNAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ITEMCODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ITEMDESCRIPTION = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QUANTITY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UNIT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    REMARKS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BILLED = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BILLNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TRANSFERKOT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MERGEKOT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SPLITKOT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CANCELBY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CANCELREMARKS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FLG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISBARITEM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KOTID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KOT", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KOT");
        }
    }
}
