using BusinessObjects.ConfigModels;
using BusinessObjects.Constants;
using CloudinaryDotNet;
using DataAccess.AutoServices;
using DataAccess.Repository.Interfaces;
using DataAccess.Repository.Services;
using LivestockManagementSystemAPI.Authorization;
using LivestockManagementSystemAPI.Controllers;
using LivestockManagementSystemAPI.Helper.QRCodeGeneratorHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LivestockManagementSystemAPI.Extensions
{
    public static class AppConfigExtension
    {
        public static IServiceCollection AddAppConfig(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<JwtSetting>(config.GetSection("JwtSettings"));
            services.AddTransient<IUserRepository, UserService>();
            services.AddScoped<IMedicalRepository, MedicalService>();
            services.AddScoped<ISpecieRepository, SpecieService>();
            services.AddScoped<IProcurementRepository, ProcurementService>();
            services.AddSingleton<TokenSessionService>();
            services.AddScoped<ILivestockRepository, LivestockService>();
            services.AddScoped<IQRCodeGeneratorHelper, QRCodeGeneratorHelper>();
            services.AddScoped<IBatchExportRepository, BatchExportService>();
            services.AddScoped<IBarnRepository, BarnService>();
            services.AddScoped<IImportRepository, ImportService>();
            services.AddScoped<IBatchVacinationRepository, BatchVacinationService>();
            services.AddScoped<IMedicalHistoriesRepository, MedicalHistoriesService>();
            services.AddScoped<IDiseaseRepository, DiseaseService>();
            services.AddScoped<JwtSecurityTokenHandler>();
            services.AddScoped<IInspectionCodeCounterRepository, InspectionCodeCounterService>();
            services.AddScoped<IInspectionCodeRangeRepository, InspectionCodeRangeService>();
            services.AddScoped<IInsuranceRequestRepository, InsuranceRequestService>();
            services.AddScoped<IOderRepository, OrderService>();
            services.AddScoped<ICustomerRepository, CustomerService>();
            // Tạm thời vô hiệu hóa service cập nhật cân nặng vật nuôi do gây lỗi
            // services.AddHostedService<UpdateLivestockWeightService>();

            // Cấu hình Email
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();

            // Cấu hình SMS
            services.Configure<SmsSettings>(config.GetSection("SmsSettings"));

            // Cấu hình Cloudinary
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            // Đăng ký Cloudinary client với DI
            var cloudinarySettings = config.GetSection("CloudinarySettings").Get<CloudinarySettings>();
            var account = new Account(
                cloudinarySettings?.CloudName,
                cloudinarySettings?.ApiKey,
                cloudinarySettings?.ApiSecret
            );
            var cloudinary = new CloudinaryDotNet.Cloudinary(account);
            services.AddSingleton(cloudinary);

            // Đăng ký repository sử dụng Cloudinary
            services.AddScoped<ICloudinaryRepository, CloudinaryService>();

            // Đăng ký dịch vụ SMS dựa trên cấu hình
            string provider = config["SmsSettings:Provider"] ?? string.Empty;

            if (string.Equals(provider, "ESMS", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<ISmsService, EsmsService>();
            }
            else if (string.Equals(provider, "Twilio", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<ISmsService, TwilioSmsService>();
            }
            else
            {
                services.AddScoped<ISmsService, EsmsService>();
            }

            // Add permission repositories
            services.AddScoped<IUserManagementRepository, UserManagementService>();
            services.AddScoped<IPermissionRepository, PermissionService>();

            // Register the permission authorization handler
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
                //options.AddPolicy("AllowLocalhost3000", policyBuilder =>
                //{
                //    policyBuilder
                //        .WithOrigins("https://localhost:3000", "http://localhost:3000")
                //        .AllowAnyHeader()
                //        .AllowAnyMethod()
                //        .AllowCredentials();
                //});
            });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
            return services;
        }



        public static WebApplication ConfigureCORSMiddlewares(this WebApplication app)
        {
            // app.UseCors(builder =>
            // {
            //     builder.AllowAnyOrigin()
            //            .AllowAnyHeader()
            //            .AllowAnyMethod()
            //            .AllowCredentials();
            // });
            app.UseCors("AllowAll");
            //app.UseCors("AllowLocalhost3000");
            return app;
        }


        public static WebApplication SeedRoles(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                // Danh sách role cần xóa
                string[] rolesToDelete = { "Quản trị viên", "Kế toán" };

                foreach (var role in rolesToDelete)
                {
                    if (roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    {
                        var roleToDelete = roleManager.FindByNameAsync(role).GetAwaiter().GetResult();
                        roleManager.DeleteAsync(roleToDelete).GetAwaiter().GetResult();
                    }
                }

                // Danh sách role cần seed cùng với priority tương ứng
                Dictionary<string, int> rolesToSeed = new Dictionary<string, int>
                {
                    { "Giám đốc", 1 },
                    { "Quản trại", 2 },
                    { "Trợ lý", 3 },
                    { "Nhân viên trại", 4 }
                };

                foreach (var roleInfo in rolesToSeed)
                {
                    var roleName = roleInfo.Key;
                    var priority = roleInfo.Value;

                    // Tạo role nếu chưa tồn tại
                    if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                    {
                        var result = roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                        if (!result.Succeeded)
                        {
                            throw new Exception($"Không thể tạo vai trò {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }

                    // Lấy role và thiết lập priority
                    var role = roleManager.FindByNameAsync(roleName).GetAwaiter().GetResult();

                    // Xóa claim Priority cũ nếu có
                    var existingClaims = roleManager.GetClaimsAsync(role).GetAwaiter().GetResult();
                    var priorityClaim = existingClaims.FirstOrDefault(c => c.Type == "Priority");
                    if (priorityClaim != null)
                    {
                        roleManager.RemoveClaimAsync(role, priorityClaim).GetAwaiter().GetResult();
                    }

                    // Thêm claim Priority mới
                    roleManager.AddClaimAsync(role, new Claim("Priority", priority.ToString())).GetAwaiter().GetResult();
                }

                // Seed tài khoản CEO mặc định từ cấu hình CeoAccount
                var ceoAccount = configuration.GetSection("CeoAccount");
                var ceoEmail = ceoAccount["Email"];
                var ceoUsername = ceoAccount["Username"];
                var ceoPassword = ceoAccount["Password"];

                if (string.IsNullOrEmpty(ceoEmail) || string.IsNullOrEmpty(ceoPassword))
                {
                    throw new Exception("CEO account configuration is missing in appsettings.json");
                }

                var ceoUser = userManager.FindByEmailAsync(ceoEmail).GetAwaiter().GetResult();
                if (ceoUser == null)
                {
                    ceoUser = new IdentityUser
                    {
                        UserName = ceoUsername ?? "Giám đốc",
                        Email = ceoEmail,
                        PhoneNumber = ceoAccount["PhoneNumber"],
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };

                    var result = userManager.CreateAsync(ceoUser, ceoPassword).GetAwaiter().GetResult();
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(ceoUser, "Giám đốc").GetAwaiter().GetResult();
                    }
                    else
                    {
                        throw new Exception($"Không thể tạo tài khoản Giám đốc: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    userManager.AddToRoleAsync(ceoUser, "Giám đốc").GetAwaiter().GetResult();
                }
            }
            return app;
        }

        public static WebApplication SeedPermissionClaims(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Seed full permissions for CEO role (Admin role)
                var ceoRole = roleManager.FindByNameAsync("Giám đốc").GetAwaiter().GetResult();
                if (ceoRole != null)
                {
                    var allPermissions = Permissions.GetAllPermissions();
                    var existingClaims = roleManager.GetClaimsAsync(ceoRole).GetAwaiter().GetResult();

                    foreach (var permission in allPermissions)
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            roleManager.AddClaimAsync(ceoRole, new Claim("Permission", permission)).GetAwaiter().GetResult();
                        }
                    }
                }

                // Seed permissions for Trợ lý role
                var assistantRole = roleManager.FindByNameAsync("Trợ lý").GetAwaiter().GetResult();
                if (assistantRole != null)
                {
                    // Define permissions for assistant
                    var assistantPermissions = new List<string>
                    {
                        // Procurement permissions
                        Permissions.ViewProcurement,
                        Permissions.CreateProcurement,
                        Permissions.EditProcurement,
                        
                        // Export permissions
                        Permissions.ViewExports,
                        Permissions.CreateExports,
                    };

                    var existingClaims = roleManager.GetClaimsAsync(assistantRole).GetAwaiter().GetResult();

                    foreach (var permission in assistantPermissions)
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            roleManager.AddClaimAsync(assistantRole, new Claim("Permission", permission)).GetAwaiter().GetResult();
                        }
                    }
                }

                // Seed permissions for Quản trại role
                var managerRole = roleManager.FindByNameAsync("Quản trại").GetAwaiter().GetResult();
                if (managerRole != null)
                {
                    // Define permissions for farm manager
                    var managerPermissions = new List<string>
                    {
                        // Livestock permissions
                        Permissions.ViewLivestock,
                        Permissions.CreateLivestock,
                        Permissions.EditLivestock,
                        
                        // Species permissions
                        Permissions.ViewSpecies,
                        Permissions.CreateSpecies,
                        Permissions.EditSpecies,
                        
                        // Medical permissions
                        Permissions.ViewMedical,
                        Permissions.CreateMedical,
                        Permissions.EditMedical,
                        
                        // Procurement permissions
                        Permissions.ViewProcurement,
                        
                        // Export permissions
                        Permissions.ViewExports,
                        Permissions.CreateExports,
                    };

                    var existingClaims = roleManager.GetClaimsAsync(managerRole).GetAwaiter().GetResult();

                    foreach (var permission in managerPermissions)
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            roleManager.AddClaimAsync(managerRole, new Claim("Permission", permission)).GetAwaiter().GetResult();
                        }
                    }
                }

                // Seed permissions for Nhân viên trại role
                var staffRole = roleManager.FindByNameAsync("Nhân viên trại").GetAwaiter().GetResult();
                if (staffRole != null)
                {
                    // Define permissions for farm staff
                    var staffPermissions = new List<string>
                    {
                        // Livestock permissions
                        Permissions.ViewLivestock,
                        
                        // Species permissions
                        Permissions.ViewSpecies,
                        
                        // Medical permissions
                        Permissions.ViewMedical,
                        
                        // Procurement permissions
                        Permissions.ViewProcurement,
                        
                        // Export permissions
                        Permissions.ViewExports,
                    };

                    var existingClaims = roleManager.GetClaimsAsync(staffRole).GetAwaiter().GetResult();

                    foreach (var permission in staffPermissions)
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            roleManager.AddClaimAsync(staffRole, new Claim("Permission", permission)).GetAwaiter().GetResult();
                        }
                    }
                }
            }
            return app;
        }
    }
}
