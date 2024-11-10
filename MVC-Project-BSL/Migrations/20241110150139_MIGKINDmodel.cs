using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Migrations
{
    /// <inheritdoc />
    public partial class MIGKINDmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergieën",
                table: "Kinderen");

            migrationBuilder.AlterColumn<string>(
                name: "Medicatie",
                table: "Kinderen",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Allergieen",
                table: "Kinderen",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergieen",
                table: "Kinderen");

            migrationBuilder.AlterColumn<string>(
                name: "Medicatie",
                table: "Kinderen",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Allergieën",
                table: "Kinderen",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
