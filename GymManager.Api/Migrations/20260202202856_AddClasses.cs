using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassTypes",
                columns: table => new
                {
                    ClassTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTypes", x => x.ClassTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ClassSessions",
                columns: table => new
                {
                    ClassSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ClassTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsCanceled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSessions", x => x.ClassSessionId);
                    table.ForeignKey(
                        name: "FK_ClassSessions_ClassTypes_ClassTypeId",
                        column: x => x.ClassTypeId,
                        principalTable: "ClassTypes",
                        principalColumn: "ClassTypeId");
                });

            migrationBuilder.CreateTable(
                name: "ClassReservations",
                columns: table => new
                {
                    ClassReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CanceledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassReservations", x => x.ClassReservationId);
                    table.ForeignKey(
                        name: "FK_ClassReservations_ClassSessions_ClassSessionId",
                        column: x => x.ClassSessionId,
                        principalTable: "ClassSessions",
                        principalColumn: "ClassSessionId");
                    table.ForeignKey(
                        name: "FK_ClassReservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassReservations_ClassSessionId",
                table: "ClassReservations",
                column: "ClassSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassReservations_Status",
                table: "ClassReservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClassReservations_UserId_ClassSessionId",
                table: "ClassReservations",
                columns: new[] { "UserId", "ClassSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_ClassTypeId",
                table: "ClassSessions",
                column: "ClassTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_IsCanceled",
                table: "ClassSessions",
                column: "IsCanceled");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_StartAt",
                table: "ClassSessions",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTypes_IsActive",
                table: "ClassTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTypes_Name",
                table: "ClassTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassReservations");

            migrationBuilder.DropTable(
                name: "ClassSessions");

            migrationBuilder.DropTable(
                name: "ClassTypes");
        }
    }
}
