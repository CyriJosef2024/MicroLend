using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroLend.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitialCreditScore",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Repayments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Repayments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Repayments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Repayments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Loans",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "RiskScore",
                table: "Loans",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Borrowers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialCreditScore",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Repayments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Repayments");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Repayments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Repayments");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Borrowers");
        }
    }
}
