using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "Todos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "AssignedTo", "CreatedDate", "DueDate" },
                values: new object[] { null, new DateTime(2025, 10, 11, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4651), new DateTime(2025, 10, 25, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4658) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "AssignedTo", "CreatedDate", "DueDate" },
                values: new object[] { null, new DateTime(2025, 10, 12, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4688), new DateTime(2025, 10, 15, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4689) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "AssignedTo", "CreatedDate", "DueDate" },
                values: new object[] { null, new DateTime(2025, 10, 13, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4701), new DateTime(2025, 10, 18, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4701) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "AssignedTo", "CreatedDate" },
                values: new object[] { null, new DateTime(2025, 10, 14, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4713) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "5",
                columns: new[] { "AssignedTo", "CreatedDate" },
                values: new object[] { null, new DateTime(2025, 10, 14, 1, 5, 19, 867, DateTimeKind.Utc).AddTicks(4728) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "Todos");

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "CreatedDate", "DueDate" },
                values: new object[] { new DateTime(2025, 10, 11, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1134), new DateTime(2025, 10, 25, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1142) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "CreatedDate", "DueDate" },
                values: new object[] { new DateTime(2025, 10, 12, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1180), new DateTime(2025, 10, 15, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1181) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "CreatedDate", "DueDate" },
                values: new object[] { new DateTime(2025, 10, 13, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1201), new DateTime(2025, 10, 18, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1201) });

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "4",
                column: "CreatedDate",
                value: new DateTime(2025, 10, 14, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1218));

            migrationBuilder.UpdateData(
                table: "Todos",
                keyColumn: "Id",
                keyValue: "5",
                column: "CreatedDate",
                value: new DateTime(2025, 10, 14, 0, 45, 31, 504, DateTimeKind.Utc).AddTicks(1237));
        }
    }
}
