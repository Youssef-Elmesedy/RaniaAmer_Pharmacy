using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Awlad_Zamzam.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddFuetuer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_NormalizedName_PhoneNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SecurityAnswerUpdatedAt",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders");

            migrationBuilder.AddColumn<DateTime>(
                name: "SecurityAnswerUpdatedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NormalizedName_PhoneNumber",
                table: "Customers",
                columns: new[] { "NormalizedName", "PhoneNumber" },
                unique: true);
        }
    }
}
