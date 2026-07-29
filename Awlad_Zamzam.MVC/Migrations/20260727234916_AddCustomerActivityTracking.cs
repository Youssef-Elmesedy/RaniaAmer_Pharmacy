using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Awlad_Zamzam.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerActivityTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsActive_LastActivityAt",
                table: "Customers",
                columns: new[] { "IsActive", "LastActivityAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_IsActive_LastActivityAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                table: "Customers");
        }
    }
}
