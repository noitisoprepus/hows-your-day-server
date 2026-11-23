using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using HowsYourDay.Server.Extensions;
using Npgsql;
using HowsYourDay.Server.Data;
using HowsYourDay.Server.Models;
using HowsYourDay.Server.Options;
using HowsYourDay.Server.Repositories;
using HowsYourDay.Server.Services;
using HowsYourDay.Server.Utilities;

namespace HowsYourDay.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

            var database = new NpgsqlConnectionStringBuilder(connectionString)
            {
                IncludeErrorDetail = false,
                LogParameters = false
            };

            builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 64 * 1024);

            var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
            var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
            var appSection = builder.Configuration.GetSection(AppOptions.SectionName);

            //// Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<PrivacySafeExceptionHandler>();
            
            // CORS for web client
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

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
                options.AddPolicy("JournalWrite", context => RateLimitPartition.GetFixedWindowLimiter(
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
                builder.Services.AddSwaggerGen();
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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            app.MapControllers();

            app.Run();
        }
    }
}
