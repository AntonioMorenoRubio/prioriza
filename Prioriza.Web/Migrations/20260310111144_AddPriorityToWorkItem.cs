using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prioriza.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityToWorkItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "WorkItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "WorkItems");
        }
    }
}
