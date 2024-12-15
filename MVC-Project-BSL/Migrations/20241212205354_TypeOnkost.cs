using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Migrations
{
    /// <inheritdoc />
    public partial class TypeOnkost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TypeOnkost",
                table: "Onkosten",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeOnkost",
                table: "Onkosten");
        }
    }
}
