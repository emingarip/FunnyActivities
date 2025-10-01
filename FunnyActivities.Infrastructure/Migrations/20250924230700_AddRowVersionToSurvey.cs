using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Users_CreatedByUserId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_SurveyVotes_SurveyId_SurveyActivityId_UserId",
                table: "SurveyVotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SurveyVotes_VoteValue_Range",
                table: "SurveyVotes");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_IsActive",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_StartDate",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_Title",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_SurveyParticipants_SurveyId_UserId",
                table: "SurveyParticipants");

            migrationBuilder.DropIndex(
                name: "IX_SurveyActivities_SurveyId_Order",
                table: "SurveyActivities");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "SurveyVotes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Surveys",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Surveys",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Surveys",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "SurveyParticipants",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Users_CreatedByUserId",
                table: "Surveys",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Users_CreatedByUserId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Surveys");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "SurveyVotes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Surveys",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Surveys",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "SurveyParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyVotes_SurveyId_SurveyActivityId_UserId",
                table: "SurveyVotes",
                columns: new[] { "SurveyId", "SurveyActivityId", "UserId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SurveyVotes_VoteValue_Range",
                table: "SurveyVotes",
                sql: "VoteValue >= 1 AND VoteValue <= 5");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_IsActive",
                table: "Surveys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_StartDate",
                table: "Surveys",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_Title",
                table: "Surveys",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyParticipants_SurveyId_UserId",
                table: "SurveyParticipants",
                columns: new[] { "SurveyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyActivities_SurveyId_Order",
                table: "SurveyActivities",
                columns: new[] { "SurveyId", "Order" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Users_CreatedByUserId",
                table: "Surveys",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
