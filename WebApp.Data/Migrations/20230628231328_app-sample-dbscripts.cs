using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;
using System.Reflection;

#nullable disable

namespace WebApp.Data.Migrations
{
    public partial class appsampledbscripts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var scripts = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/Migrations/DBScripts/SampleTrigger", "*.sql", SearchOption.AllDirectories);

            foreach (string script in scripts)
            {
                string sql = File.ReadAllText(script);
                migrationBuilder.Sql(sql);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var scripts = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/Migrations/DBScripts/SampleTrigger", "*.sql", SearchOption.AllDirectories);

            foreach (string script in scripts)
            {
                string sql = Path.GetFileNameWithoutExtension(script);
                migrationBuilder.Sql($"DROP TRIGGER {sql}");
            }
        }
    }
}