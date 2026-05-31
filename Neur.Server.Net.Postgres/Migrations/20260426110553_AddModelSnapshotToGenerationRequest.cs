using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neur.Server.Net.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddModelSnapshotToGenerationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "GenerationRequests",
                type: "Text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModelOllama",
                table: "GenerationRequests",
                type: "Text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "GenerationRequests");

            migrationBuilder.DropColumn(
                name: "ModelOllama",
                table: "GenerationRequests");
        }
    }
}
