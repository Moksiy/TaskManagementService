using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaskIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Id",
                table: "Tasks",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_Id",
                table: "Tasks");
        }
    }
}
