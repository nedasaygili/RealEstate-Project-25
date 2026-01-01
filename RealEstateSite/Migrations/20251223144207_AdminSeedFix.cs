using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateSite.Migrations
{
    /// <inheritdoc />
    public partial class AdminSeedFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Agents",
                columns: new[] { "Id", "Biography", "Email", "FirstName", "LastName", "Password", "PhoneNumber", "ProfileImageUrl", "Status", "Title" },
                values: new object[] { 1, "System Administrator Account.", "admin@unea.com", "System", "Admin", "123", "05550000000", "https://via.placeholder.com/150", true, "Administrator" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Agents",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
