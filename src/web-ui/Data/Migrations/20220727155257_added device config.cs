using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ajiva_home_automation.web_ui.Data.Migrations
{
    public partial class addeddeviceconfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceConfig_DeviceConfigId",
                table: "Devices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceConfig",
                table: "DeviceConfig");

            migrationBuilder.RenameTable(
                name: "DeviceConfig",
                newName: "DeviceConfigs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceConfigs",
                table: "DeviceConfigs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceConfigs_DeviceConfigId",
                table: "Devices",
                column: "DeviceConfigId",
                principalTable: "DeviceConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceConfigs_DeviceConfigId",
                table: "Devices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceConfigs",
                table: "DeviceConfigs");

            migrationBuilder.RenameTable(
                name: "DeviceConfigs",
                newName: "DeviceConfig");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceConfig",
                table: "DeviceConfig",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceConfig_DeviceConfigId",
                table: "Devices",
                column: "DeviceConfigId",
                principalTable: "DeviceConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
