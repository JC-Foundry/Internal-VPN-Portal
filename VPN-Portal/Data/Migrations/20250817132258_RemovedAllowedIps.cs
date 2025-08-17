using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAllowedIps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedIps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedIps",
                columns: table => new
                {
                    AllowedIpId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedIps", x => x.AllowedIpId);
                    table.ForeignKey(
                        name: "FK_AllowedIps_DevicePeers_PeerId",
                        column: x => x.PeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIps_IpAddress",
                table: "AllowedIps",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIps_PeerId",
                table: "AllowedIps",
                column: "PeerId");
        }
    }
}
