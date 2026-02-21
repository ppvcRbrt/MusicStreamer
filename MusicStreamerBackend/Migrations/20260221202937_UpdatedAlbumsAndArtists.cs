using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamerBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAlbumsAndArtists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Albums");

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

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Albums",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Albums",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscogsDbId",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "DiscogsDbId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Albums");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "Albums",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
