using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMatch.Migrations
{
    /// <inheritdoc />
    public partial class addCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmbienteIdeal",
                table: "Mascotas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CompatibleConNiños",
                table: "Mascotas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CompatibleConOtrasMascotas",
                table: "Mascotas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NivelCuidado",
                table: "Mascotas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NivelEnergia",
                table: "Mascotas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Raza",
                table: "Mascotas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequiereEjercicio",
                table: "Mascotas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Sociable",
                table: "Mascotas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PerfilMascotaIdeal",
                columns: table => new
                {
                    PerfilMascotaIdealId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TextoEstiloVida = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EtiquetaGenerada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TiposMascotaIdeales = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilMascotaIdeal", x => x.PerfilMascotaIdealId);
                });

            migrationBuilder.CreateTable(
                name: "MascotaIdeal",
                columns: table => new
                {
                    MascotaIdealId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilId = table.Column<int>(type: "int", nullable: false),
                    MascotaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MascotaIdeal", x => x.MascotaIdealId);
                    table.ForeignKey(
                        name: "FK_MascotaIdeal_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "MascotaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MascotaIdeal_PerfilMascotaIdeal_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "PerfilMascotaIdeal",
                        principalColumn: "PerfilMascotaIdealId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MascotaIdeal_MascotaId",
                table: "MascotaIdeal",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_MascotaIdeal_PerfilId",
                table: "MascotaIdeal",
                column: "PerfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MascotaIdeal");

            migrationBuilder.DropTable(
                name: "PerfilMascotaIdeal");

            migrationBuilder.DropColumn(
                name: "AmbienteIdeal",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "CompatibleConNiños",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "CompatibleConOtrasMascotas",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "NivelCuidado",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "NivelEnergia",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "Raza",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "RequiereEjercicio",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "Sociable",
                table: "Mascotas");
        }
    }
}
