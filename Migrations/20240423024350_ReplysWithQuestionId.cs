using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PressTheButton.Migrations
{
    /// <inheritdoc />
    public partial class ReplysWithQuestionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionId",
                table: "Replys",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Replys_QuestionId",
                table: "Replys",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Replys_Questions_QuestionId",
                table: "Replys",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.NoAction, // Definindo ação NO ACTION para ON DELETE
                onUpdate: ReferentialAction.NoAction); // Definindo ação NO ACTION para ON UPDATE
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Replys_Questions_QuestionId",
                table: "Replys");

            migrationBuilder.DropIndex(
                name: "IX_Replys_QuestionId",
                table: "Replys");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "Replys");
        }
    }
}
