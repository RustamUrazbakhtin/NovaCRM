using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCRM.Data.Migrations;

public partial class FixClientTagLink : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ClientTagLinks_ClientId_TagId",
            table: "ClientTagLinks");

        migrationBuilder.AddPrimaryKey(
            name: "PK_ClientTagLinks",
            table: "ClientTagLinks",
            columns: new[] { "ClientId", "TagId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_ClientTagLinks",
            table: "ClientTagLinks");

        migrationBuilder.CreateIndex(
            name: "IX_ClientTagLinks_ClientId_TagId",
            table: "ClientTagLinks",
            columns: new[] { "ClientId", "TagId" },
            unique: true);
    }
}
