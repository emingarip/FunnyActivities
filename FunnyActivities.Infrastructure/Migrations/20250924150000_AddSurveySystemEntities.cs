using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveySystemEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Surveys table
            migrationBuilder.CreateTable(
                name: "Surveys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Surveys_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create SurveyActivities table
            migrationBuilder.CreateTable(
                name: "SurveyActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyActivities_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyActivities_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create SurveyParticipants table
            migrationBuilder.CreateTable(
                name: "SurveyParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyParticipants_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create SurveyVotes table
            migrationBuilder.CreateTable(
                name: "SurveyVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoteValue = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyVotes_SurveyActivities_SurveyActivityId",
                        column: x => x.SurveyActivityId,
                        principalTable: "SurveyActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyVotes_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for better performance
            migrationBuilder.CreateIndex(
                name: "IX_Surveys_CreatedByUserId",
                table: "Surveys",
                column: "CreatedByUserId");

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
                name: "IX_SurveyActivities_ActivityId",
                table: "SurveyActivities",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyActivities_SurveyId",
                table: "SurveyActivities",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyActivities_SurveyId_Order",
                table: "SurveyActivities",
                columns: new[] { "SurveyId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyParticipants_SurveyId",
                table: "SurveyParticipants",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyParticipants_UserId",
                table: "SurveyParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyParticipants_SurveyId_UserId",
                table: "SurveyParticipants",
                columns: new[] { "SurveyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyVotes_SurveyActivityId",
                table: "SurveyVotes",
                column: "SurveyActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyVotes_SurveyId",
                table: "SurveyVotes",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyVotes_UserId",
                table: "SurveyVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyVotes_SurveyId_SurveyActivityId_UserId",
                table: "SurveyVotes",
                columns: new[] { "SurveyId", "SurveyActivityId", "UserId" },
                unique: true);

            // Add check constraints for vote values (1-5 scale)
            migrationBuilder.AddCheckConstraint(
                name: "CK_SurveyVotes_VoteValue_Range",
                table: "SurveyVotes",
                sql: "\"VoteValue\" >= 1 AND \"VoteValue\" <= 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order (respecting foreign key dependencies)
            migrationBuilder.DropTable(
                name: "SurveyVotes");

            migrationBuilder.DropTable(
                name: "SurveyParticipants");

            migrationBuilder.DropTable(
                name: "SurveyActivities");

            migrationBuilder.DropTable(
                name: "Surveys");
        }
    }
}