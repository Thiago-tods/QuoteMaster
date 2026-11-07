using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuoteMaster.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Day4CompanySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    TradeName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    Document = table.Column<string>(type: "TEXT", maxLength: 18, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    LogoPath = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    DefaultValidityDays = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySettings");
        }
    }
}
