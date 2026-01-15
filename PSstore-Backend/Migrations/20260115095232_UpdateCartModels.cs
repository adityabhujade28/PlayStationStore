using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSstore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceAtMoment",
                table: "CartItems",
                newName: "UnitPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Carts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "CartItems",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "AdminId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 52, 32, 242, DateTimeKind.Utc).AddTicks(8943));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 52, 32, 242, DateTimeKind.Utc).AddTicks(8966));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 52, 32, 242, DateTimeKind.Utc).AddTicks(8969));

            migrationBuilder.UpdateData(
                table: "UsersRegions",
                keyColumn: "UserRegionId",
                keyValue: 1,
                column: "StartDate",
                value: new DateTime(2026, 1, 15, 9, 52, 32, 242, DateTimeKind.Utc).AddTicks(8989));

            migrationBuilder.UpdateData(
                table: "UsersRegions",
                keyColumn: "UserRegionId",
                keyValue: 2,
                column: "StartDate",
                value: new DateTime(2026, 1, 15, 9, 52, 32, 242, DateTimeKind.Utc).AddTicks(8991));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "CartItems",
                newName: "PriceAtMoment");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "AdminId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 7, 47, 24, 325, DateTimeKind.Utc).AddTicks(9139));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 7, 47, 24, 325, DateTimeKind.Utc).AddTicks(9155));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 7, 47, 24, 325, DateTimeKind.Utc).AddTicks(9158));

            migrationBuilder.UpdateData(
                table: "UsersRegions",
                keyColumn: "UserRegionId",
                keyValue: 1,
                column: "StartDate",
                value: new DateTime(2026, 1, 15, 7, 47, 24, 325, DateTimeKind.Utc).AddTicks(9175));

            migrationBuilder.UpdateData(
                table: "UsersRegions",
                keyColumn: "UserRegionId",
                keyValue: 2,
                column: "StartDate",
                value: new DateTime(2026, 1, 15, 7, 47, 24, 325, DateTimeKind.Utc).AddTicks(9178));
        }
    }
}
