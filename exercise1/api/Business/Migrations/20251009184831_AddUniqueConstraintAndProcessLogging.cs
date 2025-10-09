using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintAndProcessLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Exception = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Controller = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RequestData = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Person_Name",
                table: "Person",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLog_Level",
                table: "ProcessLog",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLog_Timestamp",
                table: "ProcessLog",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessLog");

            migrationBuilder.DropIndex(
                name: "IX_Person_Name",
                table: "Person");
        }
    }
}
