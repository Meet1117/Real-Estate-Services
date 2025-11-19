using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Real_Estate_Services.Migrations
{
    /// <inheritdoc />
    public partial class addcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image5",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Image6",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Image7",
                table: "Sites");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Sites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Sites");

            migrationBuilder.AddColumn<string>(
                name: "Image5",
                table: "Sites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image6",
                table: "Sites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image7",
                table: "Sites",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
