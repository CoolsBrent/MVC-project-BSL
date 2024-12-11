using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Migrations
{
    /// <inheritdoc />
    public partial class groepsreisUpdateMaxDeelnemers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAantalDeelnemers",
                table: "Groepsreizen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroepsreisId",
                table: "Deelnemers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deelnemers_GroepsreisId",
                table: "Deelnemers",
                column: "GroepsreisId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deelnemers_Groepsreizen_GroepsreisId",
                table: "Deelnemers",
                column: "GroepsreisId",
                principalTable: "Groepsreizen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deelnemers_Groepsreizen_GroepsreisId",
                table: "Deelnemers");

            migrationBuilder.DropIndex(
                name: "IX_Deelnemers_GroepsreisId",
                table: "Deelnemers");

            migrationBuilder.DropColumn(
                name: "MaxAantalDeelnemers",
                table: "Groepsreizen");

            migrationBuilder.DropColumn(
                name: "GroepsreisId",
                table: "Deelnemers");
        }
    }
}
