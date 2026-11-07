using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clothy.PaymentService.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFraudSyncAndDisputedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DisputedAt",
                table: "payment_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FraudSync",
                table: "payment_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Payments paid before this migration were already sent to /score:
            // mark them Scored (1), so a later dispute sends its label right away.
            migrationBuilder.Sql("UPDATE payment_records SET \"FraudSync\" = 1 WHERE \"Status\" <> 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisputedAt",
                table: "payment_records");

            migrationBuilder.DropColumn(
                name: "FraudSync",
                table: "payment_records");
        }
    }
}
