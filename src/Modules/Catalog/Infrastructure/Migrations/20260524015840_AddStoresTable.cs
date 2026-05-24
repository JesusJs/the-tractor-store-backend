using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoresTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stores",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stores", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "products",
                columns: new[] { "Id", "Brand", "Category", "Description", "EnginePower", "Highlights", "Image", "Name", "Price" },
                values: new object[,]
                {
                    { "tx-001", "TractorCorp", "autonomous", "Premium autonomous driving tractor.", "240 HP", new[] { "GPS Guided Autonomous System", "240 HP High-Performance Power", "Dynamic Torque Control & Field Optimization", "Full Warranty & physical maintenance support included" }, "https://placehold.co/600x400/png?text=Autonomous+Titan", "Autonomous Titan", 85000m },
                    { "tx-002", "HeritageIron", "classics", "Beautifully restored post-war utility tractor.", "45 HP", new[] { "Standard High-Performance Power", "Full Warranty & physical maintenance support included" }, "https://placehold.co/600x400/png?text=Classic+Vintage", "Classic Vintage 1950", 45000m }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "stores",
                columns: new[] { "Id", "Address", "City", "Image", "Name" },
                values: new object[,]
                {
                    { "store-central", "Av. de la Maquinaria 404", "Madrid", "https://placehold.co/300x200", "Central Headquarters" },
                    { "store-north", "Industrial Route 66, Km 12", "Burgos", "https://placehold.co/300x200", "North Hub" }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "product_variants",
                columns: new[] { "Sku", "ProductId", "Stock", "name", "product_id" },
                values: new object[,]
                {
                    { "TX-001-AI", "tx-001", 3, "AI Edition", "tx-001" },
                    { "TX-001-GPS", "tx-001", 8, "GPS Edition", "tx-001" },
                    { "TX-CLS-01", "tx-002", 0, "Standard Edition", "tx-002" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stores",
                schema: "catalog");

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "product_variants",
                keyColumn: "Sku",
                keyValue: "TX-001-AI");

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "product_variants",
                keyColumn: "Sku",
                keyValue: "TX-001-GPS");

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "product_variants",
                keyColumn: "Sku",
                keyValue: "TX-CLS-01");

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "products",
                keyColumn: "Id",
                keyValue: "tx-001");

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "products",
                keyColumn: "Id",
                keyValue: "tx-002");
        }
    }
}
