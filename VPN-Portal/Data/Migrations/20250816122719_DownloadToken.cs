using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class DownloadToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NetworkAddress",
                table: "VpnServers",
                newName: "ServerAddress");

            migrationBuilder.AddColumn<string>(
                name: "DnsPoolId",
                table: "VpnServers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DownloadTokens",
                columns: table => new
                {
                    DownloadTokenId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    ConfigPath = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OneTime = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTokens", x => x.DownloadTokenId);
                    table.ForeignKey(
                        name: "FK_DownloadTokens_DevicePeers_PeerId",
                        column: x => x.PeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VpnServers_DnsPoolId",
                table: "VpnServers",
                column: "DnsPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTokens_ExpiresUtc_UsedUtc",
                table: "DownloadTokens",
                columns: new[] { "ExpiresUtc", "UsedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTokens_PeerId",
                table: "DownloadTokens",
                column: "PeerId");

            migrationBuilder.AddForeignKey(
                name: "FK_VpnServers_DnsPools_DnsPoolId",
                table: "VpnServers",
                column: "DnsPoolId",
                principalTable: "DnsPools",
                principalColumn: "DnsPoolId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VpnServers_DnsPools_DnsPoolId",
                table: "VpnServers");

            migrationBuilder.DropTable(
                name: "DownloadTokens");

            migrationBuilder.DropIndex(
                name: "IX_VpnServers_DnsPoolId",
                table: "VpnServers");

            migrationBuilder.DropColumn(
                name: "DnsPoolId",
                table: "VpnServers");

            migrationBuilder.RenameColumn(
                name: "ServerAddress",
                table: "VpnServers",
                newName: "NetworkAddress");
        }
    }
}
