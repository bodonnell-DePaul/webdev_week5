using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TodoListApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    TodoCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Todos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Todos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "Description", "Name", "TodoCount" },
                values: new object[,]
                {
                    { "1", "#3b82f6", "School and university related tasks", "Academic", 0 },
                    { "2", "#10b981", "Personal life and household tasks", "Personal", 0 },
                    { "3", "#f59e0b", "Professional and career related tasks", "Work", 0 },
                    { "4", "#ef4444", "Health and fitness related activities", "Health", 0 },
                    { "5", "#8b5cf6", "Learning and skill development", "Learning", 0 }
                });

            migrationBuilder.InsertData(
                table: "Todos",
                columns: new[] { "Id", "Category", "CreatedDate", "Description", "DueDate", "IsCompleted", "Priority", "Tags", "Title" },
                values: new object[,]
                {
                    { "1", "Academic", new DateTime(2025, 10, 11, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1134), "Write and submit the final project proposal for CSC436", new DateTime(2025, 10, 25, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1142), false, "High", "[\"project\",\"academic\",\"deadline\"]", "Complete project proposal" },
                    { "2", "Personal", new DateTime(2025, 10, 12, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1180), "Buy groceries for the week including fruits and vegetables", new DateTime(2025, 10, 15, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1181), true, "Medium", "[\"shopping\",\"food\",\"weekly\"]", "Grocery shopping" },
                    { "3", "Work", new DateTime(2025, 10, 13, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1201), "Prepare slides and agenda for the weekly team meeting", new DateTime(2025, 10, 18, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1201), false, "High", "[\"meeting\",\"presentation\",\"team\"]", "Team meeting preparation" },
                    { "4", "Health", new DateTime(2025, 10, 14, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1218), "Complete 30-minute workout including cardio and strength training", null, false, "Medium", "[\"fitness\",\"health\",\"routine\"]", "Exercise routine" },
                    { "5", "Learning", new DateTime(2025, 10, 14, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1237), "Study advanced React patterns including Context API and custom hooks", null, false, "Low", "[\"learning\",\"react\",\"documentation\"]", "Read React documentation" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Todos");
        }
    }
}
