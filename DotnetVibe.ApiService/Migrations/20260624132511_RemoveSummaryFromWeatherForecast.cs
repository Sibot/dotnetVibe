using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetVibe.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class _20260624132511_RemoveSummaryFromWeatherForecast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "WeatherForecasts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "WeatherForecasts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "WeatherForecasts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Summary",
                value: "Mild");

            migrationBuilder.UpdateData(
                table: "WeatherForecasts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Summary",
                value: "Cool");

            migrationBuilder.UpdateData(
                table: "WeatherForecasts",
                keyColumn: "Id",
                keyValue: 3,
                column: "Summary",
                value: "Warm");

            migrationBuilder.UpdateData(
                table: "WeatherForecasts",
                keyColumn: "Id",
                keyValue: 4,
                column: "Summary",
                value: "Chilly");

            migrationBuilder.UpdateData(
                table: "WeatherForecasts",
                keyColumn: "Id",
                keyValue: 5,
                column: "Summary",
                value: "Hot");
        }
    }
}
