using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigPath",
                table: "DownloadTokens");

            migrationBuilder.DropColumn(
                name: "OneTime",
                table: "DownloadTokens");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "DownloadTokens",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DownloadEvents",
                columns: table => new
                {
                    DownloadEventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokenId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PeerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    AttemptedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Outcome = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadEvents", x => x.DownloadEventId);
                    table.ForeignKey(
                        name: "FK_DownloadEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DownloadEvents_DevicePeers_PeerId",
                        column: x => x.PeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DownloadEvents_DownloadTokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "DownloadTokens",
                        principalColumn: "DownloadTokenId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTokens_UserId",
                table: "DownloadTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadEvents_PeerId",
                table: "DownloadEvents",
                column: "PeerId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadEvents_TokenId",
                table: "DownloadEvents",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadEvents_UserId",
                table: "DownloadEvents",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadTokens_AspNetUsers_UserId",
                table: "DownloadTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadTokens_AspNetUsers_UserId",
                table: "DownloadTokens");

            migrationBuilder.DropTable(
                name: "DownloadEvents");

            migrationBuilder.DropIndex(
                name: "IX_DownloadTokens_UserId",
                table: "DownloadTokens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DownloadTokens");

            migrationBuilder.AddColumn<string>(
                name: "ConfigPath",
                table: "DownloadTokens",
                type: "nvarchar(500)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OneTime",
                table: "DownloadTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
