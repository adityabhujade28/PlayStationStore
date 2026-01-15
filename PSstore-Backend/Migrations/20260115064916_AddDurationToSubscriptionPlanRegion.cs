using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PSstore.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationToSubscriptionPlanRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanRegions_SubscriptionId_RegionId",
                table: "SubscriptionPlanRegions");

            migrationBuilder.AddColumn<int>(
                name: "DurationMonths",
                table: "SubscriptionPlanRegions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "AdminId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 6, 49, 15, 725, DateTimeKind.Utc).AddTicks(7786));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 1,
                column: "DurationMonths",
                value: 1);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 2,
                columns: new[] { "Currency", "DurationMonths", "Price", "RegionId" },
                values: new object[] { "USD", 3, 24.99m, 1 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 3,
                columns: new[] { "DurationMonths", "Price", "SubscriptionId" },
                values: new object[] { 12, 59.99m, 1 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 4,
                columns: new[] { "DurationMonths", "Price", "SubscriptionId" },
                values: new object[] { 1, 8.99m, 1 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 5,
                columns: new[] { "Currency", "DurationMonths", "Price", "RegionId", "SubscriptionId" },
                values: new object[] { "EUR", 3, 22.99m, 2, 1 });

            migrationBuilder.InsertData(
                table: "SubscriptionPlanRegions",
                columns: new[] { "SubscriptionPlanRegionId", "Currency", "DurationMonths", "Price", "RegionId", "SubscriptionId" },
                values: new object[,]
                {
                    { 6, "EUR", 12, 54.99m, 2, 1 },
                    { 7, "USD", 1, 14.99m, 1, 2 },
                    { 8, "USD", 3, 39.99m, 1, 2 },
                    { 9, "USD", 12, 99.99m, 1, 2 },
                    { 10, "EUR", 1, 13.99m, 2, 2 },
                    { 11, "EUR", 3, 36.99m, 2, 2 },
                    { 12, "EUR", 12, 89.99m, 2, 2 },
                    { 13, "USD", 1, 17.99m, 1, 3 },
                    { 14, "USD", 3, 49.99m, 1, 3 },
                    { 15, "USD", 12, 119.99m, 1, 3 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 6, 49, 15, 725, DateTimeKind.Utc).AddTicks(7807));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 6, 49, 15, 725, DateTimeKind.Utc).AddTicks(7810));

            migrationBuilder.UpdateData(
                table: "UsersRegions",
                keyColumn: "UserRegionId",
                keyValue: 1,
                column: "StartDate",
                value: new DateTime(2026, 1, 15, 6, 49, 15, 725, DateTimeKind.Utc).AddTicks(7828));

            migrationBuilder.UpdateData(
                table: "UsersRegions",
                keyColumn: "UserRegionId",
                keyValue: 2,
                column: "StartDate",
                value: new DateTime(2026, 1, 15, 6, 49, 15, 725, DateTimeKind.Utc).AddTicks(7829));

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanRegions_SubscriptionId_RegionId_DurationMonths",
                table: "SubscriptionPlanRegions",
                columns: new[] { "SubscriptionId", "RegionId", "DurationMonths" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanRegions_SubscriptionId_RegionId_DurationMonths",
                table: "SubscriptionPlanRegions");

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 15);

            migrationBuilder.DropColumn(
                name: "DurationMonths",
                table: "SubscriptionPlanRegions");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "AdminId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2515));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 2,
                columns: new[] { "Currency", "Price", "RegionId" },
                values: new object[] { "EUR", 8.99m, 2 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 3,
                columns: new[] { "Price", "SubscriptionId" },
                values: new object[] { 14.99m, 2 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 4,
                columns: new[] { "Price", "SubscriptionId" },
                values: new object[] { 13.99m, 2 });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlanRegions",
                keyColumn: "SubscriptionPlanRegionId",
                keyValue: 5,
                columns: new[] { "Currency", "Price", "RegionId", "SubscriptionId" },
                values: new object[] { "USD", 17.99m, 1, 3 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2535));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2538));

            migrationBuilder.UpdateData(
                table: "UsersRegions",
                keyColumn: "UserRegionId",
                keyValue: 1,
                column: "StartDate",
                value: new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2560));

            migrationBuilder.UpdateData(
                table: "UsersRegions",
                keyColumn: "UserRegionId",
                keyValue: 2,
                column: "StartDate",
                value: new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2561));

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanRegions_SubscriptionId_RegionId",
                table: "SubscriptionPlanRegions",
                columns: new[] { "SubscriptionId", "RegionId" },
                unique: true);
        }
    }
}
