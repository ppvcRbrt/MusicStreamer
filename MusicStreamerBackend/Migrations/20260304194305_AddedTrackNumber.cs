using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamerBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedTrackNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrackNumber",
                table: "Tracks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackNumber",
                table: "Tracks");
        }
    }
}
