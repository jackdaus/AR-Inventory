using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AR_Inventory.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpatialAnchorUuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    LocationX = table.Column<float>(type: "REAL", nullable: false),
                    LocationY = table.Column<float>(type: "REAL", nullable: false),
                    LocationZ = table.Column<float>(type: "REAL", nullable: false),
                    OrientationX = table.Column<float>(type: "REAL", nullable: false),
                    OrientationY = table.Column<float>(type: "REAL", nullable: false),
                    OrientationZ = table.Column<float>(type: "REAL", nullable: false),
                    OrientationW = table.Column<float>(type: "REAL", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
