using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailSync.Core.Migrations
{
    /// <inheritdoc />
    public partial class DestinationRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MapTo",
                table: "Folders");

            migrationBuilder.AddColumn<int>(
                name: "MapToId",
                table: "Folders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_MapToId",
                table: "Folders",
                column: "MapToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Folders_MapToId",
                table: "Folders",
                column: "MapToId",
                principalTable: "Folders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Folders_MapToId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_MapToId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "MapToId",
                table: "Folders");

            migrationBuilder.AddColumn<string>(
                name: "MapTo",
                table: "Folders",
                type: "TEXT",
                nullable: true);
        }
    }
}
