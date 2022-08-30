using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_ui.Data.Migrations
{
    public partial class Apparatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "Devices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DeviceDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApparatusData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Raw = table.Column<double>(type: "REAL", nullable: false),
                    ApparatusId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApparatusData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApparatusData_DeviceDevices_ApparatusId",
                        column: x => x.ApparatusId,
                        principalTable: "DeviceDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApparatusData_ApparatusId",
                table: "ApparatusData",
                column: "ApparatusId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDevices_DeviceId",
                table: "DeviceDevices",
                column: "DeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApparatusData");

            migrationBuilder.DropTable(
                name: "DeviceDevices");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "Devices");
        }
    }
}
