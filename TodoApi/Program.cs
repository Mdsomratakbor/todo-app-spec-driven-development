using System.Text;
using Cartographer.Core.DependencyInjection;
using FluentResponse.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddCartographer(cfg =>
{
    cfg.PreserveReferences = true;
    cfg.MaxDepth = 3;
    new TodoApi.Models.Mappings.MappingProfile().Apply(cfg);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<Microsoft.AspNetCore.Identity.IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TodoApi.Services.Interfaces.IAuthService, TodoApi.Services.AuthService>();
builder.Services.AddScoped<TodoApi.Services.Interfaces.ICategoryService, TodoApi.Services.CategoryService>();
builder.Services.AddScoped<TodoApi.Services.Interfaces.ITodoService, TodoApi.Services.TodoService>();

builder.Services.AddFluentResponse();

builder.Services.AddControllers();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting with environment: {Env}", app.Environment.EnvironmentName);
logger.LogInformation("CORS policy {CorsPolicy} configured for {CorsOrigin}", "AllowAngularDev", "http://localhost:4200");

app.UseHttpsRedirection();

app.UseFluentResponseExceptionHandler();
app.UseFluentResponseCorrelationId();

app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
