using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedModelMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeerToReservations_DevicePeers_DevicePeerPeerId",
                table: "PeerToReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_PeerToReservations_DnsReservations_DnsReservationReservationId",
                table: "PeerToReservations");

            migrationBuilder.DropIndex(
                name: "IX_PeerToReservations_DevicePeerPeerId",
                table: "PeerToReservations");

            migrationBuilder.DropIndex(
                name: "IX_PeerToReservations_DnsReservationReservationId",
                table: "PeerToReservations");

            migrationBuilder.DropColumn(
                name: "DevicePeerPeerId",
                table: "PeerToReservations");

            migrationBuilder.DropColumn(
                name: "DnsReservationReservationId",
                table: "PeerToReservations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DevicePeerPeerId",
                table: "PeerToReservations",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DnsReservationReservationId",
                table: "PeerToReservations",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeerToReservations_DevicePeerPeerId",
                table: "PeerToReservations",
                column: "DevicePeerPeerId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerToReservations_DnsReservationReservationId",
                table: "PeerToReservations",
                column: "DnsReservationReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PeerToReservations_DevicePeers_DevicePeerPeerId",
                table: "PeerToReservations",
                column: "DevicePeerPeerId",
                principalTable: "DevicePeers",
                principalColumn: "PeerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PeerToReservations_DnsReservations_DnsReservationReservationId",
                table: "PeerToReservations",
                column: "DnsReservationReservationId",
                principalTable: "DnsReservations",
                principalColumn: "ReservationId");
        }
    }
}
