using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuoteMaster.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Day2DomainRulesAndSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuotationNumberSequences",
                columns: table => new
                {
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    LastNumber = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationNumberSequences", x => x.Year);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_Status",
                table: "Quotations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationItems_ProductId",
                table: "QuotationItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItems_Products_ProductId",
                table: "QuotationItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItems_Products_ProductId",
                table: "QuotationItems");

            migrationBuilder.DropTable(
                name: "QuotationNumberSequences");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_Status",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_QuotationItems_ProductId",
                table: "QuotationItems");
        }
    }
}
