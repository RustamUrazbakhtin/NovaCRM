using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCRM.Data.Migrations;

public partial class StoreStaffEnumsAsInts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Staff_OrganizationId_IsActive_EmploymentStatus",
            table: "Staff");

        migrationBuilder.Sql(@"
            ALTER TABLE \"Staff\"
            ALTER COLUMN \"EmploymentStatus\" TYPE integer
            USING CASE
                WHEN \"EmploymentStatus\" ILIKE 'Vacation' OR \"EmploymentStatus\" ILIKE 'OnLeave' THEN 2
                WHEN \"EmploymentStatus\" ILIKE 'Terminated' THEN 3
                ELSE 1
            END;

            ALTER TABLE \"StaffCompensations\"
            ALTER COLUMN \"CompensationType\" TYPE integer
            USING CASE
                WHEN \"CompensationType\" ILIKE 'HourlyRate' OR \"CompensationType\" ILIKE 'Hourly' THEN 2
                WHEN \"CompensationType\" ILIKE 'Commission' THEN 3
                ELSE 1
            END;
        ");

        migrationBuilder.AlterColumn<int>(
            name: "EmploymentStatus",
            table: "Staff",
            type: "integer",
            nullable: false,
            defaultValue: 1,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<int>(
            name: "CompensationType",
            table: "StaffCompensations",
            type: "integer",
            nullable: false,
            defaultValue: 1,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.CreateIndex(
            name: "IX_Staff_OrganizationId_IsActive_EmploymentStatus",
            table: "Staff",
            columns: new[] { "OrganizationId", "IsActive", "EmploymentStatus" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Staff_OrganizationId_IsActive_EmploymentStatus",
            table: "Staff");

        migrationBuilder.Sql(@"
            ALTER TABLE \"Staff\"
            ALTER COLUMN \"EmploymentStatus\" TYPE text
            USING CASE
                WHEN \"EmploymentStatus\" = 2 THEN 'Vacation'
                WHEN \"EmploymentStatus\" = 3 THEN 'Terminated'
                ELSE 'Active'
            END;

            ALTER TABLE \"StaffCompensations\"
            ALTER COLUMN \"CompensationType\" TYPE text
            USING CASE
                WHEN \"CompensationType\" = 2 THEN 'HourlyRate'
                WHEN \"CompensationType\" = 3 THEN 'Commission'
                ELSE 'FixedSalary'
            END;
        ");

        migrationBuilder.AlterColumn<string>(
            name: "EmploymentStatus",
            table: "Staff",
            type: "text",
            nullable: false,
            defaultValue: "Active",
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.AlterColumn<string>(
            name: "CompensationType",
            table: "StaffCompensations",
            type: "text",
            nullable: false,
            defaultValue: "FixedSalary",
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.CreateIndex(
            name: "IX_Staff_OrganizationId_IsActive_EmploymentStatus",
            table: "Staff",
            columns: new[] { "OrganizationId", "IsActive", "EmploymentStatus" });
    }
}
