using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WareFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTaxFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Backfill: las órdenes creadas antes de la Fase 3 no tienen desglose de
            // IVA. Sus totales ya venían con IVA incluido, así que se deriva el neto
            // con el factor vigente (19%) para que los DTE de órdenes antiguas sean
            // consistentes.
            migrationBuilder.Sql(
                "UPDATE Orders " +
                "SET NetAmount = ROUND(CAST(TotalAmount / 1.19 AS decimal(18,2)), 2), " +
                "    TaxAmount = TotalAmount - ROUND(CAST(TotalAmount / 1.19 AS decimal(18,2)), 2) " +
                "WHERE TotalAmount > 0 AND NetAmount = 0 AND TaxAmount = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Orders");
        }
    }
}
