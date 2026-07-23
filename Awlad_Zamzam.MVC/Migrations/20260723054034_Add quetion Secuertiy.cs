using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Awlad_Zamzam.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddquetionSecuertiy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecurityAnswerHash",
                table: "Customers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SecurityAnswerUpdatedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityQuestion",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NormalizedName_PhoneNumber",
                table: "Customers",
                columns: new[] { "NormalizedName", "PhoneNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_NormalizedName_PhoneNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SecurityAnswerHash",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SecurityAnswerUpdatedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SecurityQuestion",
                table: "Customers");
        }
    }
}
