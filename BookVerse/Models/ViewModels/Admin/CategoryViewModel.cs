using System.ComponentModel.DataAnnotations;

namespace BookVerse.Models.ViewModels.Admin
{
    public class CategoryListViewModel
    {
        public List<CategoryListItem> Categories { get; set; } = new();
        public CategoryFormViewModel Form { get; set; } = new();
    }

    public class CategoryListItem
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public int BookCount { get; set; }
    }

    public class CategoryFormViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
        public string? Description { get; set; }
    }
}
