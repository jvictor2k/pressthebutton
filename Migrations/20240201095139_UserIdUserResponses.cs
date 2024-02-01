using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PressTheButton.Migrations
{
    /// <inheritdoc />
    public partial class UserIdUserResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserResponses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserResponses");
        }
    }
}
