using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPN_Portal.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxDevices = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DnsPools",
                columns: table => new
                {
                    DnsPoolId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Family = table.Column<int>(type: "int", nullable: false),
                    SecondOctet = table.Column<int>(type: "int", nullable: true),
                    Subnet = table.Column<int>(type: "int", nullable: false),
                    MinHost = table.Column<int>(type: "int", nullable: false),
                    MaxHost = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnsPools", x => x.DnsPoolId);
                });

            migrationBuilder.CreateTable(
                name: "VpnServers",
                columns: table => new
                {
                    VpnServerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EndpointPort = table.Column<long>(type: "bigint", nullable: false),
                    EndpointHost = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AddressCidr = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NetworkAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VpnServers", x => x.VpnServerId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_Devices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevicePeers",
                columns: table => new
                {
                    PeerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VpnServerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePeers", x => x.PeerId);
                    table.ForeignKey(
                        name: "FK_DevicePeers_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevicePeers_VpnServers_VpnServerId",
                        column: x => x.VpnServerId,
                        principalTable: "VpnServers",
                        principalColumn: "VpnServerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DnsReservations",
                columns: table => new
                {
                    ReservationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DnsPoolId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HostOctet = table.Column<int>(type: "int", nullable: false),
                    ReservedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnsReservations", x => x.ReservationId);
                    table.ForeignKey(
                        name: "FK_DnsReservations_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DnsReservations_DnsPools_DnsPoolId",
                        column: x => x.DnsPoolId,
                        principalTable: "DnsPools",
                        principalColumn: "DnsPoolId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "PeerToReservations",
                columns: table => new
                {
                    PeerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReservationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DevicePeerPeerId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DnsReservationReservationId = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerToReservations", x => new { x.PeerId, x.ReservationId });
                    table.ForeignKey(
                        name: "FK_PeerToReservations_DevicePeers_DevicePeerPeerId",
                        column: x => x.DevicePeerPeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId");
                    table.ForeignKey(
                        name: "FK_PeerToReservations_DevicePeers_PeerId",
                        column: x => x.PeerId,
                        principalTable: "DevicePeers",
                        principalColumn: "PeerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerToReservations_DnsReservations_DnsReservationReservationId",
                        column: x => x.DnsReservationReservationId,
                        principalTable: "DnsReservations",
                        principalColumn: "ReservationId");
                    table.ForeignKey(
                        name: "FK_PeerToReservations_DnsReservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "DnsReservations",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIps_IpAddress",
                table: "AllowedIps",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIps_PeerId",
                table: "AllowedIps",
                column: "PeerId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastLogin",
                table: "AspNetUsers",
                column: "LastLogin");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePeers_DeviceId",
                table: "DevicePeers",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePeers_VpnServerId",
                table: "DevicePeers",
                column: "VpnServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_CreatedUtc",
                table: "Devices",
                column: "CreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_IsRevoked",
                table: "Devices",
                column: "IsRevoked");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DnsPools_Enabled",
                table: "DnsPools",
                column: "Enabled");

            migrationBuilder.CreateIndex(
                name: "IX_DnsPools_Name",
                table: "DnsPools",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DnsReservations_DeviceId",
                table: "DnsReservations",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DnsReservations_DnsPoolId_HostOctet",
                table: "DnsReservations",
                columns: new[] { "DnsPoolId", "HostOctet" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DnsReservations_ReleasedUtc",
                table: "DnsReservations",
                column: "ReleasedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PeerToReservations_DevicePeerPeerId",
                table: "PeerToReservations",
                column: "DevicePeerPeerId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerToReservations_DnsReservationReservationId",
                table: "PeerToReservations",
                column: "DnsReservationReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerToReservations_PeerId",
                table: "PeerToReservations",
                column: "PeerId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerToReservations_ReservationId",
                table: "PeerToReservations",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_VpnServers_Name",
                table: "VpnServers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedIps");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "PeerToReservations");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "DevicePeers");

            migrationBuilder.DropTable(
                name: "DnsReservations");

            migrationBuilder.DropTable(
                name: "VpnServers");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "DnsPools");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
