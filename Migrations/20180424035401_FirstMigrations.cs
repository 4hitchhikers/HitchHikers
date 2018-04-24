using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hitchhikers.Migrations
{
    public partial class FirstMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userid = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    first_name = table.Column<string>(nullable: true),
                    last_name = table.Column<string>(nullable: true),
                    nickname = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: true),
                    password = table.Column<string>(nullable: true),
                    profile_pict = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userid);
                });

            migrationBuilder.CreateTable(
                name: "Pictures",
                columns: table => new
                {
                    pictureid = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    pict_name = table.Column<string>(nullable: true),
                    date_visited = table.Column<DateTime>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                    userid = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pictures", x => x.pictureid);
                    table.ForeignKey(
                        name: "FK_Pictures_Users_userid",
                        column: x => x.userid,
                        principalTable: "Users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    commentid = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    comment = table.Column<string>(nullable: true),
                    senderid = table.Column<int>(nullable: false),
                    pictureid = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.commentid);
                    table.ForeignKey(
                        name: "FK_Comments_Pictures_pictureid",
                        column: x => x.pictureid,
                        principalTable: "Pictures",
                        principalColumn: "pictureid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_senderid",
                        column: x => x.senderid,
                        principalTable: "Users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    placeid = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    state = table.Column<string>(nullable: true),
                    city = table.Column<string>(nullable: true),
                    address = table.Column<string>(nullable: true),
                    visitorid = table.Column<int>(nullable: false),
                    place_pictid = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.placeid);
                    table.ForeignKey(
                        name: "FK_Places_Pictures_place_pictid",
                        column: x => x.place_pictid,
                        principalTable: "Pictures",
                        principalColumn: "pictureid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Places_Users_visitorid",
                        column: x => x.visitorid,
                        principalTable: "Users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_pictureid",
                table: "Comments",
                column: "pictureid");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_senderid",
                table: "Comments",
                column: "senderid");

            migrationBuilder.CreateIndex(
                name: "IX_Pictures_userid",
                table: "Pictures",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_Places_place_pictid",
                table: "Places",
                column: "place_pictid");

            migrationBuilder.CreateIndex(
                name: "IX_Places_visitorid",
                table: "Places",
                column: "visitorid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Places");

            migrationBuilder.DropTable(
                name: "Pictures");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
