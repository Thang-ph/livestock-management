namespace BusinessObjects.Constants
{
    public static class Permissions
    {
        // User Management Permissions
        public const string ViewUsers = "Permission.Users.View";
        public const string CreateUsers = "Permission.Users.Create";
        public const string EditUsers = "Permission.Users.Edit";
        public const string DeleteUsers = "Permission.Users.Delete";
        public const string BlockUsers = "Permission.Users.Block";

        // Role Management Permissions
        public const string ViewRoles = "Permission.Roles.View";
        public const string CreateRoles = "Permission.Roles.Create";
        public const string EditRoles = "Permission.Roles.Edit";
        public const string DeleteRoles = "Permission.Roles.Delete";
        public const string ManageRolePermissions = "Permission.Roles.ManagePermissions";

        // Species Management Permissions
        public const string ViewSpecies = "Permission.Species.View";
        public const string CreateSpecies = "Permission.Species.Create";
        public const string EditSpecies = "Permission.Species.Edit";
        public const string DeleteSpecies = "Permission.Species.Delete";

        // Livestock Management Permissions
        public const string ViewLivestock = "Permission.Livestock.View";
        public const string CreateLivestock = "Permission.Livestock.Create";
        public const string EditLivestock = "Permission.Livestock.Edit";
        public const string DeleteLivestock = "Permission.Livestock.Delete";

        // Medical Management Permissions
        public const string ViewMedical = "Permission.Medical.View";
        public const string CreateMedical = "Permission.Medical.Create";
        public const string EditMedical = "Permission.Medical.Edit";
        public const string DeleteMedical = "Permission.Medical.Delete";

        // Procurement Management Permissions
        public const string ViewProcurement = "Permission.Procurement.View";
        public const string CreateProcurement = "Permission.Procurement.Create";
        public const string EditProcurement = "Permission.Procurement.Edit";
        public const string DeleteProcurement = "Permission.Procurement.Delete";

        // Export Management Permissions
        public const string ViewExports = "Permission.Exports.View";
        public const string CreateExports = "Permission.Exports.Create";
        public const string EditExports = "Permission.Exports.Edit";
        public const string DeleteExports = "Permission.Exports.Delete";

        // Dictionary ánh xạ tên module từ tiếng Anh sang tiếng Việt
        private static readonly Dictionary<string, string> ModuleTranslations = new Dictionary<string, string>
        {
            { "Users", "Người dùng" },
            { "Roles", "Vai trò" },
            { "Species", "Loài" },
            { "Livestock", "Vật nuôi" },
            { "Medical", "Y tế" },
            { "Procurement", "Gói thầu" },
            { "Exports", "Lô xuất" }
        };

        // Dictionary ánh xạ tên hành động từ tiếng Anh sang tiếng Việt
        private static readonly Dictionary<string, string> ActionTranslations = new Dictionary<string, string>
        {
            { "View", "Xem" },
            { "Create", "Thêm" },
            { "Edit", "Chỉnh sửa" },
            { "Delete", "Xoá" },
            { "Block", "Khóa" },
            { "ManagePermissions", "Quản lý phân quyền" }
        };

        // Dictionary ánh xạ tên đầy đủ của quyền
        private static readonly Dictionary<string, string> PermissionTranslations = new Dictionary<string, string>
        {
            { ViewUsers, "Xem người dùng" },
            { CreateUsers, "Thêm người dùng" },
            { EditUsers, "Chỉnh sửa người dùng" },
            { DeleteUsers, "Xoá người dùng" },
            { BlockUsers, "Khóa người dùng" },

            { ViewRoles, "Xem vai trò" },
            { CreateRoles, "Thêm vai trò" },
            { EditRoles, "Chỉnh sửa vai trò" },
            { DeleteRoles, "Xoá vai trò" },
            { ManageRolePermissions, "Quản lý phân quyền" },

            { ViewSpecies, "Xem loài" },
            { CreateSpecies, "Thêm loài" },
            { EditSpecies, "Chỉnh sửa loài" },
            { DeleteSpecies, "Xoá loài" },

            { ViewLivestock, "Xem vật nuôi" },
            { CreateLivestock, "Thêm vật nuôi" },
            { EditLivestock, "Chỉnh sửa vật nuôi" },
            { DeleteLivestock, "Xoá vật nuôi" },

            { ViewMedical, "Xem y tế" },
            { CreateMedical, "Thêm y tế" },
            { EditMedical, "Chỉnh sửa y tế" },
            { DeleteMedical, "Xoá y tế" },

            { ViewProcurement, "Xem gói thầu" },
            { CreateProcurement, "Tạo mới gói thầu" },
            { EditProcurement, "Chỉnh sửa gói thầu" },
            { DeleteProcurement, "Xoá gói thầu" },

            { ViewExports, "Xem xuất hàng" },
            { CreateExports, "Tạo xuất hàng" },
            { EditExports, "Chỉnh sửa xuất hàng" },
            { DeleteExports, "Xoá xuất hàng" }
        };

        // Lấy tên hiển thị bằng tiếng Việt cho một quyền
        public static string GetVietnamesePermissionName(string permission)
        {
            if (PermissionTranslations.TryGetValue(permission, out string translation))
                return translation;

            // Nếu không tìm thấy, thử phân tích cấu trúc Permission.Module.Action
            var parts = permission.Split('.');
            if (parts.Length >= 3)
            {
                string module = parts[1];
                string action = parts[2];

                if (ModuleTranslations.TryGetValue(module, out string moduleTranslation) &&
                    ActionTranslations.TryGetValue(action, out string actionTranslation))
                {
                    return $"{actionTranslation} {moduleTranslation.ToLower()}";
                }
            }

            return permission; // Trả về tên gốc nếu không tìm thấy bản dịch
        }

        // Returns all permissions as a list
        public static List<string> GetAllPermissions()
        {
            return typeof(Permissions)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null))
                .ToList();
        }

        // Get all permissions grouped by module
        public static Dictionary<string, List<string>> GetPermissionsByModule()
        {
            var permissions = GetAllPermissions();
            return permissions
                .GroupBy(p => p.Split('.')[1])
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );
        }

        // Get all permissions grouped by module with Vietnamese names
        public static Dictionary<string, Dictionary<string, string>> GetVietnamesePermissionsByModule()
        {
            var permissions = GetAllPermissions();
            var groupedPermissions = permissions
                .GroupBy(p => p.Split('.')[1])
                .ToDictionary(
                    g => ModuleTranslations.TryGetValue(g.Key, out string moduleTranslation) ? moduleTranslation : g.Key,
                    g => g.ToDictionary(
                        p => p,
                        p => GetVietnamesePermissionName(p)
                    )
                );

            return groupedPermissions;
        }
    }
}
