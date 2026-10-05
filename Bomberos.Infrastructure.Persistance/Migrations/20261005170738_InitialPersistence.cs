using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bomberos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true),
                    Accion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Entidad = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resultado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emergencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    Direccion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Prioridad = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Estado = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Origen = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ReferenciaExterna = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emergencia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Estado = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true),
                    Metodo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Endpoint = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    Peticion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoRespuesta = table.Column<int>(type: "int", nullable: false),
                    Respuesta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuracionMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bombero",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Apellido = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Cedula = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Rango = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    EstacionId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bombero", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bombero_Estacion_EstacionId",
                        column: x => x.EstacionId,
                        principalTable: "Estacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Placa = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Tipo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    EstacionId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehiculo_Estacion_EstacionId",
                        column: x => x.EstacionId,
                        principalTable: "Estacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionBombero",
                columns: table => new
                {
                    EmergenciaId = table.Column<int>(type: "int", nullable: false),
                    BomberoId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionBombero", x => new { x.EmergenciaId, x.BomberoId });
                    table.ForeignKey(
                        name: "FK_AsignacionBombero_Bombero_BomberoId",
                        column: x => x.BomberoId,
                        principalTable: "Bombero",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionBombero_Emergencia_EmergenciaId",
                        column: x => x.EmergenciaId,
                        principalTable: "Emergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionVehiculo",
                columns: table => new
                {
                    EmergenciaId = table.Column<int>(type: "int", nullable: false),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionVehiculo", x => new { x.EmergenciaId, x.VehiculoId });
                    table.ForeignKey(
                        name: "FK_AsignacionVehiculo_Emergencia_EmergenciaId",
                        column: x => x.EmergenciaId,
                        principalTable: "Emergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionVehiculo_Vehiculo_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_AsignacionBombero_Activa",
                table: "AsignacionBombero",
                column: "BomberoId",
                unique: true,
                filter: "[Estado] = 'ACTIVA'");

            migrationBuilder.CreateIndex(
                name: "UX_AsignacionVehiculo_Activa",
                table: "AsignacionVehiculo",
                column: "VehiculoId",
                unique: true,
                filter: "[Estado] = 'ACTIVA'");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_FechaHora",
                table: "AuditLog",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Bombero_Cedula",
                table: "Bombero",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bombero_EstacionId",
                table: "Bombero",
                column: "EstacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Emergencia_ReferenciaExterna",
                table: "Emergencia",
                column: "ReferenciaExterna",
                unique: true,
                filter: "[ReferenciaExterna] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Estacion_Nombre",
                table: "Estacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestLog_FechaHora",
                table: "RequestLog",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_EstacionId",
                table: "Vehiculo",
                column: "EstacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_Placa",
                table: "Vehiculo",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionBombero");

            migrationBuilder.DropTable(
                name: "AsignacionVehiculo");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "RequestLog");

            migrationBuilder.DropTable(
                name: "Bombero");

            migrationBuilder.DropTable(
                name: "Emergencia");

            migrationBuilder.DropTable(
                name: "Vehiculo");

            migrationBuilder.DropTable(
                name: "Estacion");
        }
    }
}
