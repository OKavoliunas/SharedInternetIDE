using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorServerApp.Migrations
{
    public partial class AddLanguageToProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Projects",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Projects");
        }
    }
}
