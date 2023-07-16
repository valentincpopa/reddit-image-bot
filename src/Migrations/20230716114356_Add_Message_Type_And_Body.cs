using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedditImageBot.Migrations
{
    /// <inheritdoc />
    public partial class Add_Message_Type_And_Body : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExternalParentId",
                table: "messages",
                type: "varchar(16)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(16)");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "messages",
                type: "varchar(2048)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "Type",
                table: "messages",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Body",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "messages");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalParentId",
                table: "messages",
                type: "varchar(16)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldNullable: true);
        }
    }
}
