
using Pomelo.EntityFrameworkCore.MySql;
using MySqlConnector;
using MvcApp.DataModel;
using MvcApp.Controllers;
using Microsoft.EntityFrameworkCore;
using MvcApp.Models;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMvc();
// Establishing DB connection
var connection_string = builder.Configuration.GetConnectionString("DefaultConnection");
// var connection_string = NewMysqlUnixSocketConnectionString().ToString();
try
{
    MySqlConnection conn = new MySqlConnection();
    conn.ConnectionString = connection_string;
    conn.Open();
    builder.Services.AddDbContext<ApplicationDBContext>(
        options => options.UseMySql(connection_string, ServerVersion.AutoDetect(connection_string)));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
