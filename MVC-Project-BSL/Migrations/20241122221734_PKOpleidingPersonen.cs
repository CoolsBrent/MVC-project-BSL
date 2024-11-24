using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_Project_BSL.Migrations
{
    /// <inheritdoc />
    public partial class PKOpleidingPersonen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Drop Primary Key
			migrationBuilder.DropPrimaryKey(
				name: "PK_OpleidingPersonen",
				table: "OpleidingPersonen");

			// Drop the Id column
			migrationBuilder.DropColumn(
				name: "Id",
				table: "OpleidingPersonen");

			// Recreate the Id column with the IDENTITY property
			migrationBuilder.AddColumn<int>(
				name: "Id",
				table: "OpleidingPersonen",
				type: "int",
				nullable: false,
				defaultValue: 0)
				.Annotation("SqlServer:Identity", "1, 1"); // Add IDENTITY here

			// Add Primary Key on the new Id column
			migrationBuilder.AddPrimaryKey(
				name: "PK_OpleidingPersonen",
				table: "OpleidingPersonen",
				column: "Id");

			// Add index for OpleidingId to improve query performance
			migrationBuilder.CreateIndex(
				name: "IX_OpleidingPersonen_OpleidingId",
				table: "OpleidingPersonen",
				column: "OpleidingId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OpleidingPersonen",
                table: "OpleidingPersonen");

            migrationBuilder.DropIndex(
                name: "IX_OpleidingPersonen_OpleidingId",
                table: "OpleidingPersonen");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "OpleidingPersonen",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpleidingPersonen",
                table: "OpleidingPersonen",
                columns: new[] { "OpleidingId", "PersoonId" });
        }
    }
}
