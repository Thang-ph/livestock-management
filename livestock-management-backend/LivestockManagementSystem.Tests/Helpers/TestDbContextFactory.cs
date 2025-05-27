using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LivestockManagementSystem.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static LmsContext CreateInMemoryContext(string databaseName = null)
        {
            var dbName = databaseName ?? Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new LmsContext(options);

            // Ensure database is created
            context.Database.EnsureCreated();

            return context;
        }

        public static ServiceProvider CreateServiceProvider(LmsContext context)
        {
            var services = new ServiceCollection();

            // Add Identity services
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<LmsContext>()
                .AddDefaultTokenProviders();

            // Add AutoMapper
            services.AddAutoMapper(typeof(DataAccess.AutoMapperConfig.ConfigMapper));

            // Add DbContext
            services.AddSingleton(context);

            return services.BuildServiceProvider();
        }
    }
}