using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullablePeerInTokenEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvalidTokenEvents_DevicePeers_PeerId",
                table: "InvalidTokenEvents");

            migrationBuilder.AlterColumn<string>(
                name: "PeerId",
                table: "InvalidTokenEvents",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AddForeignKey(
                name: "FK_InvalidTokenEvents_DevicePeers_PeerId",
                table: "InvalidTokenEvents",
                column: "PeerId",
                principalTable: "DevicePeers",
                principalColumn: "PeerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvalidTokenEvents_DevicePeers_PeerId",
                table: "InvalidTokenEvents");

            migrationBuilder.AlterColumn<string>(
                name: "PeerId",
                table: "InvalidTokenEvents",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InvalidTokenEvents_DevicePeers_PeerId",
                table: "InvalidTokenEvents",
                column: "PeerId",
                principalTable: "DevicePeers",
                principalColumn: "PeerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
