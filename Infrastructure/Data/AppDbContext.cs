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
    }

}