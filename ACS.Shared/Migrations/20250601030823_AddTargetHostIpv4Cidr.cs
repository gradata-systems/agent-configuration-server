using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACS.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetHostIpv4Cidr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HostIpv4Cidr",
                table: "Targets",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostIpv4Cidr",
                table: "Targets");
        }
    }
}
