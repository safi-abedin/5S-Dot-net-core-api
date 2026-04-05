using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldinChecklistitemModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuestionText",
                table: "ChecklistItems",
                newName: "EvaluationCriteria");

            migrationBuilder.AddColumn<string>(
                name: "CheckingItemName",
                table: "ChecklistItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckingItemName",
                table: "ChecklistItems");

            migrationBuilder.RenameColumn(
                name: "EvaluationCriteria",
                table: "ChecklistItems",
                newName: "QuestionText");
        }
    }
}
