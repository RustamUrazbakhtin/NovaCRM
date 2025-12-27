using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCRM.Data.Migrations
{
    public partial class AddClientSegmentDefinitions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientSegmentDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSegmentDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSegmentDefinitions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ClientSegmentDefinitions",
                columns: new[] { "Id", "IsActive", "Key", "Label", "OrganizationId", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("3f7f0a73-b38b-4e9f-94d1-0d0f2b5759d1"), true, "All", "All", null, 1 },
                    { new Guid("2aaf7d9a-487d-4b33-9b30-cc6ad3a2e6d1"), true, "Vip", "VIP", null, 2 },
                    { new Guid("8f2b6d4d-1b46-4e9c-9f7a-e0b0a1e4e0e5"), true, "Regular", "Regular", null, 3 },
                    { new Guid("5a3b917a-e09f-4e58-90c1-5d8d4bfa3fb9"), true, "New", "New", null, 4 },
                    { new Guid("6dfc2ea0-62e9-4316-9d59-8285343334d2"), true, "AtRisk", "At risk", null, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientSegmentDefinitions_OrganizationId_Key",
                table: "ClientSegmentDefinitions",
                columns: new[] { "OrganizationId", "Key" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientSegmentDefinitions");
        }
    }
}
