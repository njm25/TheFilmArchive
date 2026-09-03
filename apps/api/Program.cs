using Api.Services;
using Infrastructure.Clients;
using Infrastructure.Data;
using Infrastructure.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped<TmdbService>(sp =>
    new TmdbService(
        builder.Configuration["Tmdb:ApiKey"]!
    )
);

builder.Services.AddScoped<FilmSyncService>();

builder.Services.AddHttpClient<ArchiveOrgService>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<SpaShellService>();

// Logged-not-sent locally so development never dispatches real mail; real SMTP
// everywhere else. Both implement IEmailSender, so callers don't know which.
if (builder.Environment.IsDevelopment()) 
    builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();
else
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddSingleton<BulkSyncJobService>();
builder.Services.AddSingleton<BulkFilmSyncService>();

builder.Services.AddSingleton<EmailQuotaService>();

// Sending mail costs money and burns SES reputation, so the endpoint that does
// it is capped per caller. Partitioned on the real client IP (see ClientIp) -
// partitioning on RemoteIpAddress would lump everyone behind Cloudflare into
// one bucket and rate limit the whole internet as a single visitor.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(RateLimitPolicies.RegistrationEmail, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ClientIp.Resolve(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int?>("Email:PerIpLimit") ?? 3,
                Window = TimeSpan.FromMinutes(
                    builder.Configuration.GetValue<int?>("Email:PerIpWindowMinutes") ?? 15
                ),
                QueueLimit = 0
            }));

    // Brute-force protection rather than cost control, so the budget is looser -
    // a real person needs a handful of tries, and an attacker gets a trickle
    // instead of thousands per minute. Kept generous on purpose: offices and
    // mobile carriers put many legitimate users behind one address.
    options.AddPolicy(RateLimitPolicies.Login, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ClientIp.Resolve(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int?>("Auth:LoginPerIpLimit") ?? 10,
                Window = TimeSpan.FromMinutes(
                    builder.Configuration.GetValue<int?>("Auth:LoginPerIpWindowMinutes") ?? 5
                ),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Too many attempts. Please wait a few minutes and try again." },
            token
        );
    };
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();