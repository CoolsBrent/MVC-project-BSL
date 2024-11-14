using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Migrations
{
    /// <inheritdoc />
    public partial class aanpassingOplPers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereist",
                table: "Opleidingen");

            migrationBuilder.DropIndex(
                name: "IX_Opleidingen_OpleidingVereist",
                table: "Opleidingen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Opleidingen_OpleidingVereist",
                table: "Opleidingen",
                column: "OpleidingVereist");

            migrationBuilder.AddForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereist",
                table: "Opleidingen",
                column: "OpleidingVereist",
                principalTable: "Opleidingen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
