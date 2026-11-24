using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using HowsYourDay.Server.Components;
using HowsYourDay.Server.Data;
using HowsYourDay.Server.Extensions;
using HowsYourDay.Server.Models;
using HowsYourDay.Server.Options;
using HowsYourDay.Server.Repositories;
using HowsYourDay.Server.Services;
using HowsYourDay.Server.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;

namespace HowsYourDay.Server
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var migrate = args.Contains("--migrate", StringComparer.Ordinal);
            var builder = WebApplication.CreateBuilder(args.Where(arg => arg != "--migrate").ToArray());

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

            var database = new NpgsqlConnectionStringBuilder(connectionString)
            {
                IncludeErrorDetail = false,
                LogParameters = false
            };

            if (migrate)
                return await MigrateAsync(database.ConnectionString);

            builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 64 * 1024);

            var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
            var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
            var appSection = builder.Configuration.GetSection(AppOptions.SectionName);
            var appOptions = appSection.Get<AppOptions>() ?? new AppOptions();
            IPAddress? trustedProxy = null;
            if (!string.IsNullOrWhiteSpace(appOptions.TrustedProxy) &&
                !IPAddress.TryParse(appOptions.TrustedProxy, out trustedProxy))
                throw new InvalidOperationException("App:TrustedProxy must be an IP address.");

            if (trustedProxy is not null)
            {
                builder.Services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    options.ForwardLimit = 1;
                    options.KnownIPNetworks.Clear();
                    options.KnownProxies.Clear();
                    options.KnownProxies.Add(trustedProxy);
                });
            }

            if (!string.IsNullOrWhiteSpace(appOptions.DataProtectionKeyPath))
            {
                builder.Services.AddDataProtection()
                    .SetApplicationName("HowsYourDay")
                    .PersistKeysToFileSystem(new DirectoryInfo(appOptions.DataProtectionKeyPath));
            }

            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<PrivacySafeExceptionHandler>();
            builder.Services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"], timeout: TimeSpan.FromSeconds(3));

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("Auth", context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));
                options.AddPolicy("JournalWrite", context => HttpMethods.IsGet(context.Request.Method)
                    ? RateLimitPartition.GetNoLimiter(Guid.Empty)
                    : RateLimitPartition.GetFixedWindowLimiter(
                        context.User.GetUserId(),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 12,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

            if (builder.Environment.IsDevelopment())
            {
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddSwaggerGen(option =>
                {
                    option.SwaggerDoc("v1", new OpenApiInfo { Title = "HowsYourDayAPI", Version = "v1" });
                    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter a valid token",
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });
                    option.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
                });
            }

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("Ui", policy => policy
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                    .RequireAuthenticatedUser())
                .AddPolicy("Api", policy => policy
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser());

            builder.Services.AddOptions<JwtOptions>()
                .Bind(jwtSection)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddOptions<AppOptions>()
                .Bind(appSection)
                .ValidateDataAnnotations()
                .Validate(
                    options => TimeZoneHelper.IsValid(options.DefaultTimeZone),
                    "App:DefaultTimeZone must be a valid time zone.")
                .ValidateOnStart();

            // Set up database context
            builder.Services.AddDbContext<HowsYourDayAppDbContext>(options =>
                options.UseNpgsql(database.ConnectionString));

            builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                // There will be no emails, only username
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            }).AddEntityFrameworkStores<HowsYourDayAppDbContext>();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            builder.Services.AddAuthentication().AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha512],
                };
            });

            builder.Services.AddSingleton(TimeProvider.System);

            builder.Services.AddScoped<IDayEntryService, DayEntryService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IUserTimeZoneService, UserTimeZoneService>();
            builder.Services.AddSingleton<INlpService, NlpService>();

            builder.Services.AddScoped<IDayEntryRepository, DayEntryRepository>();
            builder.Services.AddScoped<IDayEntryAnalysisRepository, DayEntryAnalysisRepository>();

            var app = builder.Build();

            app.UseExceptionHandler();

            if (trustedProxy is not null)
                app.UseForwardedHeaders();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (appOptions.HttpsRedirectionEnabled && trustedProxy is null)
                app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            app.MapAccountFormEndpoints();
            app.MapControllers();
            app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
                .AllowAnonymous();
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            }).AllowAnonymous();

            await app.RunAsync();
            return 0;
        }

        private static async Task<int> MigrateAsync(string connectionString)
        {
            var options = new DbContextOptionsBuilder<HowsYourDayAppDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            await using var context = new HowsYourDayAppDbContext(options);

            try
            {
                await context.Database.MigrateAsync();
                Console.WriteLine("Database migrations applied.");
                return 0;
            }
            catch (PostgresException exception)
            {
                Console.Error.WriteLine($"Database migration failed (PostgreSQL SQLSTATE {exception.SqlState}).");
                return 1;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Database migration failed ({exception.GetType().Name}).");
                return 1;
            }
        }
    }
}
