using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class RelaseAddedToUniqueInReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DnsReservations_DnsPoolId_HostOctet",
                table: "DnsReservations");

            migrationBuilder.CreateIndex(
                name: "IX_DnsReservations_DnsPoolId_HostOctet_ReleasedUtc",
                table: "DnsReservations",
                columns: new[] { "DnsPoolId", "HostOctet", "ReleasedUtc" },
                unique: true,
                filter: "[ReleasedUtc] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DnsReservations_DnsPoolId_HostOctet_ReleasedUtc",
                table: "DnsReservations");

            migrationBuilder.CreateIndex(
                name: "IX_DnsReservations_DnsPoolId_HostOctet",
                table: "DnsReservations",
                columns: new[] { "DnsPoolId", "HostOctet" },
                unique: true);
        }
    }
}
