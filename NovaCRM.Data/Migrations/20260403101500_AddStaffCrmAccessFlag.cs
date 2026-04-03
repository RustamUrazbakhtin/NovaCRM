using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCRM.Data.Migrations;

public partial class AddStaffCrmAccessFlag : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "HasCrmAccess",
            table: "Staff",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "HasCrmAccess",
            table: "Staff");
    }
}
