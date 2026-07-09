using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "manual"),
                    PassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegisteredByAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntryAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_Entries_Passes_PassId",
                        column: x => x.PassId,
                        principalTable: "Passes",
                        principalColumn: "PassId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Entries_Users_RegisteredByAdminId",
                        column: x => x.RegisteredByAdminId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Entries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_EntryAt",
                table: "Entries",
                column: "EntryAt");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_PassId",
                table: "Entries",
                column: "PassId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_RegisteredByAdminId",
                table: "Entries",
                column: "RegisteredByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_UserId",
                table: "Entries",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entries");
        }
    }
}
