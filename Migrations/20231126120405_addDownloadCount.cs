using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audiobooks.Migrations
{
    public partial class addDownloadCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Downloads",
                table: "Audiobook",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Downloads",
                table: "Audiobook");
        }
    }
}
