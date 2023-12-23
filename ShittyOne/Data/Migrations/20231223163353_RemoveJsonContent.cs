using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShittyOne.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJsonContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JsonContent",
                table: "SurveyQuestions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JsonContent",
                table: "SurveyQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
