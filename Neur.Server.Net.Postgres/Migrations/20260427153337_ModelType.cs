using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neur.Server.Net.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ModelType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Text");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Models",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Text");

            migrationBuilder.AlterColumn<string>(
                name: "ModelOllama",
                table: "GenerationRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Text");

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "GenerationRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "Text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "Text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Models",
                type: "Text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "Text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ModelOllama",
                table: "GenerationRequests",
                type: "Text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "GenerationRequests",
                type: "Text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
