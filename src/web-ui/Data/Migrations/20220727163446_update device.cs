using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_ui.Data.Migrations
{
    public partial class updatedevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeiveId",
                table: "Devices",
                newName: "DeviceId");

            migrationBuilder.AlterColumn<byte[]>(
                name: "IPAddress",
                table: "Devices",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "Devices",
                newName: "DeiveId");

            migrationBuilder.AlterColumn<int>(
                name: "IPAddress",
                table: "Devices",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");
        }
    }
}
