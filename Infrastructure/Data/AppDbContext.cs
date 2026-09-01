using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Film> Films => Set<Film>();
    public DbSet<FilmSource> FilmSources => Set<FilmSource>();
    public DbSet<AccountRequest> AccountRequests => Set<AccountRequest>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Keyword> Keywords => Set<Keyword>();
    public DbSet<ProductionCompany> ProductionCompanies => Set<ProductionCompany>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<FilmCredit> FilmCredits => Set<FilmCredit>();
    public DbSet<FilmGenre> FilmGenres => Set<FilmGenre>();
    public DbSet<FilmKeyword> FilmKeywords => Set<FilmKeyword>();
    public DbSet<FilmProductionCompany> FilmProductionCompanies => Set<FilmProductionCompany>();
    public DbSet<FilmAlternativeTitle> FilmAlternativeTitles => Set<FilmAlternativeTitle>();
    public DbSet<FilmVideo> FilmVideos => Set<FilmVideo>();
    public DbSet<FilmReleaseDate> FilmReleaseDates => Set<FilmReleaseDate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AccountRequest>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasIndex(x => x.Token)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        builder.Entity<Film>(entity =>
        {
            entity.HasOne(f => f.Collection)
                .WithMany(c => c.Films)
                .HasForeignKey(f => f.CollectionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Person>(entity =>
        {
            entity.HasIndex(p => p.TmdbId).IsUnique();
        });

        builder.Entity<Genre>(entity =>
        {
            entity.HasIndex(g => g.TmdbId).IsUnique();
        });

        builder.Entity<Keyword>(entity =>
        {
            entity.HasIndex(k => k.TmdbId).IsUnique();
        });

        builder.Entity<ProductionCompany>(entity =>
        {
            entity.HasIndex(p => p.TmdbId).IsUnique();
        });

        builder.Entity<Collection>(entity =>
        {
            entity.HasIndex(c => c.TmdbId).IsUnique();
        });

        builder.Entity<FilmCredit>(entity =>
        {
            entity.HasIndex(fc => fc.TmdbCreditId).IsUnique();

            entity.HasIndex(fc => new { fc.FilmId, fc.CreditType, fc.CreditOrder });

            entity.HasOne(fc => fc.Film)
                .WithMany(f => f.Credits)
                .HasForeignKey(fc => fc.FilmId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(fc => fc.Person)
                .WithMany(p => p.Credits)
                .HasForeignKey(fc => fc.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FilmGenre>(entity =>
        {
            entity.HasKey(fg => new { fg.FilmId, fg.GenreId });

            entity.HasOne(fg => fg.Film)
                .WithMany(f => f.Genres)
                .HasForeignKey(fg => fg.FilmId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(fg => fg.Genre)
                .WithMany(g => g.Films)
                .HasForeignKey(fg => fg.GenreId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FilmKeyword>(entity =>
        {
            entity.HasKey(fk => new { fk.FilmId, fk.KeywordId });

            entity.HasOne(fk => fk.Film)
                .WithMany(f => f.Keywords)
                .HasForeignKey(fk => fk.FilmId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(fk => fk.Keyword)
                .WithMany(k => k.Films)
                .HasForeignKey(fk => fk.KeywordId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FilmProductionCompany>(entity =>
        {
            entity.HasKey(fp => new { fp.FilmId, fp.ProductionCompanyId });

            entity.HasOne(fp => fp.Film)
                .WithMany(f => f.ProductionCompanies)
                .HasForeignKey(fp => fp.FilmId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(fp => fp.ProductionCompany)
                .WithMany(p => p.Films)
                .HasForeignKey(fp => fp.ProductionCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FilmAlternativeTitle>(entity =>
        {
            entity.HasOne(fa => fa.Film)
                .WithMany(f => f.AlternativeTitles)
                .HasForeignKey(fa => fa.FilmId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FilmVideo>(entity =>
        {
            entity.HasOne(fv => fv.Film)
                .WithMany(f => f.Videos)
                .HasForeignKey(fv => fv.FilmId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FilmReleaseDate>(entity =>
        {
            entity.HasOne(fr => fr.Film)
                .WithMany(f => f.ReleaseDates)
                .HasForeignKey(fr => fr.FilmId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

}