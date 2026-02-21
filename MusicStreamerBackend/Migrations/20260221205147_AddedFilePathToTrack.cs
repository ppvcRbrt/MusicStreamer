using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamerBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedFilePathToTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Tracks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Tracks");
        }
    }
}
