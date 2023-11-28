using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audiobooks.Migrations
{
    public partial class addMegaFolder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MegaFolder",
                table: "Audiobook",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MegaFolder",
                table: "Audiobook");
        }
    }
}
