using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShittyOne.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifySurveyQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSurveyQuestion_SurveyQuestion_SurveyQuestionId",
                table: "GroupSurveyQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestion_Files_FileId",
                table: "SurveyQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestion_Surveys_SurveyId",
                table: "SurveyQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestionUser_SurveyQuestion_SurveyQuestionId",
                table: "SurveyQuestionUser");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveysAnswer_SurveyQuestion_SuveyQuestionId",
                table: "SurveysAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_SurveyQuestion_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyQuestion",
                table: "SurveyQuestion");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "SurveyQuestion");

            migrationBuilder.RenameTable(
                name: "SurveyQuestion",
                newName: "SurveyQuestions");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyQuestion_SurveyId",
                table: "SurveyQuestions",
                newName: "IX_SurveyQuestions_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyQuestion_FileId",
                table: "SurveyQuestions",
                newName: "IX_SurveyQuestions_FileId");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "SurveyQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyQuestions",
                table: "SurveyQuestions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSurveyQuestion_SurveyQuestions_SurveyQuestionId",
                table: "GroupSurveyQuestion",
                column: "SurveyQuestionId",
                principalTable: "SurveyQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestions_Files_FileId",
                table: "SurveyQuestions",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestions_Surveys_SurveyId",
                table: "SurveyQuestions",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestionUser_SurveyQuestions_SurveyQuestionId",
                table: "SurveyQuestionUser",
                column: "SurveyQuestionId",
                principalTable: "SurveyQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveysAnswer_SurveyQuestions_SuveyQuestionId",
                table: "SurveysAnswer",
                column: "SuveyQuestionId",
                principalTable: "SurveyQuestions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_SurveyQuestions_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "SurveyQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupSurveyQuestion_SurveyQuestions_SurveyQuestionId",
                table: "GroupSurveyQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestions_Files_FileId",
                table: "SurveyQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestions_Surveys_SurveyId",
                table: "SurveyQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestionUser_SurveyQuestions_SurveyQuestionId",
                table: "SurveyQuestionUser");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveysAnswer_SurveyQuestions_SuveyQuestionId",
                table: "SurveysAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_SurveyQuestions_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyQuestions",
                table: "SurveyQuestions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "SurveyQuestions");

            migrationBuilder.RenameTable(
                name: "SurveyQuestions",
                newName: "SurveyQuestion");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyQuestions_SurveyId",
                table: "SurveyQuestion",
                newName: "IX_SurveyQuestion_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyQuestions_FileId",
                table: "SurveyQuestion",
                newName: "IX_SurveyQuestion_FileId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "SurveyQuestion",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyQuestion",
                table: "SurveyQuestion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupSurveyQuestion_SurveyQuestion_SurveyQuestionId",
                table: "GroupSurveyQuestion",
                column: "SurveyQuestionId",
                principalTable: "SurveyQuestion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestion_Files_FileId",
                table: "SurveyQuestion",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestion_Surveys_SurveyId",
                table: "SurveyQuestion",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestionUser_SurveyQuestion_SurveyQuestionId",
                table: "SurveyQuestionUser",
                column: "SurveyQuestionId",
                principalTable: "SurveyQuestion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveysAnswer_SurveyQuestion_SuveyQuestionId",
                table: "SurveysAnswer",
                column: "SuveyQuestionId",
                principalTable: "SurveyQuestion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_SurveyQuestion_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "SurveyQuestion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
