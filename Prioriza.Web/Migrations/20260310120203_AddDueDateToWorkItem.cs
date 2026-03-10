using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prioriza.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDueDateToWorkItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                table: "WorkItems",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "WorkItems");
        }
    }
}
