using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace RedditImageBot.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblProcessedPosts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fullname = table.Column<string>(type: "varchar(16)", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(1024)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblProcessedPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fullname = table.Column<string>(type: "varchar(16)", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PostId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblMessages_tblProcessedPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "tblProcessedPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblMessages_PostId",
                table: "tblMessages",
                column: "PostId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblMessages");

            migrationBuilder.DropTable(
                name: "tblProcessedPosts");
        }
    }
}
