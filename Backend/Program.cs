using System.Text;
using MbyteWeb.Server.Authorization;
using MbyteWeb.Server.Data;
using MbyteWeb.Server.Helpers;
using MbyteWeb.Server.Models.Admin;
using MbyteWeb.Server.Middlewares;
using MbyteWeb.Server.Services.Implementations;
using MbyteWeb.Server.Services.Implementations.Admin;
using MbyteWeb.Server.Services.Implementations.Auth;
using MbyteWeb.Server.Services.Interfaces;
using MbyteWeb.Server.Services.Interfaces.Admin;
using MbyteWeb.Server.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MbyteWebDb")));
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders(SessionVersionMiddleware.RenewedHeader)));
builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentity<Usuario, Rol>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
}).AddDefaultTokenProviders().AddEntityFrameworkStores<AppDbContext>();
var jwt = builder.Configuration.GetSection("JwtSettings");
var secret = jwt.GetValue<string>("SecretKey") ?? throw new InvalidOperationException("JwtSettings:SecretKey is required.");
builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; }).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidateAudience = true, ValidateIssuerSigningKey = true, ValidateLifetime = true, ValidIssuer = jwt["Issuer"], ValidAudience = jwt["Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)) });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddScoped<IAuthService, AuthService>(); builder.Services.AddScoped<IJwtService, JwtService>(); builder.Services.AddScoped<IPermissionService, PermissionService>(); builder.Services.AddScoped<IUsuariosService, UsuariosService>(); builder.Services.AddScoped<IRolesService, RolesService>(); builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); builder.Services.AddScoped<IQueryExecutor, QueryExecutor>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DefaultUsuarioSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection(); app.UseCors("Frontend"); app.UseAuthentication(); app.UseMiddleware<SessionVersionMiddleware>(); app.UseAuthorization(); app.MapControllers(); app.Run();

// Exposed for WebApplicationFactory in the integration test project (Program.cs uses top-level statements).
public partial class Program { }
