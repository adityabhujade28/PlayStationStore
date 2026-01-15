using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PSstore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AdminPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FreeToPlay = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsMultiplayer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameId);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RegionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RegionTimezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.RegionId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.SubscriptionId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SubscriptionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "GameCategories",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCategories", x => new { x.GameId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_GameCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameCategories_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameSubscriptions",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSubscriptions", x => new { x.GameId, x.SubscriptionId });
                    table.ForeignKey(
                        name: "FK_GameSubscriptions_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameSubscriptions_SubscriptionPlans_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanRegions",
                columns: table => new
                {
                    SubscriptionPlanRegionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanRegions", x => x.SubscriptionPlanRegionId);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanRegions_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanRegions_SubscriptionPlans_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    CartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPurchaseGames",
                columns: table => new
                {
                    PurchaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPurchaseGames", x => x.PurchaseId);
                    table.ForeignKey(
                        name: "FK_UserPurchaseGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPurchaseGames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsersRegions",
                columns: table => new
                {
                    UserRegionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersRegions", x => x.UserRegionId);
                    table.ForeignKey(
                        name: "FK_UsersRegions_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsersRegions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptionPlans",
                columns: table => new
                {
                    UserSubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionPlanRegionId = table.Column<int>(type: "int", nullable: false),
                    PlanStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanEndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionPlans", x => x.UserSubscriptionId);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPlans_SubscriptionPlanRegions_SubscriptionPlanRegionId",
                        column: x => x.SubscriptionPlanRegionId,
                        principalTable: "SubscriptionPlanRegions",
                        principalColumn: "SubscriptionPlanRegionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPlans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PriceAtMoment = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemId);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "CartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "AdminId", "AdminEmail", "AdminPassword", "CreatedAt" },
                values: new object[] { 1, "admin@psstore.com", "Admin@123", new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2515) });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Action" },
                    { 2, "Adventure" },
                    { 3, "RPG" },
                    { 4, "Sports" },
                    { 5, "Racing" }
                });

            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "GameId", "FreeToPlay", "GameName", "IsMultiplayer", "Price", "PublishedBy", "ReleaseDate" },
                values: new object[,]
                {
                    { 1, false, "God of War", false, 49.99m, "Sony", new DateTime(2018, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, false, "Spider-Man", false, 59.99m, "Sony", new DateTime(2018, 9, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, true, "Fortnite", true, 0m, "Epic Games", new DateTime(2017, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, false, "Gran Turismo 7", true, 69.99m, "Sony", new DateTime(2022, 3, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "RegionId", "Currency", "RegionCode", "RegionName", "RegionTimezone" },
                values: new object[,]
                {
                    { 1, "USD", "US", "United States", "America/New_York" },
                    { 2, "EUR", "EU", "Europe", "Europe/London" },
                    { 3, "JPY", "JP", "Japan", "Asia/Tokyo" },
                    { 4, "INR", "IN", "India", "Asia/Kolkata" }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "SubscriptionId", "SubscriptionType" },
                values: new object[,]
                {
                    { 1, "PlayStation Plus Essential" },
                    { 2, "PlayStation Plus Extra" },
                    { 3, "PlayStation Plus Premium" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Age", "CreatedAt", "SubscriptionStatus", "UserEmail", "UserName", "UserPassword" },
                values: new object[,]
                {
                    { 1, 28, new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2535), null, "john@example.com", "john_doe", "Pass@123" },
                    { 2, 25, new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2538), "Active", "jane@example.com", "jane_smith", "Pass@456" }
                });

            migrationBuilder.InsertData(
                table: "GameCategories",
                columns: new[] { "CategoryId", "GameId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 1, 3 },
                    { 5, 4 }
                });

            migrationBuilder.InsertData(
                table: "GameSubscriptions",
                columns: new[] { "GameId", "SubscriptionId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 1, 3 },
                    { 2, 3 }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlanRegions",
                columns: new[] { "SubscriptionPlanRegionId", "Currency", "Price", "RegionId", "SubscriptionId" },
                values: new object[,]
                {
                    { 1, "USD", 9.99m, 1, 1 },
                    { 2, "EUR", 8.99m, 2, 1 },
                    { 3, "USD", 14.99m, 1, 2 },
                    { 4, "EUR", 13.99m, 2, 2 },
                    { 5, "USD", 17.99m, 1, 3 }
                });

            migrationBuilder.InsertData(
                table: "UsersRegions",
                columns: new[] { "UserRegionId", "EndDate", "IsActive", "RegionId", "StartDate", "UserId" },
                values: new object[,]
                {
                    { 1, null, true, 1, new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2560), 1 },
                    { 2, null, true, 2, new DateTime(2026, 1, 15, 6, 38, 44, 66, DateTimeKind.Utc).AddTicks(2561), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_GameId",
                table: "CartItems",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCategories_CategoryId",
                table: "GameCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSubscriptions_SubscriptionId",
                table: "GameSubscriptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_RegionCode",
                table: "Regions",
                column: "RegionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanRegions_RegionId",
                table: "SubscriptionPlanRegions",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanRegions_SubscriptionId_RegionId",
                table: "SubscriptionPlanRegions",
                columns: new[] { "SubscriptionId", "RegionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPurchaseGames_GameId",
                table: "UserPurchaseGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPurchaseGames_UserId",
                table: "UserPurchaseGames",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserEmail",
                table: "Users",
                column: "UserEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersRegions_RegionId",
                table: "UsersRegions",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersRegions_UserId",
                table: "UsersRegions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPlans_SubscriptionPlanRegionId",
                table: "UserSubscriptionPlans",
                column: "SubscriptionPlanRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPlans_UserId",
                table: "UserSubscriptionPlans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "GameCategories");

            migrationBuilder.DropTable(
                name: "GameSubscriptions");

            migrationBuilder.DropTable(
                name: "UserPurchaseGames");

            migrationBuilder.DropTable(
                name: "UsersRegions");

            migrationBuilder.DropTable(
                name: "UserSubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanRegions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");
        }
    }
}
