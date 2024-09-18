using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalAppartments.Migrations
{
    /// <inheritdoc />
    public partial class AddMpesaTransactionIdToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MpesaTransactionId",
                table: "Payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MpesaTransactionId",
                table: "Payments");
        }
    }
}
