using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WareFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAnomalyScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnomalyReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnomalyScore",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnomalyReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AnomalyScore",
                table: "Orders");
        }
    }
}
