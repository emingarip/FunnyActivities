using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyParticipantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyParticipants_Users_UserId",
                table: "SurveyParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyVotes_Users_UserId",
                table: "SurveyVotes");

            migrationBuilder.DropIndex(
                name: "IX_SurveyParticipants_UserId",
                table: "SurveyParticipants");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SurveyParticipants");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SurveyVotes",
                newName: "SurveyParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyVotes_UserId",
                table: "SurveyVotes",
                newName: "IX_SurveyVotes_SurveyParticipantId");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "SurveyVotes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "ChildrenCount",
                table: "SurveyParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "SurveyParticipants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "SurveyParticipants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyVotes_SurveyParticipants_SurveyParticipantId",
                table: "SurveyVotes",
                column: "SurveyParticipantId",
                principalTable: "SurveyParticipants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyVotes_SurveyParticipants_SurveyParticipantId",
                table: "SurveyVotes");

            migrationBuilder.DropColumn(
                name: "ChildrenCount",
                table: "SurveyParticipants");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "SurveyParticipants");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "SurveyParticipants");

            migrationBuilder.RenameColumn(
                name: "SurveyParticipantId",
                table: "SurveyVotes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyVotes_SurveyParticipantId",
                table: "SurveyVotes",
                newName: "IX_SurveyVotes_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "SurveyVotes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "SurveyParticipants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SurveyParticipants_UserId",
                table: "SurveyParticipants",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyParticipants_Users_UserId",
                table: "SurveyParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyVotes_Users_UserId",
                table: "SurveyVotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
