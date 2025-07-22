using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocaLens.Migrations.AudioDb
{
    public partial class AudioDbInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AudioRecordings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AudioData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    TranscribedTextEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TranscribedTextArabic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TranscribedTranslated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioRecordings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioRecordings");
        }
    }
}
