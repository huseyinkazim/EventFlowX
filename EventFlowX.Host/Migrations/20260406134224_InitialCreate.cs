using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventFlowX.Host.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pods",
                columns: table => new
                {
                    InstanceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HostName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastHeartbeat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pods", x => x.InstanceId);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutboxEvent_EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProcessingBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxEvents_Pods_ProcessingBy",
                        column: x => x.ProcessingBy,
                        principalTable: "Pods",
                        principalColumn: "InstanceId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_ProcessingBy",
                table: "OutboxEvents",
                column: "ProcessingBy");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_Status",
                table: "OutboxEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Pods_Status",
                table: "Pods",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxEvents");

            migrationBuilder.DropTable(
                name: "Pods");
        }
    }
}
