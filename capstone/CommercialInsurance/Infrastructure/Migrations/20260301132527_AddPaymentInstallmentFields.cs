using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentInstallmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "InstallmentNumber",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaidByUserId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentFrequency",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMode",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalInstallments",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaidByUserId",
                table: "Payments",
                column: "PaidByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_PaidByUserId",
                table: "Payments",
                column: "PaidByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_PaidByUserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaidByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InstallmentNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaidByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentFrequency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentMode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TotalInstallments",
                table: "Payments");
        }
    }
}
