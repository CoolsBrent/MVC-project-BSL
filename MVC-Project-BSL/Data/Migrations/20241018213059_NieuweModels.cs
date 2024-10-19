using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Data.Migrations
{
    /// <inheritdoc />
    public partial class NieuweModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiviteitGroepsreis");

            migrationBuilder.DropTable(
                name: "GroepsreisKind");

            migrationBuilder.DropTable(
                name: "GroepsreisMonitor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpleidingPersonen",
                table: "OpleidingPersonen");

            migrationBuilder.DropIndex(
                name: "IX_OpleidingPersonen_OpleidingId",
                table: "OpleidingPersonen");

            migrationBuilder.RenameColumn(
                name: "Allergieen",
                table: "Kinderen",
                newName: "Allergieën");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "OpleidingPersonen",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "OpleidingVereist",
                table: "Opleidingen",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpleidingPersonen",
                table: "OpleidingPersonen",
                columns: new[] { "OpleidingId", "PersoonId" });

            migrationBuilder.CreateTable(
                name: "Deelnemers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KindId = table.Column<int>(type: "int", nullable: false),
                    GroepsreisDetailId = table.Column<int>(type: "int", nullable: false),
                    Opmerkingen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewScore = table.Column<int>(type: "int", nullable: false),
                    Review = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deelnemers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deelnemers_Groepsreizen_GroepsreisDetailId",
                        column: x => x.GroepsreisDetailId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deelnemers_Kinderen_KindId",
                        column: x => x.KindId,
                        principalTable: "Kinderen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Programmas",
                columns: table => new
                {
                    ActiviteitId = table.Column<int>(type: "int", nullable: false),
                    GroepsreisId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programmas", x => new { x.ActiviteitId, x.GroepsreisId });
                    table.ForeignKey(
                        name: "FK_Programmas_Activiteiten_ActiviteitId",
                        column: x => x.ActiviteitId,
                        principalTable: "Activiteiten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Programmas_Groepsreizen_GroepsreisId",
                        column: x => x.GroepsreisId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opleidingen_OpleidingVereist",
                table: "Opleidingen",
                column: "OpleidingVereist");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoren_GroepsreisDetailId",
                table: "Monitoren",
                column: "GroepsreisDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoren_GroepsreisId",
                table: "Monitoren",
                column: "GroepsreisId");

            migrationBuilder.CreateIndex(
                name: "IX_Deelnemers_GroepsreisDetailId",
                table: "Deelnemers",
                column: "GroepsreisDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Deelnemers_KindId",
                table: "Deelnemers",
                column: "KindId");

            migrationBuilder.CreateIndex(
                name: "IX_Programmas_GroepsreisId",
                table: "Programmas",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereist",
                table: "Opleidingen",
                column: "OpleidingVereist",
                principalTable: "Opleidingen",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Monitoren_Groepsreizen_GroepsreisDetailId",
                table: "Monitoren");

            migrationBuilder.DropForeignKey(
                name: "FK_Monitoren_Groepsreizen_GroepsreisId",
                table: "Monitoren");

            migrationBuilder.DropForeignKey(
                name: "FK_Opleidingen_Opleidingen_OpleidingVereist",
                table: "Opleidingen");

            migrationBuilder.DropTable(
                name: "Deelnemers");

            migrationBuilder.DropTable(
                name: "Programmas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpleidingPersonen",
                table: "OpleidingPersonen");

            migrationBuilder.DropIndex(
                name: "IX_Opleidingen_OpleidingVereist",
                table: "Opleidingen");

            migrationBuilder.DropIndex(
                name: "IX_Monitoren_GroepsreisDetailId",
                table: "Monitoren");

            migrationBuilder.DropIndex(
                name: "IX_Monitoren_GroepsreisId",
                table: "Monitoren");

            migrationBuilder.DropColumn(
                name: "OpleidingVereist",
                table: "Opleidingen");

            migrationBuilder.DropColumn(
                name: "GroepsreisDetailId",
                table: "Monitoren");

            migrationBuilder.DropColumn(
                name: "GroepsreisId",
                table: "Monitoren");

            migrationBuilder.RenameColumn(
                name: "Allergieën",
                table: "Kinderen",
                newName: "Allergieen");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "OpleidingPersonen",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpleidingPersonen",
                table: "OpleidingPersonen",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ActiviteitGroepsreis",
                columns: table => new
                {
                    ActiviteitenId = table.Column<int>(type: "int", nullable: false),
                    GroepsreizenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiviteitGroepsreis", x => new { x.ActiviteitenId, x.GroepsreizenId });
                    table.ForeignKey(
                        name: "FK_ActiviteitGroepsreis_Activiteiten_ActiviteitenId",
                        column: x => x.ActiviteitenId,
                        principalTable: "Activiteiten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiviteitGroepsreis_Groepsreizen_GroepsreizenId",
                        column: x => x.GroepsreizenId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroepsreisKind",
                columns: table => new
                {
                    GroepsreizenId = table.Column<int>(type: "int", nullable: false),
                    KinderenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroepsreisKind", x => new { x.GroepsreizenId, x.KinderenId });
                    table.ForeignKey(
                        name: "FK_GroepsreisKind_Groepsreizen_GroepsreizenId",
                        column: x => x.GroepsreizenId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroepsreisKind_Kinderen_KinderenId",
                        column: x => x.KinderenId,
                        principalTable: "Kinderen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroepsreisMonitor",
                columns: table => new
                {
                    GroepsreizenId = table.Column<int>(type: "int", nullable: false),
                    MonitorenId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroepsreisMonitor", x => new { x.GroepsreizenId, x.MonitorenId });
                    table.ForeignKey(
                        name: "FK_GroepsreisMonitor_Groepsreizen_GroepsreizenId",
                        column: x => x.GroepsreizenId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroepsreisMonitor_Monitoren_MonitorenId",
                        column: x => x.MonitorenId,
                        principalTable: "Monitoren",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpleidingPersonen_OpleidingId",
                table: "OpleidingPersonen",
                column: "OpleidingId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiviteitGroepsreis_GroepsreizenId",
                table: "ActiviteitGroepsreis",
                column: "GroepsreizenId");

            migrationBuilder.CreateIndex(
                name: "IX_GroepsreisKind_KinderenId",
                table: "GroepsreisKind",
                column: "KinderenId");

            migrationBuilder.CreateIndex(
                name: "IX_GroepsreisMonitor_MonitorenId",
                table: "GroepsreisMonitor",
                column: "MonitorenId");
        }
    }
}
