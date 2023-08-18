using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectNative.Migrations
{
    /// <inheritdoc />
    public partial class Star : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "05b9f3c5-a346-4743-b024-8d5f3e771491");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6e00c980-15c8-456f-9bdf-412c2e37a5a6");

            migrationBuilder.AddColumn<int>(
                name: "Star",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2a1894d6-a118-4769-9935-2294f759aa8a", null, "Member", "MEMBER" },
                    { "f297c161-aef3-47b9-8edf-65947f46d0d0", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2a1894d6-a118-4769-9935-2294f759aa8a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f297c161-aef3-47b9-8edf-65947f46d0d0");

            migrationBuilder.DropColumn(
                name: "Star",
                table: "Reviews");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "05b9f3c5-a346-4743-b024-8d5f3e771491", null, "Member", "MEMBER" },
                    { "6e00c980-15c8-456f-9bdf-412c2e37a5a6", null, "Admin", "ADMIN" }
                });
        }
    }
}
