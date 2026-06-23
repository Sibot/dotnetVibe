using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetVibe.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _20260623081217_AddUniqueDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WeatherForecasts_Date",
                table: "WeatherForecasts",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WeatherForecasts_Date",
                table: "WeatherForecasts");
        }
    }
}
