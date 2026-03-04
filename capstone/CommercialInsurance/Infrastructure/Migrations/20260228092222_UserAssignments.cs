using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedAgentId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedClaimsOfficerId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AssignedAgentId",
                table: "Users",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AssignedClaimsOfficerId",
                table: "Users",
                column: "AssignedClaimsOfficerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_AssignedAgentId",
                table: "Users",
                column: "AssignedAgentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_AssignedClaimsOfficerId",
                table: "Users",
                column: "AssignedClaimsOfficerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_AssignedAgentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_AssignedClaimsOfficerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AssignedAgentId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AssignedClaimsOfficerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AssignedAgentId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AssignedClaimsOfficerId",
                table: "Users");
        }
    }
}
