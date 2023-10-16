using Bookify.Web.Core.Mapping;
using Bookify.Web.Core.Models;
using Bookify.Web.Data;
using Bookify.Web.Filters;
using Bookify.Web.Helpers;
using Bookify.Web.Seeds;
using Bookify.Web.Services;
using Bookify.Web.Settings;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser,IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
});
builder.Services.AddDataProtection().SetApplicationName(nameof(Bookify));
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
builder.Services.AddExpressiveAnnotations();
builder.Services.AddWhatsAppApiClient(builder.Configuration);
builder.Services.Configure<SecurityStampValidatorOptions>(option =>
{
    option.ValidationInterval = TimeSpan.Zero;
});

builder.Services.Configure<AuthorizationOptions>(options =>
options.AddPolicy("AdminsOnly", policy =>
{
	policy.RequireAuthenticatedUser();
	policy.RequireRole(AppRoles.Admin);
}));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var scope = scopeFactory.CreateScope();

var roleManger = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManger = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

await DefaultRoles.SeedAsync(roleManger);
await DefaultUsers.SeedAdminUserAsync(userManger);
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
	DashboardTitle = "Bookify Dashboard",
	IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new IDashboardAuthorizationFilter[]
    {
        new HangfireAuthorizationFilter("AdminsOnly")
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
