using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Data.Migrations
{
    public partial class appinitialschema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaseAttachments",
                columns: table => new
                {
                    AttachmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrincipalId = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(1024)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(1024)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(1024)", nullable: true),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ModifiedDatetime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseAttachments", x => x.AttachmentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseAttachments_AccessToken",
                table: "BaseAttachments",
                column: "AccessToken");

            migrationBuilder.CreateIndex(
                name: "IX_BaseAttachments_Discriminator",
                table: "BaseAttachments",
                column: "Discriminator");

            migrationBuilder.CreateIndex(
                name: "IX_BaseAttachments_Name",
                table: "BaseAttachments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BaseAttachments_PrincipalId",
                table: "BaseAttachments",
                column: "PrincipalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseAttachments");
        }
    }
}
