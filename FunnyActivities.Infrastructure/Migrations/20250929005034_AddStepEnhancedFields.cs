using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStepEnhancedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "SurveyVotes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Steps",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaAttachments",
                table: "Steps",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PauseTimeSeconds",
                table: "Steps",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimestampSeconds",
                table: "Steps",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "MediaAttachments",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "PauseTimeSeconds",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "TimestampSeconds",
                table: "Steps");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "SurveyVotes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
