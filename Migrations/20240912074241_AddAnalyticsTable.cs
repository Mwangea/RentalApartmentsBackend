using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalAppartments.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Analytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NewLeasesCount = table.Column<int>(type: "int", nullable: false),
                    ActiveLeasesCount = table.Column<int>(type: "int", nullable: false),
                    TotalPaymentsReceived = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SuccessfulPaymentsCount = table.Column<int>(type: "int", nullable: false),
                    FailedPaymentsCount = table.Column<int>(type: "int", nullable: false),
                    NewMaintenanceRequestsCount = table.Column<int>(type: "int", nullable: false),
                    CompletedMaintenanceRequestsCount = table.Column<int>(type: "int", nullable: false),
                    AvailablePropertiesCount = table.Column<int>(type: "int", nullable: false),
                    NotificationsSentCount = table.Column<int>(type: "int", nullable: false),
                    UnreadNotificationsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analytics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Analytics");
        }
    }
}
