using System.Runtime.InteropServices;
using System.Security.Principal;

// Check OS Platform and ensure administrator role.
var name = AppDomain.CurrentDomain.FriendlyName;
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
	throw new Exception($"{name} is designed to run on Windows");
}

using (var identity = WindowsIdentity.GetCurrent())
{
	var principal = new WindowsPrincipal(identity);
	if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
	{
		throw new Exception($"{name} must be run as administrator");
	}
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
	name: "default",
	pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");
;

app.Run();