using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCRM.Data.Migrations;

public partial class AddStaffModuleMvp : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "EmploymentStatus",
            table: "Staff",
            type: "text",
            nullable: false,
            defaultValue: "Available");

        migrationBuilder.AddColumn<decimal>(
            name: "RatingAverage",
            table: "Staff",
            type: "numeric(4,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<int>(
            name: "RatingCount",
            table: "Staff",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "StaffCompensations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                CompensationType = table.Column<string>(type: "text", nullable: false),
                FixedSalary = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                HourlyRate = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                CommissionPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                PerServiceAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Notes = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StaffCompensations", x => x.Id);
                table.ForeignKey(
                    name: "FK_StaffCompensations_Staff_StaffId",
                    column: x => x.StaffId,
                    principalTable: "Staff",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StaffCompensationHistories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                PreviousSnapshot = table.Column<string>(type: "text", nullable: true),
                NewSnapshot = table.Column<string>(type: "text", nullable: false),
                ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                ChangedBy = table.Column<string>(type: "text", nullable: true),
                Notes = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StaffCompensationHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_StaffCompensationHistories_Staff_StaffId",
                    column: x => x.StaffId,
                    principalTable: "Staff",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StaffRoles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                IsSystem = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StaffRoles", x => x.Id);
                table.ForeignKey(
                    name: "FK_StaffRoles_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StaffSpecializations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Code = table.Column<string>(type: "text", nullable: false),
                Category = table.Column<string>(type: "text", nullable: true),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StaffSpecializations", x => x.Id);
                table.ForeignKey(
                    name: "FK_StaffSpecializations_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StaffRoleLinks",
            columns: table => new
            {
                StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StaffRoleLinks", x => new { x.StaffId, x.RoleId });
                table.ForeignKey(
                    name: "FK_StaffRoleLinks_StaffRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "StaffRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_StaffRoleLinks_Staff_StaffId",
                    column: x => x.StaffId,
                    principalTable: "Staff",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StaffSpecializationLinks",
            columns: table => new
            {
                StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                SpecializationId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StaffSpecializationLinks", x => new { x.StaffId, x.SpecializationId });
                table.ForeignKey(
                    name: "FK_StaffSpecializationLinks_StaffSpecializations_SpecializationId",
                    column: x => x.SpecializationId,
                    principalTable: "StaffSpecializations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_StaffSpecializationLinks_Staff_StaffId",
                    column: x => x.StaffId,
                    principalTable: "Staff",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Staff_OrganizationId_IsActive_EmploymentStatus", table: "Staff", columns: new[] { "OrganizationId", "IsActive", "EmploymentStatus" });
        migrationBuilder.CreateIndex(name: "IX_StaffCompensationHistories_StaffId", table: "StaffCompensationHistories", column: "StaffId");
        migrationBuilder.CreateIndex(name: "IX_StaffCompensations_StaffId_EffectiveFrom", table: "StaffCompensations", columns: new[] { "StaffId", "EffectiveFrom" });
        migrationBuilder.CreateIndex(name: "IX_StaffRoleLinks_RoleId", table: "StaffRoleLinks", column: "RoleId");
        migrationBuilder.CreateIndex(name: "IX_StaffRoles_OrganizationId_Code", table: "StaffRoles", columns: new[] { "OrganizationId", "Code" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_StaffSpecializationLinks_SpecializationId", table: "StaffSpecializationLinks", column: "SpecializationId");
        migrationBuilder.CreateIndex(name: "IX_StaffSpecializations_OrganizationId_Code", table: "StaffSpecializations", columns: new[] { "OrganizationId", "Code" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "StaffCompensationHistories");
        migrationBuilder.DropTable(name: "StaffCompensations");
        migrationBuilder.DropTable(name: "StaffRoleLinks");
        migrationBuilder.DropTable(name: "StaffSpecializationLinks");
        migrationBuilder.DropTable(name: "StaffRoles");
        migrationBuilder.DropTable(name: "StaffSpecializations");

        migrationBuilder.DropIndex(name: "IX_Staff_OrganizationId_IsActive_EmploymentStatus", table: "Staff");
        migrationBuilder.DropColumn(name: "EmploymentStatus", table: "Staff");
        migrationBuilder.DropColumn(name: "RatingAverage", table: "Staff");
        migrationBuilder.DropColumn(name: "RatingCount", table: "Staff");
    }
}
