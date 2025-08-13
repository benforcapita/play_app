using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace play_app_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add index for status-only queries (used by background service)
            migrationBuilder.CreateIndex(
                name: "IX_ExtractionJobs_Status_CreatedAt",
                table: "ExtractionJobs",
                columns: new[] { "Status", "CreatedAt" });

            // Add index for job token lookups
            migrationBuilder.CreateIndex(
                name: "IX_ExtractionJobs_JobToken",
                table: "ExtractionJobs",
                column: "JobToken",
                unique: true);

            // Add index for character lookups
            migrationBuilder.CreateIndex(
                name: "IX_Characters_OwnerId",
                table: "Characters",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExtractionJobs_Status_CreatedAt",
                table: "ExtractionJobs");

            migrationBuilder.DropIndex(
                name: "IX_ExtractionJobs_JobToken",
                table: "ExtractionJobs");

            migrationBuilder.DropIndex(
                name: "IX_Characters_OwnerId",
                table: "Characters");
        }
    }
}
