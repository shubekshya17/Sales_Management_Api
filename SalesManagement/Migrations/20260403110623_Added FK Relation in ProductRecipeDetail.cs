using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddedFKRelationinProductRecipeDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeDetails_ProductIngredientId",
                table: "ProductRecipeDetails",
                column: "ProductIngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecipeDetails_ProductIngredients_ProductIngredientId",
                table: "ProductRecipeDetails",
                column: "ProductIngredientId",
                principalTable: "ProductIngredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecipeDetails_ProductIngredients_ProductIngredientId",
                table: "ProductRecipeDetails");

            migrationBuilder.DropIndex(
                name: "IX_ProductRecipeDetails_ProductIngredientId",
                table: "ProductRecipeDetails");
        }
    }
}
