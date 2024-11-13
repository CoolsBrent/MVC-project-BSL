using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Migrations
{
    /// <inheritdoc />
    public partial class OpleidingvereistNullAllowed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereist",
                table: "Opleidingen");

            migrationBuilder.AlterColumn<int>(
                name: "OpleidingVereist",
                table: "Opleidingen",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereist",
                table: "Opleidingen",
                column: "OpleidingVereist",
                principalTable: "Opleidingen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereist",
                table: "Opleidingen");

            migrationBuilder.AlterColumn<int>(
                name: "OpleidingVereist",
                table: "Opleidingen",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
