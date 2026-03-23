using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Film> Films => Set<Film>();
    public DbSet<FilmSource> FilmSources => Set<FilmSource>();

}