using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estetica_Unicid.Migrations
{
    public partial class first : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClienteModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicoModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeServico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Valor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicoModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgendamentoModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServicoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendamentoModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendamentoModel_ClienteModel_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "ClienteModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgendamentoModel_ServicoModel_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "ServicoModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentoModel_ClienteId",
                table: "AgendamentoModel",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentoModel_ServicoId",
                table: "AgendamentoModel",
                column: "ServicoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendamentoModel");

            migrationBuilder.DropTable(
                name: "ClienteModel");

            migrationBuilder.DropTable(
                name: "ServicoModel");
        }
    }
}
