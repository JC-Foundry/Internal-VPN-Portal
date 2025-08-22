using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteAndVpnUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCleared",
                table: "DownloadTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DevicePeers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VpnServerUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VpnServerId = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpnServerUsers", x => new { x.UserId, x.VpnServerId });
                    table.ForeignKey(
                        name: "FK_VpnServerUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VpnServerUsers_VpnServers_VpnServerId",
                        column: x => x.VpnServerId,
                        principalTable: "VpnServers",
                        principalColumn: "VpnServerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VpnServerUsers_VpnServerId",
                table: "VpnServerUsers",
                column: "VpnServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VpnServerUsers");

            migrationBuilder.DropColumn(
                name: "IsCleared",
                table: "DownloadTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DevicePeers");
        }
    }
}
