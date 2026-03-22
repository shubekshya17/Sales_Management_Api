using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddedItemCodeForHashset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_SalesDetail_TRNDATE_VCHRNO",
                table: "SalesDetail");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "SalesDetail",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SalesDetail_TRNDATE_VCHRNO_ITEMCODE",
                table: "SalesDetail",
                columns: new[] { "TRNDATE", "VCHRNO", "ItemCode" },
                unique: true,
                filter: "[VCHRNO] IS NOT NULL AND [ItemCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_SalesDetail_TRNDATE_VCHRNO_ITEMCODE",
                table: "SalesDetail");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                table: "SalesDetail",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SalesDetail_TRNDATE_VCHRNO",
                table: "SalesDetail",
                columns: new[] { "TRNDATE", "VCHRNO" },
                unique: true,
                filter: "[VCHRNO] IS NOT NULL");
        }
    }
}
