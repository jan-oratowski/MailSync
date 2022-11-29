using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailSync.Core.Migrations
{
    /// <inheritdoc />
    public partial class SslAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseSsl",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseSsl",
                table: "Accounts");
        }
    }
}
