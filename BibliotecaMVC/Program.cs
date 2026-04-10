using BibliotecaMVC.Models;
using BibliotecaMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Configuración principal de la aplicación BibliotecaMVC.
/// Orquesta la inyección de dependencias, la configuración de seguridad (Identity)
/// y el pipeline de procesamiento de solicitudes HTTP.
/// </summary>
var builder = WebApplication.CreateBuilder(args);
// Registro del Contexto de Datos (Entity Framework Core)
builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de Identity (Gestión de Usuarios, Roles y Seguridad)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    
    // Configuración de Lockout
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<BibliotecaContext>();

// 🔹 Registrar Servicio Transaccional de Correos. Reemplaza el Mock de Identity por el nuestro.
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, BibliotecaMVC.Services.EmailSender>();

// 🔹 Registrar Servicio Transaccional de SMS Móvil (Twilio).
builder.Services.AddTransient<BibliotecaMVC.Services.ISmsSender, BibliotecaMVC.Services.TwilioSmsSender>();

// 🤖 Inyectar Motor en Segundo Plano (Cron Job SMS)
builder.Services.AddHostedService<BibliotecaMVC.Services.SmsBackgroundWorker>();

// Servicios de validación
builder.Services.AddScoped<IUserValidationService, UserValidationService>();

// Registro de Controladores con Vistas (MVC) e infraestructura de Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

/// <summary>
/// Construcción de la aplicación y configuración del pipeline.
/// </summary>
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = services.GetRequiredService<IConfiguration>();

    string[] roles = { "Admin", "Usuario" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 🔹 Crear Admin inicial desde configuración para seguridad
    string adminEmail = configuration["AdminSettings:Email"] ?? "dgomezpulid@outlook.com";
    string adminPassword = configuration["AdminSettings:Password"];

    if (!string.IsNullOrEmpty(adminPassword))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nombre = "Administrador",
                Apellido = "General",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newAdmin, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

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

app.MapStaticAssets();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Inicia el ciclo de vida de la aplicación y escucha peticiones entrantes.
app.Run();
