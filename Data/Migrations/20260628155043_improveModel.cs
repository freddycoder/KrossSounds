using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KrossSounds.Migrations
{
    /// <inheritdoc />
    public partial class improveModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ushort>(
                name: "Number",
                table: "Sounds",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Sounds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(ushort),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
