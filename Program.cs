using GestionLaboresAcademicas.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GestionLaboresAcademicas.Services;
using QuestPDF.Infrastructure;

namespace GestionLaboresAcademicas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            QuestPDF.Settings.License = LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<ServicioGestionUsuarios>();
            builder.Services.AddScoped<ServicioEstadisticas>();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/AccesoDenegado";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("EsDirector", policy => policy.RequireRole("Director"));
                options.AddPolicy("EsSecretaria", policy => policy.RequireRole("Secretaria"));
                options.AddPolicy("EsDocente", policy => policy.RequireRole("Docente"));
                options.AddPolicy("EsEstudiante", policy => policy.RequireRole("Estudiante"));
                options.AddPolicy("EsPadre", policy => policy.RequireRole("Padre"));
                options.AddPolicy("EsRegente", policy => policy.RequireRole("Regente"));
                options.AddPolicy("EsBibliotecario", policy => policy.RequireRole("Bibliotecario"));
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
