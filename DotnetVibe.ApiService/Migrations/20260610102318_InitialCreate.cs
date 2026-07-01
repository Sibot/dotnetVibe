using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DotnetVibe.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _20260610102318_InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherForecasts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TemperatureC = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherForecasts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "WeatherForecasts",
                columns: new[] { "Id", "Date", "Summary", "TemperatureC" },
                values: new object[,]
                {
                    { 1, new DateOnly(2026, 6, 10), "Mild", 22 },
                    { 2, new DateOnly(2026, 6, 11), "Cool", 18 },
                    { 3, new DateOnly(2026, 6, 12), "Warm", 25 },
                    { 4, new DateOnly(2026, 6, 13), "Chilly", 12 },
                    { 5, new DateOnly(2026, 6, 14), "Hot", 30 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherForecasts");
        }
    }
}
