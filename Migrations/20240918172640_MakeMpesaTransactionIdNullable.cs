using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalAppartments.Migrations
{
    /// <inheritdoc />
    public partial class MakeMpesaTransactionIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MpesaTransactionId",
                table: "Payments",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "MpesaTransactionId",
                keyValue: null,
                column: "MpesaTransactionId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "MpesaTransactionId",
                table: "Payments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
