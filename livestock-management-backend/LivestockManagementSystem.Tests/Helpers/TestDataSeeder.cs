using BusinessObjects;
using BusinessObjects.Constants;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace LivestockManagementSystem.Tests.Helpers
{
    public static class TestDataSeeder
    {
        public static async Task SeedTestData(LmsContext context)
        {
            // Seed test customers
            var customer1 = new Customer
            {
                Id = "test-customer-1",
                Fullname = "Nguyễn Văn A",
                Phone = "0123456789",
                Address = "123 Test Street",
                Email = "test@example.com",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var customer2 = new Customer
            {
                Id = "test-customer-2",
                Fullname = "Trần Thị B",
                Phone = "0987654321",
                Address = "456 Test Avenue",
                Email = "test2@example.com",
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            // Seed test diseases
            var disease1 = new Disease
            {
                Id = "test-disease-1",
                Name = "Bệnh cúm gia cầm",
                Symptom = "Sốt cao, khó thở",
                Description = "Bệnh truyền nhiễm nguy hiểm",
                Type = LmsConstants.disease_type.TRUYỀN_NHIỄM_NGUY_HIỂM,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            var disease2 = new Disease
            {
                Id = "test-disease-2",
                Name = "Bệnh tiêu chảy",
                Symptom = "Tiêu chảy, mất nước",
                Description = "Bệnh thường gặp ở gia súc",
                Type = LmsConstants.disease_type.KHÔNG_TRUYỀN_NHIỄM,
                CreatedAt = DateTime.Now,
                CreatedBy = "TEST",
                UpdatedAt = DateTime.Now,
                UpdatedBy = "TEST"
            };

            // Add to context
            context.Customers.AddRange(customer1, customer2);
            context.Diseases.AddRange(disease1, disease2);

            await context.SaveChangesAsync();
        }

        public static async Task CleanupTestData(LmsContext context)
        {
            // Remove all test data
            context.Customers.RemoveRange(context.Customers);
            context.Diseases.RemoveRange(context.Diseases);

            await context.SaveChangesAsync();
        }
    }
}