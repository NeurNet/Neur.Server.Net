using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neur.Server.Net.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Settings_Name",
                table: "Settings",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Settings_Name",
                table: "Settings");
        }
    }
}
