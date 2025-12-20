using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.PaymentService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "payment_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "payment_records");
        }
    }
}
