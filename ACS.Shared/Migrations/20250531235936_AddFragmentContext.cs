using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACS.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddFragmentContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Context",
                table: "Fragments",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Context",
                table: "Fragments");
        }
    }
}
