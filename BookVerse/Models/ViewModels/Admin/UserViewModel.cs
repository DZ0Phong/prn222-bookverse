namespace BookVerse.Models.ViewModels.Admin
{
    public class UserListViewModel
    {
        public List<UserListItem> Users { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? FilterRoleId { get; set; }
        public bool? FilterIsActive { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 15;
    }

    public class UserListItem
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? RoleName { get; set; }
        public int? RoleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; }
    }
}
