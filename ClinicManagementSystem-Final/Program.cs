using ClinicManagementSystem_Final.Repository;
using ClinicManagementSystem_Final.Service;

namespace ClinicManagementSystem2025
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // replace the existing AddControllersWithViews() call with this:
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.ResponseCacheAttribute
                {
                    NoStore = true,
                    Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None
                });
            });

            //  Add distributed cache (required by session)
            builder.Services.AddDistributedMemoryCache();

            //  Add session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 1 - Add connection string
            var connectionString = builder.Configuration.GetConnectionString("MVCConnectionString");

            // 2 - Dependency injection for repositories & services
            builder.Services.AddScoped<IUserService, UserServiceImpl>();
            builder.Services.AddScoped<IUserRepository, UserRepositoryImpl>();
            builder.Services.AddScoped<IDoctorService, DoctorServiceImpl>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepositoryImpl>();

            builder.Services.AddScoped<IStaffService, StaffServiceImpl>();
            builder.Services.AddScoped<IStaffRepository, StaffRepositoryImpl>();

            builder.Services.AddScoped<IReceptionistRepository, ReceptionistRepositoryImpl>();
            builder.Services.AddScoped<IReceptionistService, ReceptionistServiceImpl>();

            // Lab Technician DI
            builder.Services.AddScoped<ILabTechnicianRepository, LabTechnicianRepository>();
            builder.Services.AddScoped<ILabTechnicianService, LabTechnicianService>();

            // Pharmacist DI
            builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
            builder.Services.AddScoped<IMedicineService, MedicineService>();
            builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            

            //  Add session middleware BEFORE routing endpoints
            app.UseSession();

            // register no-cache middleware for authenticated pages
            app.UseMiddleware<ClinicManagementSystem_Final.Middleware.NoCacheAuthenticatedMiddleware>();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}