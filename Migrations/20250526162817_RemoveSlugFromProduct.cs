using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Website_BanMayTinh.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSlugFromProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
