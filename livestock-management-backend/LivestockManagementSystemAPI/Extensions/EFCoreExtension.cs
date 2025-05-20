using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace LivestockManagementSystemAPI.Extensions
{
    public static class EFCoreExtension
    {
        public static IServiceCollection AddDBContextService(this IServiceCollection services, IConfiguration config)
        {
            // Lấy chuỗi kết nối từ cấu hình, ưu tiên RDSConnection
            var rdsConnection = config.GetConnectionString("RDSConnection");
            var localConnection = config.GetConnectionString("LocalConnection");

            var connectionString = string.IsNullOrEmpty(rdsConnection) ? localConnection : rdsConnection;

            services.AddDbContext<LmsContext>(options =>
                options.UseSqlServer(connectionString));
            return services;
        }
    }
}
