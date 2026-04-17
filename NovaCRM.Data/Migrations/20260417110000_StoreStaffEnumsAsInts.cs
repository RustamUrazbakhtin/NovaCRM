using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCRM.Data.Migrations;

public partial class StoreStaffEnumsAsInts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE \"Staff\" ADD COLUMN IF NOT EXISTS \"EmploymentStatusInt\" integer NOT NULL DEFAULT 1;
            UPDATE \"Staff\"
            SET \"EmploymentStatusInt\" = CASE
                WHEN \"EmploymentStatus\" ILIKE 'Vacation' OR \"EmploymentStatus\" ILIKE 'OnLeave' THEN 2
                WHEN \"EmploymentStatus\" ILIKE 'Terminated' THEN 3
                ELSE 1
            END;
            ALTER TABLE \"Staff\" DROP COLUMN \"EmploymentStatus\";
            ALTER TABLE \"Staff\" RENAME COLUMN \"EmploymentStatusInt\" TO \"EmploymentStatus\";

            ALTER TABLE \"StaffCompensations\" ADD COLUMN IF NOT EXISTS \"CompensationTypeInt\" integer NOT NULL DEFAULT 1;
            UPDATE \"StaffCompensations\"
            SET \"CompensationTypeInt\" = CASE
                WHEN \"CompensationType\" ILIKE 'HourlyRate' OR \"CompensationType\" ILIKE 'Hourly' THEN 2
                WHEN \"CompensationType\" ILIKE 'Commission' THEN 3
                ELSE 1
            END;
            ALTER TABLE \"StaffCompensations\" DROP COLUMN \"CompensationType\";
            ALTER TABLE \"StaffCompensations\" RENAME COLUMN \"CompensationTypeInt\" TO \"CompensationType\";
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE \"Staff\" ADD COLUMN IF NOT EXISTS \"EmploymentStatusText\" text NOT NULL DEFAULT 'Active';
            UPDATE \"Staff\"
            SET \"EmploymentStatusText\" = CASE
                WHEN \"EmploymentStatus\" = 2 THEN 'Vacation'
                WHEN \"EmploymentStatus\" = 3 THEN 'Terminated'
                ELSE 'Active'
            END;
            ALTER TABLE \"Staff\" DROP COLUMN \"EmploymentStatus\";
            ALTER TABLE \"Staff\" RENAME COLUMN \"EmploymentStatusText\" TO \"EmploymentStatus\";

            ALTER TABLE \"StaffCompensations\" ADD COLUMN IF NOT EXISTS \"CompensationTypeText\" text NOT NULL DEFAULT 'FixedSalary';
            UPDATE \"StaffCompensations\"
            SET \"CompensationTypeText\" = CASE
                WHEN \"CompensationType\" = 2 THEN 'HourlyRate'
                WHEN \"CompensationType\" = 3 THEN 'Commission'
                ELSE 'FixedSalary'
            END;
            ALTER TABLE \"StaffCompensations\" DROP COLUMN \"CompensationType\";
            ALTER TABLE \"StaffCompensations\" RENAME COLUMN \"CompensationTypeText\" TO \"CompensationType\";
        ");
    }
}
