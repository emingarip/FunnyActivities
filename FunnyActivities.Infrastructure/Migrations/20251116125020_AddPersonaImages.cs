using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunnyActivities.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonaImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonaId",
                table: "Images",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_PersonaId",
                table: "Images",
                column: "PersonaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Personas_PersonaId",
                table: "Images",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Personas_PersonaId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_PersonaId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "PersonaId",
                table: "Images");
        }
    }
}
