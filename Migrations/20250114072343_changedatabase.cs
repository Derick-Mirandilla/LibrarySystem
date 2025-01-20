using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Migrations
{
    /// <inheritdoc />
    public partial class changedatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookInformation",
                columns: table => new
                {
                    BookID = table.Column<int>(type: "int", nullable: false),
                    BookTitle = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    BookAuthor = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    BookClassification = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    BookDonor = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    BookCategory = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    BookStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookInformation", x => x.BookID);
                });

            migrationBuilder.CreateTable(
                name: "StudentInformation",
                columns: table => new
                {
                    StudentLRN = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    StudentGradeSection = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    StudentContact = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    StudentEmail = table.Column<string>(type: "nvarchar(150)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentInformation", x => x.StudentLRN);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LRN = table.Column<string>(type: "nvarchar(12)", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradeAndSection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookID = table.Column<int>(type: "int", nullable: false),
                    BookTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_Transactions_BookInformation_BookID",
                        column: x => x.BookID,
                        principalTable: "BookInformation",
                        principalColumn: "BookID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_StudentInformation_LRN",
                        column: x => x.LRN,
                        principalTable: "StudentInformation",
                        principalColumn: "StudentLRN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BookID",
                table: "Transactions",
                column: "BookID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_LRN",
                table: "Transactions",
                column: "LRN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "BookInformation");

            migrationBuilder.DropTable(
                name: "StudentInformation");
        }
    }
}
