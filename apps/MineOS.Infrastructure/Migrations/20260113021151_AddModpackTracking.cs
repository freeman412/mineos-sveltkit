using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MineOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModpackTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstalledModpacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CurseForgeProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ModCount = table.Column<int>(type: "INTEGER", nullable: false),
                    InstalledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstalledModpacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstalledModRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CurseForgeProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    ModName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ModpackId = table.Column<int>(type: "INTEGER", nullable: true),
                    InstalledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstalledModRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstalledModRecords_InstalledModpacks_ModpackId",
                        column: x => x.ModpackId,
                        principalTable: "InstalledModpacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstalledModpacks_ServerName_CurseForgeProjectId",
                table: "InstalledModpacks",
                columns: new[] { "ServerName", "CurseForgeProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstalledModRecords_ModpackId",
                table: "InstalledModRecords",
                column: "ModpackId");

            migrationBuilder.CreateIndex(
                name: "IX_InstalledModRecords_ServerName_FileName",
                table: "InstalledModRecords",
                columns: new[] { "ServerName", "FileName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstalledModRecords");

            migrationBuilder.DropTable(
                name: "InstalledModpacks");
        }
    }
}
