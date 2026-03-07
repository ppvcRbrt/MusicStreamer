using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamerBackend.Migrations
{
    /// <inheritdoc />
    public partial class CreatedExtAlbumsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Artists");

            migrationBuilder.CreateTable(
                name: "ExtAlbums",
                columns: table => new
                {
                    ExtId = table.Column<string>(type: "text", nullable: false),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtAlbums", x => x.ExtId);
                    table.ForeignKey(
                        name: "FK_ExtAlbums_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtAlbums_AlbumId",
                table: "ExtAlbums",
                column: "AlbumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtAlbums");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Artists",
                type: "text",
                nullable: true);
        }
    }
}
