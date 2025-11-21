using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLlmSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LlmSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DefaultProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OllamaBaseUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OllamaHealthCheckModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OllamaPreferredModelsJson = table.Column<string>(type: "text", nullable: false),
                    OpenAiBaseUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OpenAiDefaultModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OpenAiAllowedModelsJson = table.Column<string>(type: "text", nullable: false),
                    OpenAiOrganizationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OpenAiApiKey = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ModelCacheSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 300),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LlmSettings");
        }
    }
}
