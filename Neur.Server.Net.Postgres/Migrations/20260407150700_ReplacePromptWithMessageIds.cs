using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neur.Server.Net.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePromptWithMessageIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"GenerationRequests\";");

            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "GenerationRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "PromptMessageId",
                table: "GenerationRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ResponseMessageId",
                table: "GenerationRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenerationRequests_PromptMessageId",
                table: "GenerationRequests",
                column: "PromptMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationRequests_ResponseMessageId",
                table: "GenerationRequests",
                column: "ResponseMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationRequests_Messages_PromptMessageId",
                table: "GenerationRequests",
                column: "PromptMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationRequests_Messages_ResponseMessageId",
                table: "GenerationRequests",
                column: "ResponseMessageId",
                principalTable: "Messages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GenerationRequests_Messages_PromptMessageId",
                table: "GenerationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GenerationRequests_Messages_ResponseMessageId",
                table: "GenerationRequests");

            migrationBuilder.DropIndex(
                name: "IX_GenerationRequests_PromptMessageId",
                table: "GenerationRequests");

            migrationBuilder.DropIndex(
                name: "IX_GenerationRequests_ResponseMessageId",
                table: "GenerationRequests");

            migrationBuilder.DropColumn(
                name: "PromptMessageId",
                table: "GenerationRequests");

            migrationBuilder.DropColumn(
                name: "ResponseMessageId",
                table: "GenerationRequests");

            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "GenerationRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
