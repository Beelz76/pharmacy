using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pharmacy.Database.Migrations
{
    /// <inheritdoc />
    public partial class M2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.DeleteData(
                table: "Ref_PaymentMethods",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Ожидает оплаты", "WaitingForPayment" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Ожидает обработки", "Pending" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "В обработке", "Processing" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Готов к получению", "ReadyForReceive" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Получен", "Received" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Отменен", "Cancelled" });

            migrationBuilder.InsertData(
                table: "Ref_OrderStatuses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 7, "Возврат средств", "Refunded" });

            migrationBuilder.UpdateData(
                table: "Ref_PaymentMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Картой онлайн");

            migrationBuilder.UpdateData(
                table: "Ref_PaymentMethods",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "При получении", "OnDelivery" });

            migrationBuilder.InsertData(
                table: "Ref_PaymentStatuses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 5, "Не оплачено", "NotPaid" },
                    { 6, "Возвращено", "Refunded" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.DeleteData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Ref_PaymentStatuses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Ref_PaymentStatuses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Ожидает обработки", "Pending" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "В обработке", "Processing" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Готов к получению", "ReadyForReceive" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Получен", "Received" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Отменен", "Cancelled" });

            migrationBuilder.UpdateData(
                table: "Ref_OrderStatuses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Возврат средств", "Refunded" });

            migrationBuilder.UpdateData(
                table: "Ref_PaymentMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Онлайн");

            migrationBuilder.UpdateData(
                table: "Ref_PaymentMethods",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Карта", "Card" });

            migrationBuilder.InsertData(
                table: "Ref_PaymentMethods",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 3, "Наличные", "Cash" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");
        }
    }
}
