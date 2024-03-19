using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACS.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveUsersHostRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveUserNamePattern",
                table: "Targets",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HostRolePattern",
                table: "Targets",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveUserNamePattern",
                table: "Targets");

            migrationBuilder.DropColumn(
                name: "HostRolePattern",
                table: "Targets");
        }
    }
}
