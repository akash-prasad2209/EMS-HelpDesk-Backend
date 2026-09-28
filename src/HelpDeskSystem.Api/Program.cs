using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HelpDeskSystem.Api.Data;
using HelpDeskSystem.Api.Middleware;
using HelpDeskSystem.Api.Patterns.Factory;
using HelpDeskSystem.Api.Patterns.Observer;
using HelpDeskSystem.Api.Patterns.Policy;
using HelpDeskSystem.Api.Patterns.Strategy;
using HelpDeskSystem.Api.Repositories;
using HelpDeskSystem.Api.Security;
using HelpDeskSystem.Api.Services;
using HelpDeskSystem.Api.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- Options (bound from appsettings / user-secrets) ----
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// ---- EF Core / SQL Server ----
builder.Services.AddDbContext<HelpDeskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HelpDeskDb")));

// ---- Repositories / Unit of Work ----
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ---- Patterns ----
builder.Services.AddScoped<TicketFactory>();
builder.Services.AddSingleton<AssignmentContext>(_ => new AssignmentContext(new WorkloadStrategy()));
builder.Services.AddScoped<ITicketObserver, TicketHistoryObserver>();
builder.Services.AddScoped<ITicketObserver, NotificationObserver>();
builder.Services.AddScoped<TicketEventPublisher>();
builder.Services.AddScoped<ITicketAccessPolicy, TicketAccessPolicy>();

// ---- Services ----
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRequestTypeService, RequestTypeService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ---- Security ----
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
    };
});

builder.Services.AddAuthorization();

// ---- CORS (restricted to the React app's origin) ----
const string corsPolicyName = "HelpDeskFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ---- MVC / Validation / Swagger ----
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// Without this, a validation failure returns ASP.NET Core's default ProblemDetails shape
// instead of ApiResponse<T> - breaking the "one consistent response shape" guarantee
// (Section 6.6 of the LLD) and the frontend's axiosClient error handling, which expects
// { message, errors } on every non-2xx response.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
        var response = HelpDeskSystem.Api.Common.ApiResponse<object>.Fail("Validation failed.", errors, 400);
        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---- Pipeline (order matters - Chain of Responsibility) ----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HelpDeskDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
