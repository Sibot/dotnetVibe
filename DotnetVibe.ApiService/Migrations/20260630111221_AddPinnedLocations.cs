using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetVibe.ApiService.Migrations;

/// <inheritdoc />
public partial class _20260630111221_AddPinnedLocations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PinnedLocations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Latitude = table.Column<double>(type: "float", nullable: false),
                Longitude = table.Column<double>(type: "float", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PinnedLocations", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PinnedLocations_UserId_SortOrder",
            table: "PinnedLocations",
            columns: new[] { "UserId", "SortOrder" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PinnedLocations");
    }
}
