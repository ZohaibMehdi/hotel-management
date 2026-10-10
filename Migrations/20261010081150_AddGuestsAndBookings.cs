using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestsAndBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Rooms_TenantId_Id",
                table: "Rooms",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FathersName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NicNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.Id);
                    table.UniqueConstraint("AK_Guests_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_Guests_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Guests_TenantId_GuestId",
                        columns: x => new { x.TenantId, x.GuestId },
                        principalTable: "Guests",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Rooms_TenantId_RoomId",
                        columns: x => new { x.TenantId, x.RoomId },
                        principalTable: "Rooms",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TenantId_GuestId",
                table: "Bookings",
                columns: new[] { "TenantId", "GuestId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TenantId_RoomId_CheckInDate",
                table: "Bookings",
                columns: new[] { "TenantId", "RoomId", "CheckInDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Guests_TenantId_NicNumber",
                table: "Guests",
                columns: new[] { "TenantId", "NicNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Guests");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Rooms_TenantId_Id",
                table: "Rooms");
        }
    }
}
