using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCRM.Server.Migrations
{
    public partial class AddStaffProfileFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RoleTitle",
                table: "Staff",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Staff",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Staff",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Staff",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Staff",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Staff",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Staff",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Staff",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Staff",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Staff");

            migrationBuilder.AlterColumn<string>(
                name: "RoleTitle",
                table: "Staff",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
