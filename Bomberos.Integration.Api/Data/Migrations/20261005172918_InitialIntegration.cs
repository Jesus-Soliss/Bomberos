using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bomberos.Integration.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CacheRecurso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ExternalId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UltimaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheRecurso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Origen = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Metodo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Endpoint = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    Peticion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoRespuesta = table.Column<int>(type: "int", nullable: false),
                    Respuesta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AtendidoLocal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperacionPendiente",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "varchar(450)", unicode: false, maxLength: 450, nullable: true),
                    Tipo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Metodo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Endpoint = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    Cuerpo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenciaProvisional = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Reintentos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaAplicacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperacionPendiente", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CacheRecurso_Tipo_ExternalId",
                table: "CacheRecurso",
                columns: new[] { "Tipo", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationLog_FechaHora",
                table: "IntegrationLog",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_OperacionPendiente_Estado",
                table: "OperacionPendiente",
                column: "Estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CacheRecurso");

            migrationBuilder.DropTable(
                name: "IntegrationLog");

            migrationBuilder.DropTable(
                name: "OperacionPendiente");
        }
    }
}
