using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neur.Server.Net.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class NullableChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Models_ModelId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_GenerationRequests_Messages_ResponseMessageId",
                table: "GenerationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GenerationRequests_Models_ModelId",
                table: "GenerationRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModelId",
                table: "GenerationRequests",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModelId",
                table: "Chats",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Models_ModelId",
                table: "Chats",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationRequests_Messages_ResponseMessageId",
                table: "GenerationRequests",
                column: "ResponseMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationRequests_Models_ModelId",
                table: "GenerationRequests",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Models_ModelId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_GenerationRequests_Messages_ResponseMessageId",
                table: "GenerationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GenerationRequests_Models_ModelId",
                table: "GenerationRequests");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModelId",
                table: "GenerationRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModelId",
                table: "Chats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Models_ModelId",
                table: "Chats",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationRequests_Messages_ResponseMessageId",
                table: "GenerationRequests",
                column: "ResponseMessageId",
                principalTable: "Messages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GenerationRequests_Models_ModelId",
                table: "GenerationRequests",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
