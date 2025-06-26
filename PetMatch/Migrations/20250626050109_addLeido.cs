using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMatch.Migrations
{
    /// <inheritdoc />
    public partial class addLeido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AddColumn<bool>(
                name: "Leido",
                table: "Mensajes",
                type: "bit",
                nullable: false,
                defaultValue: false);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Leido",
                table: "Mensajes");
        }
    }
}
