using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorServerApp.Migrations
{
    public partial class AddPermissionLevelToAspNetUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "PermissionLevel",
                table: "AspNetUsers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermissionLevel",
                table: "AspNetUsers");
        }
    }
}
