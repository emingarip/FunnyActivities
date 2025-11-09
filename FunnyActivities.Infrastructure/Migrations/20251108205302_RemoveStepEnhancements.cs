using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStepEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<int>(
                name: "TimestampSeconds",
                table: "Steps",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TimestampSeconds",
                table: "Steps",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

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
        }
    }
}
