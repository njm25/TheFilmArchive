using Api.Services;
using Infrastructure.Clients;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<TmdbService>(sp =>
    new TmdbService(
        builder.Configuration["Tmdb:ApiKey"]!
    )
);

builder.Services.AddSingleton<DiscordBot>(sp =>
    new DiscordBot(
        builder.Configuration["Discord:Token"]!,
        ulong.Parse(builder.Configuration["Discord:UserId"]!),
        sp.GetRequiredService<IServiceScopeFactory>()
    )
);

builder.Services.AddHostedService<DiscordBotHostedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();