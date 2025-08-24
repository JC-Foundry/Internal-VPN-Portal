using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedInvalidLoginEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTimeUtc",
                table: "InvalidLoginAttemptEvents");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenUtc",
                table: "InvalidLoginAttemptEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenUtc",
                table: "InvalidLoginAttemptEvents");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTimeUtc",
                table: "InvalidLoginAttemptEvents",
                type: "datetime2",
                nullable: true);
        }
    }
}
