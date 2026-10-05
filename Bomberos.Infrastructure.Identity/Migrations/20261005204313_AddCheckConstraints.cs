using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bomberos.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_Estado",
                table: "AspNetUsers",
                sql: "[Estado] IN ('ACTIVO', 'INACTIVO')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_Estado",
                table: "AspNetUsers");
        }
    }
}
