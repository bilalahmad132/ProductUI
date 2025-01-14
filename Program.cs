using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Set the license context for EPPlus (NonCommercial or Commercial)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Adjust if you have a commercial license

// Add services to the container.
builder.Services.AddControllersWithViews();

// Enable CORS with a policy that allows any origin, method, and header
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add IHttpClientFactory to the services
builder.Services.AddHttpClient();

var app = builder.Build();

// Use the CORS policy
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS
app.UseStaticFiles();       // Serve static files like CSS, JS, images, etc.
app.UseRouting();           // Enable routing for the app
app.UseAuthorization();     // Enable authorization middleware

// Define the default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the application
app.Run();
