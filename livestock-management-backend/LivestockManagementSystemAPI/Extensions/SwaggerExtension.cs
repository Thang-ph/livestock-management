using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Xml.XPath;

namespace LivestockManagementSystemAPI.Extensions
{
    public static class SwaggerExtension
    {
        public static IServiceCollection AddSwaggerExplorer(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Livestock Management System API",
                    Version = "v1",
                    Description = "API cho hệ thống quản lý gia súc",
                    Contact = new OpenApiContact
                    {
                        Name = "LMS Team",
                        Email = "support@lms.com"
                    }
                });

                // Thêm XML comments vào Swagger
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                options.IncludeXmlComments(xmlPath);

                // Cấu hình nhóm API theo controller
                options.DocInclusionPredicate((docName, apiDesc) => true);
                options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });

                // Sắp xếp các tags/nhóm theo tên
                options.OrderActionsBy(apiDesc => apiDesc.RelativePath);

                // Bật tính năng tìm kiếm và filter
                options.EnableAnnotations();

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Điền vào token JWT"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<String>()
                    }
                });

                // Cấu hình hiển thị cho tham số
                options.DescribeAllParametersInCamelCase();
                options.CustomSchemaIds(type => type.Name);
            });
            return services;
        }

        public static WebApplication AddSwaggerExplorerMiddleware(this WebApplication app)
        {
            // Luôn bật Swagger bất kể môi trường nào
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Livestock Management System API v1");
                options.DocumentTitle = "Livestock Management System API";

                // Hiển thị các API mở rộng mặc định (List) để dễ dàng nhìn thấy các nhóm
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);

                // Ẩn phần mô hình Schema mặc định để làm gọn giao diện
                options.DefaultModelsExpandDepth(-1);

                // Bật tính năng hiển thị filter
                options.EnableFilter();

                // Bật tính năng tìm kiếm theo URL để thuận tiện bookmark
                options.EnableDeepLinking();

                // Hiển thị thời gian request và thông tin mở rộng
                options.DisplayRequestDuration();

                // Ẩn các extensions
                options.ShowExtensions();

                // Cấu hình hiển thị model
                options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);

                // Ẩn OperationId (phần thông tin ở bên phải mỗi API)
                options.DisplayOperationId();

                // Thêm CSS để cải thiện giao diện và đảm bảo thanh filter hiển thị
                options.HeadContent = @"
                    <style>
                        .swagger-ui .topbar { background-color: #1a237e; }
                        .swagger-ui .info .title { font-size: 2em; color: #333; }
                        .swagger-ui .opblock-tag { font-size: 16px; margin: 10px 0; }
                        .swagger-ui .opblock-tag:hover { background-color: #f8f9fa; }
                        
                        /* Tăng kích thước và nổi bật hộp tìm kiếm */
                        .swagger-ui .filter input[type=text] {
                            width: 100% !important;
                            min-width: 250px !important;
                            padding: 8px !important;
                            border: 2px solid #1a237e !important;
                            border-radius: 5px !important;
                            font-size: 14px !important;
                        }
                        
                        /* Đảm bảo thanh filter luôn hiển thị */
                        .swagger-ui .filter-container {
                            display: block !important;
                            margin: 15px 0 !important;
                            padding: 10px !important;
                            background-color: #f8f9fa !important;
                            border-radius: 5px !important;
                        }
                        
                        /* Thêm tiêu đề cho thanh filter */
                        .swagger-ui .filter-container:before {
                            content: 'Tìm kiếm API:' !important;
                            display: block !important;
                            margin-bottom: 5px !important;
                            font-weight: bold !important;
                            color: #1a237e !important;
                        }

                        /* Ẩn phần operation ID bên phải các API */
                        .swagger-ui .opblock .opblock-summary-operation-id,
                        .swagger-ui .opblock .opblock-summary-path-description {
                            display: none !important;
                        }
                        
                        /* Ẩn hoàn toàn phần thông tin bên phải của API */
                        .swagger-ui .opblock .opblock-summary__right {
                            display: none !important;
                        }
                    </style>
                    <script>
                        window.onload = function() {
                            // Đảm bảo thanh filter được hiển thị
                            var filterContainer = document.querySelector('.filter-container');
                            if (filterContainer) {
                                filterContainer.style.display = 'block';
                                var filterInput = document.querySelector('.operation-filter-input');
                                if (filterInput) {
                                    filterInput.placeholder = 'Nhập từ khóa để tìm kiếm API...';
                                }
                            }
                        };
                    </script>
                ";
            });

            return app;
        }
    }
}
