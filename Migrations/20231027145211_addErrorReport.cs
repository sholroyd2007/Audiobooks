using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audiobooks.Migrations
{
    public partial class addErrorReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Error",
                table: "Audiobook",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ErrorReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AudiobookId = table.Column<int>(type: "int", nullable: true),
                    ErrorStatus = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorReports_Audiobook_AudiobookId",
                        column: x => x.AudiobookId,
                        principalTable: "Audiobook",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorReports_AudiobookId",
                table: "ErrorReports",
                column: "AudiobookId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorReports");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "Audiobook");
        }
    }
}
