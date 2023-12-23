using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShittyOne.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserSessionsToSurveySessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_UserSessions_SessionId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_AspNetUsers_UserId",
                table: "UserSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Surveys_SurveyId",
                table: "UserSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSessions",
                table: "UserSessions");

            migrationBuilder.RenameTable(
                name: "UserSessions",
                newName: "SurveySessions");

            migrationBuilder.RenameIndex(
                name: "IX_UserSessions_UserId",
                table: "SurveySessions",
                newName: "IX_SurveySessions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSessions_SurveyId",
                table: "SurveySessions",
                newName: "IX_SurveySessions_SurveyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveySessions",
                table: "SurveySessions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySessions_AspNetUsers_UserId",
                table: "SurveySessions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveySessions_Surveys_SurveyId",
                table: "SurveySessions",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_SurveySessions_SessionId",
                table: "UserAnswers",
                column: "SessionId",
                principalTable: "SurveySessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveySessions_AspNetUsers_UserId",
                table: "SurveySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveySessions_Surveys_SurveyId",
                table: "SurveySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_SurveySessions_SessionId",
                table: "UserAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveySessions",
                table: "SurveySessions");

            migrationBuilder.RenameTable(
                name: "SurveySessions",
                newName: "UserSessions");

            migrationBuilder.RenameIndex(
                name: "IX_SurveySessions_UserId",
                table: "UserSessions",
                newName: "IX_UserSessions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveySessions_SurveyId",
                table: "UserSessions",
                newName: "IX_UserSessions_SurveyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSessions",
                table: "UserSessions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_UserSessions_SessionId",
                table: "UserAnswers",
                column: "SessionId",
                principalTable: "UserSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_AspNetUsers_UserId",
                table: "UserSessions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Surveys_SurveyId",
                table: "UserSessions",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id");
        }
    }
}
