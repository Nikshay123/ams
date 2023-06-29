using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using TenantManagement.Common;

#nullable disable

namespace TenantManagement.Migrations
{
    public partial class tmusermanagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(1024)", nullable: false),
                    StreetAddress = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    StreetAddress2 = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    City = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    State = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ModifiedDatetime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(1024)", nullable: false),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ModifiedDatetime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                columns: table => new
                {
                    ScopeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ModifiedDatetime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.ScopeId);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TimezoneInfo = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ModifiedDatetime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContactFirstName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContactLastName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TimezoneInfo = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ModifiedDatetime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Accounts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Userhash = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Salt = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransientAuthToken = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TransientAuthExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Verified = table.Column<bool>(type: "bit", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    LatestLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimezoneInfo = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ModifiedDatetime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountAddress",
                columns: table => new
                {
                    AccountsAccountId = table.Column<int>(type: "int", nullable: false),
                    AddressesAddressId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAddress", x => new { x.AccountsAccountId, x.AddressesAddressId });
                    table.ForeignKey(
                        name: "FK_AccountAddress_Accounts_AccountsAccountId",
                        column: x => x.AccountsAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountAddress_Addresses_AddressesAddressId",
                        column: x => x.AddressesAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountUsers",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserPrimary = table.Column<bool>(type: "bit", nullable: true),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ModifiedDatetime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountUsers", x => new { x.AccountId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AccountUsers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_AccountUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "AddressUser",
                columns: table => new
                {
                    AddressesAddressId = table.Column<int>(type: "int", nullable: false),
                    UsersUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressUser", x => new { x.AddressesAddressId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_AddressUser_Addresses_AddressesAddressId",
                        column: x => x.AddressesAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddressUser_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RolesRoleId = table.Column<int>(type: "int", nullable: false),
                    UsersUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesRoleId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScopeUser",
                columns: table => new
                {
                    ScopesScopeId = table.Column<int>(type: "int", nullable: false),
                    UsersUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScopeUser", x => new { x.ScopesScopeId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_ScopeUser_Scopes_ScopesScopeId",
                        column: x => x.ScopesScopeId,
                        principalTable: "Scopes",
                        principalColumn: "ScopeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScopeUser_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountUserRole",
                columns: table => new
                {
                    RolesRoleId = table.Column<int>(type: "int", nullable: false),
                    AccountUsersAccountId = table.Column<int>(type: "int", nullable: false),
                    AccountUsersUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountUserRole", x => new { x.RolesRoleId, x.AccountUsersAccountId, x.AccountUsersUserId });
                    table.ForeignKey(
                        name: "FK_AccountUserRole_AccountUsers_AccountUsersAccountId_AccountUsersUserId",
                        columns: x => new { x.AccountUsersAccountId, x.AccountUsersUserId },
                        principalTable: "AccountUsers",
                        principalColumns: new[] { "AccountId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountUserRole_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAddress_AddressesAddressId",
                table: "AccountAddress",
                column: "AddressesAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId_Name",
                table: "Accounts",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountUserRole_AccountUsersAccountId_AccountUsersUserId",
                table: "AccountUserRole",
                columns: new[] { "AccountUsersAccountId", "AccountUsersUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountUsers_UserId",
                table: "AccountUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressUser_UsersUserId",
                table: "AddressUser",
                column: "UsersUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersUserId",
                table: "RoleUser",
                column: "UsersUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Scopes_Name",
                table: "Scopes",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScopeUser_UsersUserId",
                table: "ScopeUser",
                column: "UsersUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RefreshToken",
                table: "Users",
                column: "RefreshToken");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TransientAuthToken",
                table: "Users",
                column: "TransientAuthToken");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Userhash",
                table: "Users",
                column: "Userhash");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");

            CreateDefaultAdmin(migrationBuilder);
        }

        protected void CreateDefaultAdmin(MigrationBuilder migrationBuilder)
        {
            //Debugger.Launch();
            var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            var dir = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

            var config = new ConfigurationBuilder()
                                    .SetBasePath(dir)
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                    .Build();

            //Seeding Global Admin
            var roottenant = Guid.Empty.ToString();
            var appadminuser = config.GetSection("AppAdmin").GetValue(typeof(string), "Username").ToString();
            var adminpass = Environment.GetEnvironmentVariable("AppAdmin__Userauth") ?? config.GetSection("AppAdmin").GetValue(typeof(string), "Userauth").ToString();
            var appadminsalt = Guid.NewGuid().ToString();

            var tenant = config.GetValue(typeof(string), "DefaultTenant").ToString();
            var tenantadmin = $"admin@{tenant}.org";
            var tenantsalt = Guid.NewGuid().ToString();

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantId", "Name" },
                values: new object[] { roottenant, "Root Tenant" });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantId", "Name" },
                values: new object[] { tenant, "WebApp Tenant" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 1, "AppAdmin" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 2, "Admin" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 3, "Manager" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 4, "User" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 5, "AccountOwner" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 6, "AccountAdmin" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 7, "AccountManager" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 8, "AccountUser" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 9, "AccountStakeholder" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "TenantId", "Username", "Userhash", "Salt" },
                values: new object[] { 1, roottenant, appadminuser, CryptoUtils.GenerateHashV2($"{appadminuser}:{adminpass}:{appadminsalt}"), appadminsalt });

            migrationBuilder.InsertData(
                table: "RoleUser",
                columns: new[] { "RolesRoleId", "UsersUserId" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "TenantId", "Username", "Userhash", "Salt" },
                values: new object[] { 2, tenant, tenantadmin, CryptoUtils.GenerateHashV2($"{tenantadmin}:{adminpass}:{tenantsalt}"), tenantsalt });

            migrationBuilder.InsertData(
                table: "RoleUser",
                columns: new[] { "RolesRoleId", "UsersUserId" },
                values: new object[] { 2, 2 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountAddress");

            migrationBuilder.DropTable(
                name: "AccountUserRole");

            migrationBuilder.DropTable(
                name: "AddressUser");

            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropTable(
                name: "ScopeUser");

            migrationBuilder.DropTable(
                name: "AccountUsers");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Scopes");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}