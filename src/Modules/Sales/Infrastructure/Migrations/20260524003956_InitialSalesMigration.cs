using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TractorEcommerce.Modules.Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSalesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.CreateTable(
                name: "carts",
                schema: "sales",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carts", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                schema: "sales",
                columns: table => new
                {
                    VariantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cart_user_id = table.Column<string>(type: "character varying(200)", nullable: false),
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    VariantName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => new { x.cart_user_id, x.VariantId });
                    table.ForeignKey(
                        name: "FK_cart_items_carts_cart_user_id",
                        column: x => x.cart_user_id,
                        principalSchema: "sales",
                        principalTable: "carts",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cart_items",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "carts",
                schema: "sales");
        }
    }
}
