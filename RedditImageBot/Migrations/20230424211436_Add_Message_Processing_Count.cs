using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedditImageBot.Migrations
{
    /// <inheritdoc />
    public partial class Add_Message_Processing_Count : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "ProcessingCount",
                table: "messages",
                type: "smallint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingCount",
                table: "messages");
        }
    }
}
