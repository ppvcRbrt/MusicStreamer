using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamerBackend.Migrations
{
    /// <inheritdoc />
    public partial class CreatedExtArtistsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscogsDbId",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "DiscogsDbId",
                table: "Albums");

            migrationBuilder.CreateTable(
                name: "ExtArtists",
                columns: table => new
                {
                    ExtId = table.Column<string>(type: "text", nullable: false),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtArtists", x => x.ExtId);
                    table.ForeignKey(
                        name: "FK_ExtArtists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtArtists_ArtistId",
                table: "ExtArtists",
                column: "ArtistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtArtists");

            migrationBuilder.AddColumn<int>(
                name: "DiscogsDbId",
                table: "Artists",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscogsDbId",
                table: "Albums",
                type: "integer",
                nullable: true);
        }
    }
}
