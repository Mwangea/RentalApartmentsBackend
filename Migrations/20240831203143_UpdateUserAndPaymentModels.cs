using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalAppartments.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAndPaymentModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Properties_PropertyId",
                table: "MaintenanceRequests");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAvailable",
                table: "Properties",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AddColumn<string>(
                name: "CurrentTenantId",
                table: "Properties",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Properties",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Available")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_CurrentTenantId",
                table: "Properties",
                column: "CurrentTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PropertyId",
                table: "Payments",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Properties_PropertyId",
                table: "MaintenanceRequests",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Properties_PropertyId",
                table: "Payments",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AspNetUsers_CurrentTenantId",
                table: "Properties",
                column: "CurrentTenantId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Properties_PropertyId",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Properties_PropertyId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AspNetUsers_CurrentTenantId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_CurrentTenantId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PropertyId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CurrentTenantId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "Payments");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAvailable",
                table: "Properties",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Properties_PropertyId",
                table: "MaintenanceRequests",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
