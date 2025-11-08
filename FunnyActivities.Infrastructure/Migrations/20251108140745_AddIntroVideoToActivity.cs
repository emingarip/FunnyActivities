using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIntroVideoToActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntroVideoUrl",
                table: "Activities",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityUsers",
                columns: table => new
                {
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityUsers", x => new { x.ActivityId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ActivityUsers_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_IsPublic",
                table: "Activities",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_IsPublic_CreatedAt",
                table: "Activities",
                columns: new[] { "IsPublic", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityUsers_UserId",
                table: "ActivityUsers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityUsers");

            migrationBuilder.DropIndex(
                name: "IX_Activities_IsPublic",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_IsPublic_CreatedAt",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "IntroVideoUrl",
                table: "Activities");
        }
    }
}
