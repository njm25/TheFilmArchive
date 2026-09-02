using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addDeepMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TmdbId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Overview = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PosterPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BackdropPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TmdbId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    KnownForDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProfilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Biography = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deathday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImdbId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Popularity = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TmdbId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Keywords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TmdbId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keywords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TmdbId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginCountry = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionCompanies", x => x.Id);
                });

            migrationBuilder.AddColumn<string>(
                name: "ImdbId",
                table: "Films",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalTitle",
                table: "Films",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalLanguage",
                table: "Films",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Homepage",
                table: "Films",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Films",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Adult",
                table: "Films",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Budget",
                table: "Films",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Revenue",
                table: "Films",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Popularity",
                table: "Films",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VoteAverage",
                table: "Films",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VoteCount",
                table: "Films",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "Films",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FilmCredits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    CreditType = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Job = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Character = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreditOrder = table.Column<int>(type: "int", nullable: true),
                    TmdbCreditId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilmCredits_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilmCredits_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilmGenres",
                columns: table => new
                {
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmGenres", x => new { x.FilmId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_FilmGenres_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilmGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilmKeywords",
                columns: table => new
                {
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    KeywordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmKeywords", x => new { x.FilmId, x.KeywordId });
                    table.ForeignKey(
                        name: "FK_FilmKeywords_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilmKeywords_Keywords_KeywordId",
                        column: x => x.KeywordId,
                        principalTable: "Keywords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilmProductionCompanies",
                columns: table => new
                {
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    ProductionCompanyId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmProductionCompanies", x => new { x.FilmId, x.ProductionCompanyId });
                    table.ForeignKey(
                        name: "FK_FilmProductionCompanies_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilmProductionCompanies_ProductionCompanies_ProductionCompanyId",
                        column: x => x.ProductionCompanyId,
                        principalTable: "ProductionCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilmAlternativeTitles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmAlternativeTitles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilmAlternativeTitles_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilmVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Site = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VideoType = table.Column<int>(type: "int", nullable: false),
                    Official = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilmVideos_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilmReleaseDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleaseType = table.Column<int>(type: "int", nullable: false),
                    Certification = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmReleaseDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilmReleaseDates_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collections_TmdbId",
                table: "Collections",
                column: "TmdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_TmdbId",
                table: "People",
                column: "TmdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genres_TmdbId",
                table: "Genres",
                column: "TmdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_TmdbId",
                table: "Keywords",
                column: "TmdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionCompanies_TmdbId",
                table: "ProductionCompanies",
                column: "TmdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Films_CollectionId",
                table: "Films",
                column: "CollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Films_Collections_CollectionId",
                table: "Films",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_FilmCredits_TmdbCreditId",
                table: "FilmCredits",
                column: "TmdbCreditId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilmCredits_FilmId_CreditType_CreditOrder",
                table: "FilmCredits",
                columns: new[] { "FilmId", "CreditType", "CreditOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FilmCredits_PersonId",
                table: "FilmCredits",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmGenres_GenreId",
                table: "FilmGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmKeywords_KeywordId",
                table: "FilmKeywords",
                column: "KeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmProductionCompanies_ProductionCompanyId",
                table: "FilmProductionCompanies",
                column: "ProductionCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmAlternativeTitles_FilmId",
                table: "FilmAlternativeTitles",
                column: "FilmId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmVideos_FilmId",
                table: "FilmVideos",
                column: "FilmId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmReleaseDates_FilmId",
                table: "FilmReleaseDates",
                column: "FilmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilmReleaseDates");

            migrationBuilder.DropTable(
                name: "FilmVideos");

            migrationBuilder.DropTable(
                name: "FilmAlternativeTitles");

            migrationBuilder.DropTable(
                name: "FilmProductionCompanies");

            migrationBuilder.DropTable(
                name: "FilmKeywords");

            migrationBuilder.DropTable(
                name: "FilmGenres");

            migrationBuilder.DropTable(
                name: "FilmCredits");

            migrationBuilder.DropForeignKey(
                name: "FK_Films_Collections_CollectionId",
                table: "Films");

            migrationBuilder.DropIndex(
                name: "IX_Films_CollectionId",
                table: "Films");

            migrationBuilder.DropTable(
                name: "ProductionCompanies");

            migrationBuilder.DropTable(
                name: "Keywords");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "VoteCount",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "VoteAverage",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "Popularity",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "Revenue",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "Adult",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "Homepage",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "OriginalLanguage",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "OriginalTitle",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "ImdbId",
                table: "Films");
        }
    }
}
