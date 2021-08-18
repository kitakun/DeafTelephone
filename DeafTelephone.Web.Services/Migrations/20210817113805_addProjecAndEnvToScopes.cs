using Microsoft.EntityFrameworkCore.Migrations;

namespace DeafTelephone.Web.Services.Migrations
{
    public partial class addProjecAndEnvToScopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "LogScopes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Project",
                table: "LogScopes",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Environment",
                table: "LogScopes");

            migrationBuilder.DropColumn(
                name: "Project",
                table: "LogScopes");
        }
    }
}
