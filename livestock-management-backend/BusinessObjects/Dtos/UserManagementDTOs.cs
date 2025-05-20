namespace BusinessObjects.Dtos
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsLocked { get; set; }
        public IList<string> Roles { get; set; }
        public string PrimaryRole { get; set; }
        public List<RoleWithPriority> RolesWithPriority { get; set; }
    }

    public class CreateUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; }
    }

    public class UpdateUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class UserRoleDTO
    {
        public List<string> Roles { get; set; }
    }

    public class RoleDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateRoleDTO
    {
        public string Name { get; set; }
    }

    public class UpdateRoleDTO
    {
        public string Name { get; set; }
    }

    public class RolePermissionsDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<string> Permissions { get; set; }
        public Dictionary<string, string> VietnamesePermissions { get; set; }
    }
}
