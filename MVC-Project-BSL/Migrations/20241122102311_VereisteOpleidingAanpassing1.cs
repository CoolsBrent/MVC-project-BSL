using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Migrations
{
    /// <inheritdoc />
    public partial class VereisteOpleidingAanpassing1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpleidingVereist",
                table: "Opleidingen",
                newName: "OpleidingVereistId");

            migrationBuilder.CreateIndex(
                name: "IX_Opleidingen_OpleidingVereistId",
                table: "Opleidingen",
                column: "OpleidingVereistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereistId",
                table: "Opleidingen",
                column: "OpleidingVereistId",
                principalTable: "Opleidingen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereistId",
                table: "Opleidingen");

            migrationBuilder.DropIndex(
                name: "IX_Opleidingen_OpleidingVereistId",
                table: "Opleidingen");

            migrationBuilder.RenameColumn(
                name: "OpleidingVereistId",
                table: "Opleidingen",
                newName: "OpleidingVereist");
        }
    }
}
