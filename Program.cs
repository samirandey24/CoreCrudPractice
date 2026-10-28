using Azure.Core;
using CoreCrudWithJwt.Data;
using CoreCrudWithJwt.Exception;
using CoreCrudWithJwt.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

//var app = builder.Build();

//// 1. Global Exception Handling (Must catch downstream errors)
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
//else
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts(); // Strictly HTTP Strict Transport Security
//}

//// 2. Security & Redirections
//app.UseHttpsRedirection();
//app.UseForwardedHeaders(); // Essential if behind a reverse proxy (Nginx, IIS, YARP)

//// 3. Performance & Content Delivery
//app.UseStaticFiles(); // Serves images, CSS, JS quickly and skips remaining pipeline
//app.UseCookiePolicy();

//// 4. Structural Decisions & Network Access
//app.UseRouting(); // Matches request to endpoint (Sets Endpoint metadata)
//app.UseCors("MyCorsPolicy"); // Must sit AFTER UseRouting but BEFORE Auth

//// 5. Security Checkpoints
//app.UseAuthentication(); // "Who are you?" (Must be before Authorization)
//app.UseAuthorization();  // "Are you allowed here?"
//app.UseSession();

//// 6. Custom Middleware
//// Place custom logic here if it requires Routing/Auth context
//// app.UseMiddleware<MyCustomMiddleware>(); 

//// 7. Execution Phase
//app.MapControllers(); // Executes the matched controller action/Razor page
//app.MapRazorPages();

//app.Run();
//---------------------------------------------------------------------------------------

//What is Middleware ?
//Middleware is a core functionality of Asp.net Core. It is nothing but a class or component that will execute on each request.

//Implementing Custom Middleware

//There are two primary ways to create custom middleware: Inline(Anonymous) for simple tasks and a Class - Based approach for reusable architecture.

//Option A: Inline Middleware(Using app.Use)

//Perfect for quick logging, diagnostic timing, or adding headers.
//app.Use(async (context, next) =>
//{
//    // 1. Code here runs on the INCOMING request
//    Console.WriteLine($"Incoming request: {context.Request.Path}");

//    await next(context); // 2. Call the next middleware in line

//    // 3. Code here runs on the OUTGOING response
//    Console.WriteLine($"Outgoing response status: {context.Response.StatusCode}");
//});


//Option B: Convention - Based Middleware Class

//Best for clean, maintainable architecture. It expects a RequestDelegate in the constructor and an InvokeAsync method

//public class SecurityHeadersMiddleware
//{
//    private readonly RequestDelegate _next;

//    public SecurityHeadersMiddleware(RequestDelegate next)
//    {
//        _next = next;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        // Add a security header before passing the request down
//        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

//        await _next(context); // Pass to the next middleware
//    }
//}

//// Registration in Program.cs
//app.UseMiddleware<SecurityHeadersMiddleware>();
//-------------------------------------------------------------------------------------------------------------

//ASP.NET Core provides three ways to add middleware.
//Use, Run, and Map
//Use() Calls the next middleware:

//app.Use(async (context, next) =>
//{
//    logger.LogInformation("Request received"); //Pre-processing: Log the incoming request
//    await next(); // 2. Pass control to the next middleware
//    Console.WriteLine("After");  // 3. Post-processing: Log the outgoing response status
//});

//Run()Terminates pipeline and does not call next:

//app.Run(async context =>
//{
//    await context.Response.WriteAsync("Request handled.");
//});

//Map() Branches pipeline based on path:

//app.Map("/admin", adminApp =>
//{
//    This custom sub-pipeline only runs for requests starting with /admin
//    adminApp.UseAdminAuthentication();

//    adminApp.Run(async context =>
//    {
//        await context.Response.WriteAsync("Admin Panel");
//    });
//});



//Need Conditional Branching that Rejoins?

//If you want to conditionally run middleware but want the request to flow back into the main pipeline afterward (instead of hitting a dead-end branch), you should look into app.UseWhen().
//• app.MapWhen() behaves like Map but uses a custom boolean predicate (like checking a query string or a header) instead of a URL path, and it never rejoins the main pipeline.
//• app.UseWhen() uses a custom boolean predicate but rejoins the main pipeline as long as nothing inside the conditional block short-circuits the response.

//-----------------------------------------------------------------
//Middleware is used for:

//Authentication & Authorization

//Logging and Monitoring

//Error Handling

//Caching

//Compression

//Rate Limiting

//CORS

//Security Headers

//Request Validation