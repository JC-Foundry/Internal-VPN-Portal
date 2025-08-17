using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnabledStateForVpnServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "VpnServers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "VpnServers");
        }
    }
}
