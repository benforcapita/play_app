using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace play_app_api.Migrations
{
    /// <inheritdoc />
    public partial class AddExtractionJobCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ExtractionJobs_JobToken_OwnerId",
                table: "ExtractionJobs",
                columns: new[] { "JobToken", "OwnerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExtractionJobs_JobToken_OwnerId",
                table: "ExtractionJobs");
        }
    }
}
