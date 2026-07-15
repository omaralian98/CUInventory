using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CUInventory.Migrations
{
    /// <inheritdoc />
    public partial class AddPersistedLineAggregates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinesCount",
                table: "AppStockTransfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LinesCount",
                table: "AppSales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "AppSales",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LinesCount",
                table: "AppPurchaseOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE s SET
                    s.LinesCount = l.LinesCount,
                    s.TotalAmount = l.TotalAmount
                FROM AppSales s
                INNER JOIN (
                    SELECT SaleId, COUNT(*) AS LinesCount, SUM(Quantity * UnitPrice) AS TotalAmount
                    FROM AppSaleLines
                    GROUP BY SaleId
                ) l ON l.SaleId = s.Id;

                UPDATE t SET t.LinesCount = l.LinesCount
                FROM AppStockTransfers t
                INNER JOIN (
                    SELECT StockTransferId, COUNT(*) AS LinesCount
                    FROM AppStockTransferLines
                    GROUP BY StockTransferId
                ) l ON l.StockTransferId = t.Id;

                UPDATE p SET p.LinesCount = l.LinesCount
                FROM AppPurchaseOrders p
                INNER JOIN (
                    SELECT PurchaseOrderId, COUNT(*) AS LinesCount
                    FROM AppPurchaseOrderLines
                    GROUP BY PurchaseOrderId
                ) l ON l.PurchaseOrderId = p.Id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinesCount",
                table: "AppStockTransfers");

            migrationBuilder.DropColumn(
                name: "LinesCount",
                table: "AppSales");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "AppSales");

            migrationBuilder.DropColumn(
                name: "LinesCount",
                table: "AppPurchaseOrders");
        }
    }
}
