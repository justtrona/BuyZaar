using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyZaar.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSellerDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileData",
                table: "SellerApplicationDocument");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "SellerApplicationDocument",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "SellerApplicationDocument");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "SellerApplicationDocument",
                type: "longblob",
                nullable: false);
        }
    }
}
