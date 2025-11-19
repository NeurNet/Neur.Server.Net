using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neur.Server.Net.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Context",
                table: "Chats");

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "Models",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Context",
                table: "Models",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Context",
                table: "Models");

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "Models",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "Context",
                table: "Chats",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
