using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bomberos.Integration.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_OperacionPendiente_Estado",
                table: "OperacionPendiente",
                sql: "[Estado] IN ('PENDIENTE', 'APLICADA', 'RECHAZADA')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OperacionPendiente_Tipo",
                table: "OperacionPendiente",
                sql: "[Tipo] IN ('CREAR_EMERGENCIA', 'ASIGNAR_BOMBERO', 'ASIGNAR_VEHICULO', 'FINALIZAR_ASIGNACION', 'REPORTE_911')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CacheRecurso_Estado",
                table: "CacheRecurso",
                sql: "[Estado] IN ('DISPONIBLE', 'EN_SERVICIO', 'RESERVADO', 'NO_DISPONIBLE')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CacheRecurso_Tipo",
                table: "CacheRecurso",
                sql: "[Tipo] IN ('BOMBERO', 'VEHICULO')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_OperacionPendiente_Estado",
                table: "OperacionPendiente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OperacionPendiente_Tipo",
                table: "OperacionPendiente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CacheRecurso_Estado",
                table: "CacheRecurso");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CacheRecurso_Tipo",
                table: "CacheRecurso");
        }
    }
}
