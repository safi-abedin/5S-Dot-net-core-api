using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "AuditItems");

            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "AuditItems");

            migrationBuilder.CreateTable(
                name: "FeedBackItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Good = table.Column<bool>(type: "bit", nullable: true),
                    Bad = table.Column<bool>(type: "bit", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedBackItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedBackItem_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedBackItem_AuditId",
                table: "FeedBackItem",
                column: "AuditId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedBackItem");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "AuditItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "AuditItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
