using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bomberos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehiculo_Estado",
                table: "Vehiculo",
                sql: "[Estado] IN ('DISPONIBLE', 'EN_SERVICIO', 'MANTENIMIENTO', 'FUERA_DE_SERVICIO')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vehiculo_Tipo",
                table: "Vehiculo",
                sql: "[Tipo] IN ('CAMION', 'AMBULANCIA', 'RESCATE', 'TANQUERO')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Estacion_Estado",
                table: "Estacion",
                sql: "[Estado] IN ('ACTIVA', 'INACTIVA')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Emergencia_Estado",
                table: "Emergencia",
                sql: "[Estado] IN ('PENDIENTE', 'EN_ATENCION', 'CONTROLADA', 'FINALIZADA', 'CANCELADA')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Emergencia_Origen",
                table: "Emergencia",
                sql: "[Origen] IN ('OPERADOR', 'CENTRAL_911')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Emergencia_Prioridad",
                table: "Emergencia",
                sql: "[Prioridad] IN ('BAJA', 'MEDIA', 'ALTA', 'CRITICA')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Emergencia_Tipo",
                table: "Emergencia",
                sql: "[Tipo] IN ('INCENDIO', 'ACCIDENTE', 'RESCATE', 'INCIDENTE_PELIGROSO', 'OTRO')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bombero_Estado",
                table: "Bombero",
                sql: "[Estado] IN ('ACTIVO', 'INACTIVO')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bombero_Rango",
                table: "Bombero",
                sql: "[Rango] IN ('BOMBERO', 'CABO', 'SARGENTO', 'TENIENTE', 'CAPITAN')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AuditLog_Resultado",
                table: "AuditLog",
                sql: "[Resultado] IN ('EXITOSO', 'FALLIDO')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AsignacionVehiculo_Estado",
                table: "AsignacionVehiculo",
                sql: "[Estado] IN ('ACTIVA', 'FINALIZADA', 'CANCELADA')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AsignacionVehiculo_FechaFinalizacion",
                table: "AsignacionVehiculo",
                sql: "[Estado] <> 'FINALIZADA' OR [FechaFinalizacion] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AsignacionBombero_Estado",
                table: "AsignacionBombero",
                sql: "[Estado] IN ('ACTIVA', 'FINALIZADA', 'CANCELADA')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AsignacionBombero_FechaFinalizacion",
                table: "AsignacionBombero",
                sql: "[Estado] <> 'FINALIZADA' OR [FechaFinalizacion] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehiculo_Estado",
                table: "Vehiculo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Vehiculo_Tipo",
                table: "Vehiculo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Estacion_Estado",
                table: "Estacion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Emergencia_Estado",
                table: "Emergencia");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Emergencia_Origen",
                table: "Emergencia");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Emergencia_Prioridad",
                table: "Emergencia");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Emergencia_Tipo",
                table: "Emergencia");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bombero_Estado",
                table: "Bombero");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bombero_Rango",
                table: "Bombero");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AuditLog_Resultado",
                table: "AuditLog");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AsignacionVehiculo_Estado",
                table: "AsignacionVehiculo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AsignacionVehiculo_FechaFinalizacion",
                table: "AsignacionVehiculo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AsignacionBombero_Estado",
                table: "AsignacionBombero");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AsignacionBombero_FechaFinalizacion",
                table: "AsignacionBombero");
        }
    }
}
