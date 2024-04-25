using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PressTheButton.Migrations
{
    /// <inheritdoc />
    public partial class CommentsAndReplysWithPictures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfilePictureId",
                table: "Replys",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "Replys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfilePictureId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Replys_ProfilePictureId",
                table: "Replys",
                column: "ProfilePictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ProfilePictureId",
                table: "Comments",
                column: "ProfilePictureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_ProfilePictures_ProfilePictureId",
                table: "Comments",
                column: "ProfilePictureId",
                principalTable: "ProfilePictures",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Replys_ProfilePictures_ProfilePictureId",
                table: "Replys",
                column: "ProfilePictureId",
                principalTable: "ProfilePictures",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_ProfilePictures_ProfilePictureId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Replys_ProfilePictures_ProfilePictureId",
                table: "Replys");

            migrationBuilder.DropIndex(
                name: "IX_Replys_ProfilePictureId",
                table: "Replys");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ProfilePictureId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ProfilePictureId",
                table: "Replys");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "Replys");

            migrationBuilder.DropColumn(
                name: "ProfilePictureId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "Comments");
        }
    }
}
