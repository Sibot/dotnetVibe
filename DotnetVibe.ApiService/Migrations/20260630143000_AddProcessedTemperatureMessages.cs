using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetVibe.ApiService.Migrations;

/// <inheritdoc />
public partial class AddProcessedTemperatureMessages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProcessedTemperatureMessages",
            columns: table => new
            {
                MessageId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProcessedTemperatureMessages", x => x.MessageId);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProcessedTemperatureMessages");
    }
}
