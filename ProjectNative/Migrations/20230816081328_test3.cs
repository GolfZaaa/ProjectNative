using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectNative.Migrations
{
    /// <inheritdoc />
    public partial class test3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2f2bd7f-09d6-4c89-9683-08185800cc09");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ec69415e-d6b0-4a90-b1ce-3aa6a018a845");

            migrationBuilder.AddColumn<string>(
                name: "OrderImage",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5afad4fc-1dac-425e-89b4-e73c7a5d8dee", null, "Admin", "ADMIN" },
                    { "c093d428-8cc3-4c67-ad99-30db870f9081", null, "Member", "MEMBER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5afad4fc-1dac-425e-89b4-e73c7a5d8dee");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c093d428-8cc3-4c67-ad99-30db870f9081");

            migrationBuilder.DropColumn(
                name: "OrderImage",
                table: "Orders");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "c2f2bd7f-09d6-4c89-9683-08185800cc09", null, "Admin", "ADMIN" },
                    { "ec69415e-d6b0-4a90-b1ce-3aa6a018a845", null, "Member", "MEMBER" }
                });
        }
    }
}
