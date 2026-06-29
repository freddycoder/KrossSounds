using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KrossSounds.Migrations
{
    /// <inheritdoc />
    public partial class addFieldToSound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Sounds",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Sounds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "YoutubeUrl",
                table: "Sounds",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Sounds");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Sounds");

            migrationBuilder.DropColumn(
                name: "YoutubeUrl",
                table: "Sounds");
        }
    }
}
