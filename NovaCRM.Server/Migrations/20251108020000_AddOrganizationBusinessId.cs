using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCRM.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationBusinessId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessId",
                table: "Organizations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Organizations");
        }
    }
}
