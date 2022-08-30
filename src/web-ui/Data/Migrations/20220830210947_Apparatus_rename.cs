using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_ui.Data.Migrations
{
    public partial class Apparatus_rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApparatusData_DeviceDevices_ApparatusId",
                table: "ApparatusData");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDevices_Devices_DeviceId",
                table: "DeviceDevices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceDevices",
                table: "DeviceDevices");

            migrationBuilder.RenameTable(
                name: "DeviceDevices",
                newName: "DeviceApparatus");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceDevices_DeviceId",
                table: "DeviceApparatus",
                newName: "IX_DeviceApparatus_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceApparatus",
                table: "DeviceApparatus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApparatusData_DeviceApparatus_ApparatusId",
                table: "ApparatusData",
                column: "ApparatusId",
                principalTable: "DeviceApparatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceApparatus_Devices_DeviceId",
                table: "DeviceApparatus",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApparatusData_DeviceApparatus_ApparatusId",
                table: "ApparatusData");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceApparatus_Devices_DeviceId",
                table: "DeviceApparatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceApparatus",
                table: "DeviceApparatus");

            migrationBuilder.RenameTable(
                name: "DeviceApparatus",
                newName: "DeviceDevices");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceApparatus_DeviceId",
                table: "DeviceDevices",
                newName: "IX_DeviceDevices_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceDevices",
                table: "DeviceDevices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApparatusData_DeviceDevices_ApparatusId",
                table: "ApparatusData",
                column: "ApparatusId",
                principalTable: "DeviceDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDevices_Devices_DeviceId",
                table: "DeviceDevices",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
