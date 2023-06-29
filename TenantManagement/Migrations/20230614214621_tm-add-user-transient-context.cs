using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenantManagement.Migrations
{
    public partial class tmaddusertransientcontext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransientContext",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransientContext",
                table: "Users");
        }
    }
}
