using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Data.Migrations
{
    /// <inheritdoc />
    public partial class Identityloginregister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractNummer",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true, // tijdelijk nullable
                defaultValue: null);

            migrationBuilder.AddColumn<DateTime>(
                name: "Geboortedatum",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Gemeente",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Huisdokter",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Huisnummer",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActief",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Naam",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Postcode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RekeningNummer",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Straat",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelefoonNummer",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Voornaam",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Activiteiten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activiteiten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bestemmingen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BestemmingsNaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinLeeftijd = table.Column<int>(type: "int", nullable: false),
                    MaxLeeftijd = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bestemmingen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kinderen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Voornaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Geboortedatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Allergieen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Medicatie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersoonId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kinderen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kinderen_AspNetUsers_PersoonId",
                        column: x => x.PersoonId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Monitoren",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsHoofdMonitor = table.Column<bool>(type: "bit", nullable: false),
                    PersoonId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monitoren", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Monitoren_AspNetUsers_PersoonId",
                        column: x => x.PersoonId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opleidingen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Begindatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Einddatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AantalPlaatsen = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opleidingen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BestemmingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fotos_Bestemmingen_BestemmingId",
                        column: x => x.BestemmingId,
                        principalTable: "Bestemmingen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groepsreizen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Begindatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Einddatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Prijs = table.Column<float>(type: "real", nullable: false),
                    BestemmingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groepsreizen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groepsreizen_Bestemmingen_BestemmingId",
                        column: x => x.BestemmingId,
                        principalTable: "Bestemmingen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpleidingPersonen",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OpleidingId = table.Column<int>(type: "int", nullable: false),
                    PersoonId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpleidingPersonen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpleidingPersonen_AspNetUsers_PersoonId",
                        column: x => x.PersoonId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpleidingPersonen_Opleidingen_OpleidingId",
                        column: x => x.OpleidingId,
                        principalTable: "Opleidingen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "Onkosten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Omschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bedrag = table.Column<float>(type: "real", nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Foto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroepsreisId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Onkosten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Onkosten_Groepsreizen_GroepsreisId",
                        column: x => x.GroepsreisId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiviteitGroepsreis_GroepsreizenId",
                table: "ActiviteitGroepsreis",
                column: "GroepsreizenId");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_BestemmingId",
                table: "Fotos",
                column: "BestemmingId");

            migrationBuilder.CreateIndex(
                name: "IX_GroepsreisKind_KinderenId",
                table: "GroepsreisKind",
                column: "KinderenId");

            migrationBuilder.CreateIndex(
                name: "IX_GroepsreisMonitor_MonitorenId",
                table: "GroepsreisMonitor",
                column: "MonitorenId");

            migrationBuilder.CreateIndex(
                name: "IX_Groepsreizen_BestemmingId",
                table: "Groepsreizen",
                column: "BestemmingId");

            migrationBuilder.CreateIndex(
                name: "IX_Kinderen_PersoonId",
                table: "Kinderen",
                column: "PersoonId");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoren_PersoonId",
                table: "Monitoren",
                column: "PersoonId");

            migrationBuilder.CreateIndex(
                name: "IX_Onkosten_GroepsreisId",
                table: "Onkosten",
                column: "GroepsreisId");

            migrationBuilder.CreateIndex(
                name: "IX_OpleidingPersonen_OpleidingId",
                table: "OpleidingPersonen",
                column: "OpleidingId");

            migrationBuilder.CreateIndex(
                name: "IX_OpleidingPersonen_PersoonId",
                table: "OpleidingPersonen",
                column: "PersoonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiviteitGroepsreis");

            migrationBuilder.DropTable(
                name: "Fotos");

            migrationBuilder.DropTable(
                name: "GroepsreisKind");

            migrationBuilder.DropTable(
                name: "GroepsreisMonitor");

            migrationBuilder.DropTable(
                name: "Onkosten");

            migrationBuilder.DropTable(
                name: "OpleidingPersonen");

            migrationBuilder.DropTable(
                name: "Activiteiten");

            migrationBuilder.DropTable(
                name: "Kinderen");

            migrationBuilder.DropTable(
                name: "Monitoren");

            migrationBuilder.DropTable(
                name: "Groepsreizen");

            migrationBuilder.DropTable(
                name: "Opleidingen");

            migrationBuilder.DropTable(
                name: "Bestemmingen");

            migrationBuilder.DropColumn(
                name: "ContractNummer",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Geboortedatum",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gemeente",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Huisdokter",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Huisnummer",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActief",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Naam",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RekeningNummer",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Straat",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TelefoonNummer",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Voornaam",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);
        }
    }
}
