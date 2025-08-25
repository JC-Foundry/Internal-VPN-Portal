using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class PeerAbuseLastSeen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTimeUtc",
                table: "PeerAbuseEvents");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenUtc",
                table: "PeerAbuseEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedUtc",
                table: "DevicePeers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenUtc",
                table: "PeerAbuseEvents");

            migrationBuilder.DropColumn(
                name: "DeletedUtc",
                table: "DevicePeers");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTimeUtc",
                table: "PeerAbuseEvents",
                type: "datetime2",
                nullable: true);
        }
    }
}
