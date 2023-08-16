using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectNative.Migrations
{
    /// <inheritdoc />
    public partial class test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "379acb86-e3ea-4012-87cf-84ae0b275420");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "44be81a2-c863-4883-84e0-3b29676d59bd");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "c2f2bd7f-09d6-4c89-9683-08185800cc09", null, "Admin", "ADMIN" },
                    { "ec69415e-d6b0-4a90-b1ce-3aa6a018a845", null, "Member", "MEMBER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2f2bd7f-09d6-4c89-9683-08185800cc09");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ec69415e-d6b0-4a90-b1ce-3aa6a018a845");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "379acb86-e3ea-4012-87cf-84ae0b275420", null, "Member", "MEMBER" },
                    { "44be81a2-c863-4883-84e0-3b29676d59bd", null, "Admin", "ADMIN" }
                });
        }
    }
}
