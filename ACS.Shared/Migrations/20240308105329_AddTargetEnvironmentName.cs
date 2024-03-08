using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACS.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetEnvironmentName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnvironmentNamePattern",
                table: "Targets",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnvironmentNamePattern",
                table: "Targets");
        }
    }
}
