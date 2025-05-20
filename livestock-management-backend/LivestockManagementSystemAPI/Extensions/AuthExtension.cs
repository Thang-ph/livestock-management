using BusinessObjects;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Security.Claims;

namespace LivestockManagementSystemAPI.Extensions
{
    public static class AuthExtension
    {
        public static IServiceCollection AddIdentityHandlerAndStores(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Cấu hình Identity không sử dụng cookie-based authentication
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<LmsContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<VietnameseIdentityErrorDescriber>();

            return services;
        }

        public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {

                //Cấu hình về Password
                options.Password.RequireDigit = true; // bắt phải có số
                options.Password.RequireLowercase = true; // Không bắt phải có chữ thường
                options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                options.Password.RequiredLength = 8; // Số ký tự tối thiểu của password
                options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt


                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Khóa 1 phút
                options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lần thì khóa
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@-_+. ĐđĂăÂâÊêÔôƠơƯưáàảãạâấầẩẫậăắằẳẵặéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵ";


                options.User.RequireUniqueEmail = true;  // Email là duy nhất

            });

            return services;
        }

        // Lớp ghi đè các thông báo lỗi mặc định sang tiếng Việt
        public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
        {
            public override IdentityError DefaultError() { return new IdentityError { Code = nameof(DefaultError), Description = "Đã xảy ra lỗi." }; }
            public override IdentityError ConcurrencyFailure() { return new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Lỗi đồng thời, đối tượng đã được sửa đổi." }; }
            public override IdentityError PasswordMismatch() { return new IdentityError { Code = nameof(PasswordMismatch), Description = "Mật khẩu không chính xác." }; }
            public override IdentityError InvalidToken() { return new IdentityError { Code = nameof(InvalidToken), Description = "Mã token không hợp lệ." }; }
            public override IdentityError LoginAlreadyAssociated() { return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Tài khoản đăng nhập này đã tồn tại." }; }
            public override IdentityError InvalidUserName(string userName) { return new IdentityError { Code = nameof(InvalidUserName), Description = $"Tên người dùng '{userName}' không hợp lệ, chỉ có thể chứa chữ cái hoặc chữ số." }; }
            public override IdentityError InvalidEmail(string email) { return new IdentityError { Code = nameof(InvalidEmail), Description = $"Email '{email}' không hợp lệ." }; }
            public override IdentityError DuplicateUserName(string userName) { return new IdentityError { Code = nameof(DuplicateUserName), Description = $"Tên đăng nhập '{userName}' đã tồn tại." }; }
            public override IdentityError DuplicateEmail(string email) { return new IdentityError { Code = nameof(DuplicateEmail), Description = $"Email '{email}' đã tồn tại." }; }
            public override IdentityError InvalidRoleName(string role) { return new IdentityError { Code = nameof(InvalidRoleName), Description = $"Tên vai trò '{role}' không hợp lệ." }; }
            public override IdentityError DuplicateRoleName(string role) { return new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Tên vai trò '{role}' đã tồn tại." }; }
            public override IdentityError UserAlreadyHasPassword() { return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "Người dùng đã thiết lập mật khẩu." }; }
            public override IdentityError UserLockoutNotEnabled() { return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Khóa tài khoản không được kích hoạt cho người dùng này." }; }
            public override IdentityError UserAlreadyInRole(string role) { return new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Người dùng đã có vai trò '{role}'." }; }
            public override IdentityError UserNotInRole(string role) { return new IdentityError { Code = nameof(UserNotInRole), Description = $"Người dùng không có vai trò '{role}'." }; }
            public override IdentityError PasswordTooShort(int length) { return new IdentityError { Code = nameof(PasswordTooShort), Description = $"Mật khẩu phải có ít nhất {length} ký tự." }; }
            public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt." }; }
            public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Mật khẩu phải chứa ít nhất một chữ số ('0'-'9')." }; }
            public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Mật khẩu phải chứa ít nhất một chữ cái viết thường ('a'-'z')." }; }
            public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Mật khẩu phải chứa ít nhất một chữ cái viết hoa ('A'-'Z')." }; }
            public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) { return new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = $"Mật khẩu phải có ít nhất {uniqueChars} ký tự khác nhau." }; }
        }

        public static IServiceCollection AddIdentityAuth(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // Loại bỏ cấu hình DefaultSignInScheme để không tự động tạo cookie
            })
            .AddJwtBearer(options =>
            {
                // Giữ nguyên cấu hình JWT
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["JwtSettings:Audience"],
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"])),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                };

                // Add event handlers for authentication failures
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Skip the default logic
                        context.HandleResponse();

                        var payload = new
                        {
                            status = 401,
                            success = false,
                            message = "Không được phép truy cập. Vui lòng đăng nhập.",
                            details = "Bạn cần đăng nhập để truy cập tài nguyên này."
                        };

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 401;

                        var result = JsonSerializer.Serialize(payload);
                        await context.Response.WriteAsync(result);
                    },

                    OnForbidden = async context =>
                    {
                        var payload = new
                        {
                            status = 403,
                            success = false,
                            message = "Không đủ quyền truy cập.",
                            details = "Bạn không có quyền truy cập tài nguyên này."
                        };

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 403;

                        var result = JsonSerializer.Serialize(payload);
                        await context.Response.WriteAsync(result);
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Vô hiệu hóa cookies trong ASP.NET Identity
            services.ConfigureApplicationCookie(options =>
            {
                // Tắt cookie authentication
                options.Cookie.IsEssential = false;
            });

            return services;
        }

        //Middlewares
        public static WebApplication AddIdentityAuthMiddlewares(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }

    }
}
