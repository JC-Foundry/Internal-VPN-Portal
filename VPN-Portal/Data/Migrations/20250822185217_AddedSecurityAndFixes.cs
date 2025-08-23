using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedSecurityAndFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxDevices",
                table: "AspNetUsers",
                newName: "MaxPeers");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeactivated",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    ElevatedSeverity = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_SecurityEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AddRouterPeerEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeerId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAssigned = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddRouterPeerEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_AddRouterPeerEvents_DevicePeers_PeerId",
                        column: x => x.PeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddRouterPeerEvents_SecurityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "SecurityEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreateUserEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreateUserEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_CreateUserEvents_SecurityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "SecurityEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvalidLoginAttemptEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<long>(type: "bigint", nullable: false),
                    TargetUsername = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvalidLoginAttemptEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_InvalidLoginAttemptEvents_SecurityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "SecurityEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvalidTokenEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokenId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    PeerId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Outcome = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvalidTokenEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_InvalidTokenEvents_DevicePeers_PeerId",
                        column: x => x.PeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvalidTokenEvents_DownloadTokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "DownloadTokens",
                        principalColumn: "DownloadTokenId");
                    table.ForeignKey(
                        name: "FK_InvalidTokenEvents_SecurityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "SecurityEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeerAbuseEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeersMade = table.Column<long>(type: "bigint", nullable: false),
                    PeerIds = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerAbuseEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_PeerAbuseEvents_SecurityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "SecurityEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityActions",
                columns: table => new
                {
                    ActionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    TakenByType = table.Column<int>(type: "int", nullable: false),
                    IsReversible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityActions", x => x.ActionId);
                    table.ForeignKey(
                        name: "FK_SecurityActions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SecurityActions_SecurityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "SecurityEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnauthorisedVpnPeerEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VpnServerId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PeerId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PeerCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnauthorisedVpnPeerEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_UnauthorisedVpnPeerEvents_DevicePeers_PeerId",
                        column: x => x.PeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnauthorisedVpnPeerEvents_SecurityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "SecurityEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnauthorisedVpnPeerEvents_VpnServers_VpnServerId",
                        column: x => x.VpnServerId,
                        principalTable: "VpnServers",
                        principalColumn: "VpnServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnauthorisedVpnTokenEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VpnServerId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PeerId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TokenId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    TokenDownloadUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnauthorisedVpnTokenEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_UnauthorisedVpnTokenEvents_DevicePeers_PeerId",
                        column: x => x.PeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnauthorisedVpnTokenEvents_DownloadTokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "DownloadTokens",
                        principalColumn: "DownloadTokenId");
                    table.ForeignKey(
                        name: "FK_UnauthorisedVpnTokenEvents_SecurityEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "SecurityEvents",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnauthorisedVpnTokenEvents_VpnServers_VpnServerId",
                        column: x => x.VpnServerId,
                        principalTable: "VpnServers",
                        principalColumn: "VpnServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddRouterPeerEvents_PeerId",
                table: "AddRouterPeerEvents",
                column: "PeerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvalidTokenEvents_PeerId",
                table: "InvalidTokenEvents",
                column: "PeerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvalidTokenEvents_TokenId",
                table: "InvalidTokenEvents",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityActions_EventId",
                table: "SecurityActions",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityActions_UserId",
                table: "SecurityActions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_UserId",
                table: "SecurityEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UnauthorisedVpnPeerEvents_PeerId",
                table: "UnauthorisedVpnPeerEvents",
                column: "PeerId");

            migrationBuilder.CreateIndex(
                name: "IX_UnauthorisedVpnPeerEvents_VpnServerId",
                table: "UnauthorisedVpnPeerEvents",
                column: "VpnServerId");

            migrationBuilder.CreateIndex(
                name: "IX_UnauthorisedVpnTokenEvents_PeerId",
                table: "UnauthorisedVpnTokenEvents",
                column: "PeerId");

            migrationBuilder.CreateIndex(
                name: "IX_UnauthorisedVpnTokenEvents_TokenId",
                table: "UnauthorisedVpnTokenEvents",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_UnauthorisedVpnTokenEvents_VpnServerId",
                table: "UnauthorisedVpnTokenEvents",
                column: "VpnServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddRouterPeerEvents");

            migrationBuilder.DropTable(
                name: "CreateUserEvents");

            migrationBuilder.DropTable(
                name: "InvalidLoginAttemptEvents");

            migrationBuilder.DropTable(
                name: "InvalidTokenEvents");

            migrationBuilder.DropTable(
                name: "PeerAbuseEvents");

            migrationBuilder.DropTable(
                name: "SecurityActions");

            migrationBuilder.DropTable(
                name: "UnauthorisedVpnPeerEvents");

            migrationBuilder.DropTable(
                name: "UnauthorisedVpnTokenEvents");

            migrationBuilder.DropTable(
                name: "SecurityEvents");

            migrationBuilder.DropColumn(
                name: "IsDeactivated",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "MaxPeers",
                table: "AspNetUsers",
                newName: "MaxDevices");
        }
    }
}
