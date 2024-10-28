using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Data.Migrations
{
    /// <inheritdoc />
    public partial class GroepsreisMonitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Monitoren_Groepsreizen_GroepsreisDetailId",
                table: "Monitoren");

            migrationBuilder.DropForeignKey(
                name: "FK_Monitoren_Groepsreizen_GroepsreisId",
                table: "Monitoren");

            migrationBuilder.DropIndex(
                name: "IX_Monitoren_GroepsreisDetailId",
                table: "Monitoren");

            migrationBuilder.DropIndex(
                name: "IX_Monitoren_GroepsreisId",
                table: "Monitoren");

            migrationBuilder.DropColumn(
                name: "GroepsreisDetailId",
                table: "Monitoren");

            migrationBuilder.DropColumn(
                name: "GroepsreisId",
                table: "Monitoren");

            migrationBuilder.CreateTable(
                name: "GroepsreisMonitor",
                columns: table => new
                {
                    GroepsreisId = table.Column<int>(type: "int", nullable: false),
                    MonitorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroepsreisMonitor", x => new { x.GroepsreisId, x.MonitorId });
                    table.ForeignKey(
                        name: "FK_GroepsreisMonitor_Groepsreizen_GroepsreisId",
                        column: x => x.GroepsreisId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroepsreisMonitor_Monitoren_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "Monitoren",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroepsreisMonitor_MonitorId",
                table: "GroepsreisMonitor",
                column: "MonitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroepsreisMonitor");

            migrationBuilder.AddColumn<int>(
                name: "GroepsreisDetailId",
                table: "Monitoren",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroepsreisId",
                table: "Monitoren",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Monitoren_GroepsreisDetailId",
                table: "Monitoren",
                column: "GroepsreisDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoren_GroepsreisId",
                table: "Monitoren",
                column: "GroepsreisId");

            migrationBuilder.AddForeignKey(
                name: "FK_Monitoren_Groepsreizen_GroepsreisDetailId",
                table: "Monitoren",
                column: "GroepsreisDetailId",
                principalTable: "Groepsreizen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Monitoren_Groepsreizen_GroepsreisId",
                table: "Monitoren",
                column: "GroepsreisId",
                principalTable: "Groepsreizen",
                principalColumn: "Id");
        }
    }
}
