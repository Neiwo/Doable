using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doable.Migrations
{
    /// <inheritdoc />
    public partial class Adduploadedrole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadedbyRole",
                table: "Docus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedbyRole",
                table: "Docus");
        }
    }
}
