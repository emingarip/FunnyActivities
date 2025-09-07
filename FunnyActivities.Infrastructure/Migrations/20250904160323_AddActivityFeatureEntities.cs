using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityFeatureEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VideoUrl = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ActivityCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_ActivityCategories_ActivityCategoryId",
                        column: x => x.ActivityCategoryId,
                        principalTable: "ActivityCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityProductVariants_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityProductVariants_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityProductVariants_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ActivityCategoryId",
                table: "Activities",
                column: "ActivityCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CreatedAt",
                table: "Activities",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Name",
                table: "Activities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCategories_CreatedAt",
                table: "ActivityCategories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCategories_Name",
                table: "ActivityCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProductVariants_ActivityId",
                table: "ActivityProductVariants",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProductVariants_ProductVariantId",
                table: "ActivityProductVariants",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProductVariants_UnitOfMeasureId",
                table: "ActivityProductVariants",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ActivityId",
                table: "Steps",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ActivityId_Order",
                table: "Steps",
                columns: new[] { "ActivityId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityProductVariants");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "ActivityCategories");
        }
    }
}
