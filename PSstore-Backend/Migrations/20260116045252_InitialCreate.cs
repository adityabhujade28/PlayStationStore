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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsMultiplayer = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    RegionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
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
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                    table.ForeignKey(
                        name: "FK_Countries_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "GameCountries",
                columns: table => new
                {
                    GameCountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCountries", x => x.GameCountryId);
                    table.ForeignKey(
                        name: "FK_GameCountries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameCountries_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanCountries",
                columns: table => new
                {
                    SubscriptionPlanCountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanCountries", x => x.SubscriptionPlanCountryId);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanCountries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanCountries_SubscriptionPlans_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Cascade);
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    CartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
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
                name: "UserSubscriptionPlans",
                columns: table => new
                {
                    UserSubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionPlanCountryId = table.Column<int>(type: "int", nullable: false),
                    PlanStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanEndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionPlans", x => x.UserSubscriptionId);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionPlans_SubscriptionPlanCountries_SubscriptionPlanCountryId",
                        column: x => x.SubscriptionPlanCountryId,
                        principalTable: "SubscriptionPlanCountries",
                        principalColumn: "SubscriptionPlanCountryId",
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
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
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
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "DeletedAt", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Action", null, false },
                    { 2, "Adventure", null, false },
                    { 3, "RPG", null, false },
                    { 4, "Sports", null, false },
                    { 5, "Racing", null, false },
                    { 6, "Shooter", null, false },
                    { 7, "Horror", null, false },
                    { 8, "Platformer", null, false }
                });

            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "GameId", "BasePrice", "DeletedAt", "FreeToPlay", "GameName", "IsDeleted", "IsMultiplayer", "PublishedBy", "ReleaseDate" },
                values: new object[,]
                {
                    { 1, 49.99m, null, false, "God of War", false, false, "Sony Interactive Entertainment", new DateTime(2018, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 59.99m, null, false, "Spider-Man", false, false, "Sony Interactive Entertainment", new DateTime(2018, 9, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 59.99m, null, false, "The Last of Us Part II", false, false, "Sony Interactive Entertainment", new DateTime(2020, 6, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 59.99m, null, false, "Ghost of Tsushima", false, false, "Sony Interactive Entertainment", new DateTime(2020, 7, 17, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, 49.99m, null, false, "Horizon Zero Dawn", false, false, "Sony Interactive Entertainment", new DateTime(2017, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, 39.99m, null, false, "Uncharted 4: A Thief's End", false, true, "Sony Interactive Entertainment", new DateTime(2016, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, 39.99m, null, false, "Bloodborne", false, true, "Sony Interactive Entertainment", new DateTime(2015, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, 69.99m, null, false, "Ratchet & Clank: Rift Apart", false, false, "Sony Interactive Entertainment", new DateTime(2021, 6, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, 69.99m, null, false, "Demon's Souls", false, true, "Sony Interactive Entertainment", new DateTime(2020, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, 69.99m, null, false, "Returnal", false, false, "Sony Interactive Entertainment", new DateTime(2021, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 11, 69.99m, null, false, "Gran Turismo 7", false, true, "Sony Interactive Entertainment", new DateTime(2022, 3, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, 69.99m, null, false, "Horizon Forbidden West", false, false, "Sony Interactive Entertainment", new DateTime(2022, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 13, 59.99m, null, false, "Sackboy: A Big Adventure", false, true, "Sony Interactive Entertainment", new DateTime(2020, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 14, 39.99m, null, false, "Days Gone", false, false, "Sony Interactive Entertainment", new DateTime(2019, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 15, 59.99m, null, false, "Death Stranding", false, true, "Sony Interactive Entertainment", new DateTime(2019, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 16, 49.99m, null, false, "Spider-Man: Miles Morales", false, false, "Sony Interactive Entertainment", new DateTime(2020, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 17, 0m, null, true, "Astro's Playroom", false, false, "Sony Interactive Entertainment", new DateTime(2020, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 18, 29.99m, null, false, "Infamous Second Son", false, false, "Sony Interactive Entertainment", new DateTime(2014, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 19, 29.99m, null, false, "Until Dawn", false, false, "Sony Interactive Entertainment", new DateTime(2015, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 20, 19.99m, null, false, "The Order: 1886", false, false, "Sony Interactive Entertainment", new DateTime(2015, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 21, 0m, null, true, "Fortnite", false, true, "Epic Games", new DateTime(2017, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 22, 69.99m, null, false, "Call of Duty: Modern Warfare II", false, true, "Activision", new DateTime(2022, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 23, 69.99m, null, false, "FIFA 23", false, true, "Electronic Arts", new DateTime(2022, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 24, 69.99m, null, false, "NBA 2K23", false, true, "2K Sports", new DateTime(2022, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 25, 59.99m, null, false, "Red Dead Redemption 2", false, true, "Rockstar Games", new DateTime(2018, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 26, 29.99m, null, false, "Grand Theft Auto V", false, true, "Rockstar Games", new DateTime(2013, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 27, 39.99m, null, false, "The Witcher 3: Wild Hunt", false, false, "CD Projekt", new DateTime(2015, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 28, 59.99m, null, false, "Cyberpunk 2077", false, false, "CD Projekt", new DateTime(2020, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 29, 59.99m, null, false, "Assassin's Creed Valhalla", false, false, "Ubisoft", new DateTime(2020, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 30, 59.99m, null, false, "Far Cry 6", false, true, "Ubisoft", new DateTime(2021, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 31, 59.99m, null, false, "Resident Evil Village", false, false, "Capcom", new DateTime(2021, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 32, 59.99m, null, false, "Elden Ring", false, true, "Bandai Namco", new DateTime(2022, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 33, 39.99m, null, false, "Dark Souls III", false, true, "Bandai Namco", new DateTime(2016, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 34, 59.99m, null, false, "Sekiro: Shadows Die Twice", false, false, "Activision", new DateTime(2019, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 35, 29.99m, null, false, "Monster Hunter World", false, true, "Capcom", new DateTime(2018, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 36, 59.99m, null, false, "Final Fantasy VII Remake", false, false, "Square Enix", new DateTime(2020, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 37, 69.99m, null, false, "Final Fantasy XVI", false, false, "Square Enix", new DateTime(2023, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 38, 59.99m, null, false, "Persona 5 Royal", false, false, "Atlus", new DateTime(2019, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 39, 59.99m, null, false, "Dragon Ball Z: Kakarot", false, false, "Bandai Namco", new DateTime(2020, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 40, 49.99m, null, false, "Mortal Kombat 11", false, true, "Warner Bros", new DateTime(2019, 4, 23, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 41, 69.99m, null, false, "Street Fighter 6", false, true, "Capcom", new DateTime(2023, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 42, 39.99m, null, false, "Tekken 7", false, true, "Bandai Namco", new DateTime(2017, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 43, 39.99m, null, false, "Control", false, false, "505 Games", new DateTime(2019, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 44, 39.99m, null, false, "It Takes Two", false, true, "Electronic Arts", new DateTime(2021, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 45, 0m, null, true, "Apex Legends", false, true, "Electronic Arts", new DateTime(2019, 2, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 46, 0m, null, true, "Overwatch 2", false, true, "Blizzard Entertainment", new DateTime(2022, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 47, 59.99m, null, false, "Battlefield 2042", false, true, "Electronic Arts", new DateTime(2021, 11, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 48, 59.99m, null, false, "Doom Eternal", false, true, "Bethesda", new DateTime(2020, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 49, 24.99m, null, false, "Hades", false, false, "Supergiant Games", new DateTime(2020, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 50, 29.99m, null, false, "Stray", false, false, "Annapurna Interactive", new DateTime(2022, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "RegionId", "RegionCode", "RegionName" },
                values: new object[,]
                {
                    { 1, "ASIA", "Asia" },
                    { 2, "EU", "Europe" }
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
                table: "Countries",
                columns: new[] { "CountryId", "CountryCode", "CountryName", "Currency", "RegionId", "TaxRate", "Timezone" },
                values: new object[,]
                {
                    { 1, "IN", "India", "INR", 1, 0.18m, "Asia/Kolkata" },
                    { 2, "UK", "United Kingdom", "GBP", 2, 0.20m, "Europe/London" }
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
                    { 2, 3 },
                    { 7, 3 },
                    { 1, 4 },
                    { 2, 4 },
                    { 1, 5 },
                    { 2, 5 },
                    { 3, 5 },
                    { 1, 6 },
                    { 2, 6 },
                    { 6, 6 },
                    { 1, 7 },
                    { 3, 7 },
                    { 7, 7 },
                    { 1, 8 },
                    { 8, 8 },
                    { 1, 9 },
                    { 3, 9 },
                    { 1, 10 },
                    { 6, 10 },
                    { 4, 11 },
                    { 5, 11 },
                    { 1, 12 },
                    { 2, 12 },
                    { 3, 12 },
                    { 1, 21 },
                    { 6, 21 },
                    { 1, 22 },
                    { 6, 22 },
                    { 4, 23 },
                    { 1, 25 },
                    { 2, 25 },
                    { 2, 27 },
                    { 3, 27 },
                    { 1, 32 },
                    { 3, 32 },
                    { 1, 36 },
                    { 3, 36 },
                    { 3, 38 },
                    { 1, 41 },
                    { 1, 45 },
                    { 6, 45 },
                    { 1, 49 },
                    { 3, 49 }
                });

            migrationBuilder.InsertData(
                table: "GameSubscriptions",
                columns: new[] { "GameId", "SubscriptionId" },
                values: new object[,]
                {
                    { 1, 3 },
                    { 2, 3 },
                    { 3, 3 },
                    { 4, 3 },
                    { 5, 3 },
                    { 6, 2 },
                    { 6, 3 },
                    { 7, 1 },
                    { 7, 2 },
                    { 7, 3 },
                    { 8, 3 },
                    { 9, 3 },
                    { 10, 3 },
                    { 11, 3 },
                    { 12, 3 },
                    { 13, 2 },
                    { 13, 3 },
                    { 14, 1 },
                    { 14, 2 },
                    { 14, 3 },
                    { 16, 2 },
                    { 16, 3 },
                    { 17, 1 },
                    { 17, 2 },
                    { 17, 3 },
                    { 18, 1 },
                    { 18, 2 },
                    { 18, 3 },
                    { 19, 1 },
                    { 19, 2 },
                    { 19, 3 },
                    { 20, 1 },
                    { 20, 2 },
                    { 20, 3 },
                    { 21, 1 },
                    { 21, 2 },
                    { 21, 3 },
                    { 26, 1 },
                    { 26, 2 },
                    { 26, 3 },
                    { 27, 2 },
                    { 27, 3 },
                    { 35, 1 },
                    { 35, 2 },
                    { 35, 3 },
                    { 40, 2 },
                    { 40, 3 },
                    { 42, 1 },
                    { 42, 2 },
                    { 42, 3 },
                    { 43, 1 },
                    { 43, 2 },
                    { 43, 3 },
                    { 45, 1 },
                    { 45, 2 },
                    { 45, 3 },
                    { 46, 1 },
                    { 46, 2 },
                    { 46, 3 },
                    { 49, 1 },
                    { 49, 2 },
                    { 49, 3 },
                    { 50, 1 },
                    { 50, 2 },
                    { 50, 3 }
                });

            migrationBuilder.InsertData(
                table: "GameCountries",
                columns: new[] { "GameCountryId", "CountryId", "GameId", "Price" },
                values: new object[,]
                {
                    { 1, 1, 1, 4149.17m },
                    { 2, 1, 2, 4979.17m },
                    { 3, 1, 3, 4979.17m },
                    { 4, 1, 4, 4979.17m },
                    { 5, 1, 5, 4149.17m },
                    { 6, 1, 6, 3319.17m },
                    { 7, 1, 7, 3319.17m },
                    { 8, 1, 8, 5809.17m },
                    { 9, 1, 9, 5809.17m },
                    { 10, 1, 10, 5809.17m },
                    { 11, 1, 11, 5809.17m },
                    { 12, 1, 12, 4979.17m },
                    { 13, 1, 13, 4149.17m },
                    { 14, 1, 14, 3319.17m },
                    { 15, 1, 15, 4979.17m },
                    { 16, 1, 16, 4149.17m },
                    { 17, 1, 17, 0m },
                    { 18, 1, 18, 2489.17m },
                    { 19, 1, 19, 2489.17m },
                    { 20, 1, 20, 1659.17m },
                    { 21, 1, 21, 0m },
                    { 22, 1, 22, 5809.17m },
                    { 23, 1, 23, 5809.17m },
                    { 24, 1, 24, 5809.17m },
                    { 25, 1, 25, 4979.17m },
                    { 26, 1, 26, 2489.17m },
                    { 27, 1, 27, 4979.17m },
                    { 28, 1, 28, 4979.17m },
                    { 29, 1, 29, 4979.17m },
                    { 30, 1, 30, 4979.17m },
                    { 31, 1, 31, 4979.17m },
                    { 32, 1, 32, 4979.17m },
                    { 33, 1, 33, 3319.17m },
                    { 34, 1, 34, 4979.17m },
                    { 35, 1, 35, 3319.17m },
                    { 36, 1, 36, 4979.17m },
                    { 37, 1, 37, 5809.17m },
                    { 38, 1, 38, 4979.17m },
                    { 39, 1, 39, 4979.17m },
                    { 40, 1, 40, 4149.17m },
                    { 41, 1, 41, 5809.17m },
                    { 42, 1, 42, 4149.17m },
                    { 43, 1, 43, 3319.17m },
                    { 44, 1, 44, 3319.17m },
                    { 45, 1, 45, 0m },
                    { 46, 1, 46, 0m },
                    { 47, 1, 47, 4979.17m },
                    { 48, 1, 48, 4979.17m },
                    { 49, 1, 49, 2074.17m },
                    { 50, 1, 50, 3319.17m },
                    { 51, 2, 1, 39.49m },
                    { 52, 2, 2, 47.39m },
                    { 53, 2, 3, 47.39m },
                    { 54, 2, 4, 47.39m },
                    { 55, 2, 5, 39.49m },
                    { 56, 2, 6, 31.59m },
                    { 57, 2, 7, 31.59m },
                    { 58, 2, 8, 55.29m },
                    { 59, 2, 9, 55.29m },
                    { 60, 2, 10, 55.29m },
                    { 61, 2, 11, 55.29m },
                    { 62, 2, 12, 47.39m },
                    { 63, 2, 13, 39.49m },
                    { 64, 2, 14, 31.59m },
                    { 65, 2, 15, 47.39m },
                    { 66, 2, 16, 39.49m },
                    { 67, 2, 17, 0m },
                    { 68, 2, 18, 23.69m },
                    { 69, 2, 19, 23.69m },
                    { 70, 2, 20, 15.79m },
                    { 71, 2, 21, 0m },
                    { 72, 2, 22, 55.29m },
                    { 73, 2, 23, 55.29m },
                    { 74, 2, 24, 55.29m },
                    { 75, 2, 25, 47.39m },
                    { 76, 2, 26, 23.69m },
                    { 77, 2, 27, 47.39m },
                    { 78, 2, 28, 47.39m },
                    { 79, 2, 29, 47.39m },
                    { 80, 2, 30, 47.39m },
                    { 81, 2, 31, 47.39m },
                    { 82, 2, 32, 47.39m },
                    { 83, 2, 33, 31.59m },
                    { 84, 2, 34, 47.39m },
                    { 85, 2, 35, 31.59m },
                    { 86, 2, 36, 47.39m },
                    { 87, 2, 37, 55.29m },
                    { 88, 2, 38, 47.39m },
                    { 89, 2, 39, 47.39m },
                    { 90, 2, 40, 39.49m },
                    { 91, 2, 41, 55.29m },
                    { 92, 2, 42, 39.49m },
                    { 93, 2, 43, 31.59m },
                    { 94, 2, 44, 31.59m },
                    { 95, 2, 45, 0m },
                    { 96, 2, 46, 0m },
                    { 97, 2, 47, 47.39m },
                    { 98, 2, 48, 47.39m },
                    { 99, 2, 49, 19.74m },
                    { 100, 2, 50, 31.59m }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlanCountries",
                columns: new[] { "SubscriptionPlanCountryId", "CountryId", "DurationMonths", "Price", "SubscriptionId" },
                values: new object[,]
                {
                    { 1, 1, 1, 749m, 1 },
                    { 2, 1, 3, 1999m, 1 },
                    { 3, 1, 12, 4999m, 1 },
                    { 4, 2, 1, 6.99m, 1 },
                    { 5, 2, 3, 19.99m, 1 },
                    { 6, 2, 12, 49.99m, 1 },
                    { 7, 1, 1, 1249m, 2 },
                    { 8, 1, 3, 3299m, 2 },
                    { 9, 1, 12, 8299m, 2 },
                    { 10, 2, 1, 10.99m, 2 },
                    { 11, 2, 3, 31.99m, 2 },
                    { 12, 2, 12, 83.99m, 2 },
                    { 13, 1, 1, 1499m, 3 },
                    { 14, 1, 3, 3999m, 3 },
                    { 15, 1, 12, 9999m, 3 },
                    { 16, 2, 1, 13.49m, 3 },
                    { 17, 2, 3, 39.99m, 3 },
                    { 18, 2, 12, 99.99m, 3 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Age", "CountryId", "CreatedAt", "DeletedAt", "IsDeleted", "SubscriptionStatus", "UserEmail", "UserName", "UserPassword" },
                values: new object[,]
                {
                    { 1, 20, 1, new DateTime(2025, 12, 17, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4720), null, false, null, "arjun.india@example.com", "arjun_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 2, 21, 1, new DateTime(2025, 12, 18, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4730), null, false, null, "priya.india@example.com", "priya_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 3, 22, 1, new DateTime(2025, 12, 19, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4734), null, false, null, "raj.india@example.com", "raj_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 4, 23, 1, new DateTime(2025, 12, 20, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4737), null, false, null, "ananya.india@example.com", "ananya_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 5, 24, 1, new DateTime(2025, 12, 21, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4739), null, false, null, "vikram.india@example.com", "vikram_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 6, 25, 1, new DateTime(2025, 12, 22, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4743), null, false, null, "sneha.india@example.com", "sneha_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 7, 26, 1, new DateTime(2025, 12, 23, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4746), null, false, null, "rohan.india@example.com", "rohan_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 8, 27, 1, new DateTime(2025, 12, 24, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4781), null, false, null, "diya.india@example.com", "diya_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 9, 28, 1, new DateTime(2025, 12, 25, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4783), null, false, null, "karan.india@example.com", "karan_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 10, 29, 1, new DateTime(2025, 12, 26, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4786), null, false, null, "meera.india@example.com", "meera_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 11, 30, 1, new DateTime(2025, 12, 27, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4788), null, false, null, "aditya.india@example.com", "aditya_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 12, 31, 1, new DateTime(2025, 12, 28, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4790), null, false, null, "kavya.india@example.com", "kavya_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 13, 32, 1, new DateTime(2025, 12, 29, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4793), null, false, null, "siddharth.india@example.com", "siddharth_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 14, 33, 1, new DateTime(2025, 12, 30, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4795), null, false, null, "isha.india@example.com", "isha_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 15, 34, 1, new DateTime(2025, 12, 31, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4797), null, false, null, "rahul.india@example.com", "rahul_ind", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 16, 22, 2, new DateTime(2025, 12, 17, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4728), null, false, null, "james.uk@example.com", "james_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 17, 23, 2, new DateTime(2025, 12, 18, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4732), null, false, null, "emma.uk@example.com", "emma_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 18, 24, 2, new DateTime(2025, 12, 19, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4736), null, false, null, "oliver.uk@example.com", "oliver_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 19, 25, 2, new DateTime(2025, 12, 20, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4739), null, false, null, "sophie.uk@example.com", "sophie_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 20, 26, 2, new DateTime(2025, 12, 21, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4741), null, false, null, "william.uk@example.com", "william_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 21, 27, 2, new DateTime(2025, 12, 22, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4745), null, false, null, "charlotte.uk@example.com", "charlotte_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 22, 28, 2, new DateTime(2025, 12, 23, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4747), null, false, null, "harry.uk@example.com", "harry_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 23, 29, 2, new DateTime(2025, 12, 24, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4782), null, false, null, "amelia.uk@example.com", "amelia_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 24, 30, 2, new DateTime(2025, 12, 25, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4785), null, false, null, "george.uk@example.com", "george_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 25, 31, 2, new DateTime(2025, 12, 26, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4787), null, false, null, "emily.uk@example.com", "emily_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 26, 32, 2, new DateTime(2025, 12, 27, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4789), null, false, null, "jack.uk@example.com", "jack_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 27, 33, 2, new DateTime(2025, 12, 28, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4791), null, false, null, "isabella.uk@example.com", "isabella_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 28, 34, 2, new DateTime(2025, 12, 29, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4794), null, false, null, "thomas.uk@example.com", "thomas_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 29, 35, 2, new DateTime(2025, 12, 30, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4796), null, false, null, "mia.uk@example.com", "mia_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" },
                    { 30, 36, 2, new DateTime(2025, 12, 31, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4798), null, false, null, "joshua.uk@example.com", "joshua_uk", "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm" }
                });

            migrationBuilder.InsertData(
                table: "UserPurchaseGames",
                columns: new[] { "PurchaseId", "GameId", "PurchaseDate", "PurchasePrice", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 6, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4916), 4149.17m, 1 },
                    { 2, 27, new DateTime(2026, 1, 8, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4918), 3319.17m, 2 },
                    { 3, 2, new DateTime(2026, 1, 11, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4919), 47.39m, 16 },
                    { 4, 32, new DateTime(2026, 1, 13, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4919), 47.39m, 17 },
                    { 5, 22, new DateTime(2026, 1, 14, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4920), 5809.17m, 3 }
                });

            migrationBuilder.InsertData(
                table: "UserSubscriptionPlans",
                columns: new[] { "UserSubscriptionId", "PlanEndDate", "PlanStartDate", "SubscriptionPlanCountryId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 12, 27, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4942), new DateTime(2025, 12, 27, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4941), 3, 4 },
                    { 2, new DateTime(2027, 1, 1, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4944), new DateTime(2026, 1, 1, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4944), 9, 5 },
                    { 3, new DateTime(2026, 12, 22, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4945), new DateTime(2025, 12, 22, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4945), 6, 18 },
                    { 4, new DateTime(2027, 1, 6, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4947), new DateTime(2026, 1, 6, 4, 52, 52, 446, DateTimeKind.Utc).AddTicks(4946), 18, 19 }
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
                name: "IX_Countries_CountryCode",
                table: "Countries",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_RegionId",
                table: "Countries",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCategories_CategoryId",
                table: "GameCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCountries_CountryId",
                table: "GameCountries",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCountries_GameId_CountryId",
                table: "GameCountries",
                columns: new[] { "GameId", "CountryId" },
                unique: true);

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
                name: "IX_SubscriptionPlanCountries_CountryId",
                table: "SubscriptionPlanCountries",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanCountries_SubscriptionId_CountryId_DurationMonths",
                table: "SubscriptionPlanCountries",
                columns: new[] { "SubscriptionId", "CountryId", "DurationMonths" },
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
                name: "IX_Users_CountryId",
                table: "Users",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserEmail",
                table: "Users",
                column: "UserEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPlans_SubscriptionPlanCountryId",
                table: "UserSubscriptionPlans",
                column: "SubscriptionPlanCountryId");

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
                name: "GameCountries");

            migrationBuilder.DropTable(
                name: "GameSubscriptions");

            migrationBuilder.DropTable(
                name: "UserPurchaseGames");

            migrationBuilder.DropTable(
                name: "UserSubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanCountries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
