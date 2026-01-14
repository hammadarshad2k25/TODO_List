using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TODO_List.Migrations
{
    /// <inheritdoc />
    public partial class addsubtodotasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubTaskDbModel",
                columns: table => new
                {
                    SubTaskId = table.Column<int>(type: "int", nullable: false),
                    SubTaskName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubTaskDbModel", x => x.SubTaskId);
                    table.ForeignKey(
                        name: "FK_SubTaskDbModel_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "TaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubTaskDbModel_TaskId",
                table: "SubTaskDbModel",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubTaskDbModel");
        }
    }
}
