using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{

    public partial class AddProcessLogging : Migration
    {
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

            migrationBuilder.InsertData(
                table: "Person",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "John Doe" },
                    { 2, "Jane Doe" },
                    { 3, "Samantha Carter" },
                    { 4, "Daniel Jackson" }
                });

            migrationBuilder.InsertData(
                table: "AstronautDetail",
                columns: new[] { "Id", "CareerEndDate", "CareerStartDate", "CurrentDutyTitle", "CurrentRank", "PersonId" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Commander", "1LT", 1 },
                    { 2, null, new DateTime(2012, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Science Officer", "Major", 3 }
                });

            migrationBuilder.InsertData(
                table: "AstronautDuty",
                columns: new[] { "Id", "DutyEndDate", "DutyStartDate", "DutyTitle", "PersonId", "Rank" },
                values: new object[,]
                {
                    { 1, new DateTime(2015, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Commander", 1, "1LT" },
                    { 2, null, new DateTime(2016, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mission Lead", 1, "Captain" },
                    { 3, null, new DateTime(2012, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Science Officer", 3, "Major" },
                    { 4, null, new DateTime(2018, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pilot", 2, "Lieutenant" }
                });
        }
    }
}
