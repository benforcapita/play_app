using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace play_app_api.Migrations
{
    /// <inheritdoc />
    public partial class AddQueuePositionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ExtractionJobs_OwnerId_Status_CreatedAt",
                table: "ExtractionJobs",
                columns: new[] { "OwnerId", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExtractionJobs_OwnerId_Status_CreatedAt",
                table: "ExtractionJobs");
        }
    }
}
