using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectNative.Migrations
{
    /// <inheritdoc />
    public partial class inittest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6ffae015-4c8a-4e05-aa76-21c0f8418b7a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9823994e-dd91-41ec-82c4-84408717da58");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Addresses",
                newName: "SubDistrict");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Addresses",
                newName: "Province");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "8910c767-c636-4c9b-97ec-d663b2680fd4", null, "Member", "MEMBER" },
                    { "e27734e7-cf8e-48fd-915a-752039ffae23", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8910c767-c636-4c9b-97ec-d663b2680fd4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e27734e7-cf8e-48fd-915a-752039ffae23");

            migrationBuilder.RenameColumn(
                name: "SubDistrict",
                table: "Addresses",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "Addresses",
                newName: "City");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6ffae015-4c8a-4e05-aa76-21c0f8418b7a", null, "Member", "MEMBER" },
                    { "9823994e-dd91-41ec-82c4-84408717da58", null, "Admin", "ADMIN" }
                });
        }
    }
}
