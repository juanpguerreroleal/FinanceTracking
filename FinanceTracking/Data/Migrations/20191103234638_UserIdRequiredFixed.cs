using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceTracking.Data.Migrations
{
    public partial class UserIdRequiredFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_AspNetUsers_UserId",
                table: "Expenses");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Expenses",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_AspNetUsers_UserId",
                table: "Expenses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_AspNetUsers_UserId",
                table: "Expenses");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Expenses",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_AspNetUsers_UserId",
                table: "Expenses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
