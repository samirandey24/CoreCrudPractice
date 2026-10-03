using CoreCrudWithJwt.Data;
using CoreCrudWithJwt.Exception;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Check services folder for implementation
builder.Services.AddMemoryCache();

// Check Exception folder for implementation to catch exceptions from all downstream middlewares and controllers
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("SamiranConnection")));

//old approach for new check services folder
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;

    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("iDnwDtSY2mMriFtLboeGIOCBTKKpjCXIxopMRlSv11w"))
    };
});

// 1. Register CORS Services and define a policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowedOrigins", policy =>
    {
        policy.WithOrigins("https://example.com", "http://localhost:3000") // Trust specific frontend origins
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

// 1 Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "text/html";//application/json
        var exceptionObject = context.Features.Get<IExceptionHandlerFeature>();
        if (null != exceptionObject)
        {
            var errorMessage = $"<b>Exception Error: {exceptionObject.Error.Message}</b> {exceptionObject.Error.StackTrace}";
            await context.Response.WriteAsync(errorMessage).ConfigureAwait(false);
        }
    });
});
// 2 Check Exception folder for implementation to catch exceptions from all downstream middlewares and controllers
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//Option A: Apply Globally(Shown below)
app.UseCors("MyAllowedOrigins");
//Apply to Specific Endpoints (Minimal APIs)
app.UseCors(); // Enables CORS mechanism but enforces nothing globally

app.MapGet("/api/public-data", () => "Hello")
   .RequireCors("MyAllowedOrigins"); // Enforces policy here

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
